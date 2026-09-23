using UnityEngine;
using Zenject;
using Unity.Cinemachine;
using Core;
using FMODUnity;

namespace NightCycle
{
    public class FirstPersonController : MonoBehaviour
    {
        private const float GROUNDED_GRAVITY = -0.5f;

        [Header("Movement Speeds")]
        [SerializeField] private float walkSpeed = 3.0f;
        [SerializeField] private float sprintMultiplier = 2.0f;

        [Header("Crouch Parameters")]
        [SerializeField] private float crouchSpeedMultiplier = 0.5f; // Уменьшение скорости при приседе
        [SerializeField] private float crouchHeightRatio = 1.5f; // Во сколько раз уменьшается высота
        [SerializeField] private float crouchCameraTransitionSpeed = 10f; // Скорость приседания камеры
        [SerializeField] private LayerMask ceilingCheckMask = ~0; // Слои для проверки препятствий сверху (убери слой Player/IgnoreRaycast)

        [Header("Jump Parameters")]
        [SerializeField] private float jumpForce = 5.0f;
        [SerializeField] private float gravityMultiplier = 1.0f;

        [Header("Look Parameters")]
        [SerializeField] private float mouseSensitivity = 0.1f;
        [SerializeField] private float upDownLookRange = 80f;

        [Header("Head Bobbing (Perlin Noise)")]
        [SerializeField] private float noiseTransitionSpeed = 5f;
        [SerializeField] private float idleAmplitude = 0.1f;
        [SerializeField] private float idleFrequency = 0.5f;
        [SerializeField] private float walkAmplitude = 0.5f;
        [SerializeField] private float walkFrequency = 1.5f;
        [SerializeField] private float sprintAmplitude = 1.0f;
        [SerializeField] private float sprintFrequency = 2.5f;

        [Header("Footsteps (FMOD)")]
        [SerializeField] private EventReference footstepEvent;
        [SerializeField] private float walkStepInterval = 0.5f;
        [SerializeField] private float sprintStepInterval = 0.3f;
        [SerializeField] private float crouchStepInterval = 0.7f; // Интервал шагов в приседе

        private float stepTimer;
        private bool wasMoving;

        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private CinemachineCamera mainCamera;
        [SerializeField] private PlayerStateController playerStateController;

        private PlayerInputManager playerInputHandler;
        private AudioService audioService;

        [SerializeField] private CinemachineBasicMultiChannelPerlin cameraNoise;

        private Vector3 currentMovement;
        private float verticalRotation;

        // Состояния и параметры приседа
        private bool isCrouching;
        private float originalControllerHeight;
        private Vector3 originalControllerCenter;
        private float originalCameraLocalY;

        private float CurrentSpeed
        {
            get
            {
                if (isCrouching) return walkSpeed * crouchSpeedMultiplier;
                return walkSpeed * (playerInputHandler.SprintTriggered ? sprintMultiplier : 1);
            }
        }

        [Inject]
        private void Construct(PlayerInputManager playerInputHandler, AudioService audioService)
        {
            this.playerInputHandler = playerInputHandler;
            this.playerStateController = playerStateController;
            this.audioService = audioService;
        }

        private void Start()
        {
            InitializeCursor();
            stepTimer = 0f;
            wasMoving = false;

            // Запоминаем изначальные параметры для приседа
            originalControllerHeight = characterController.height;
            originalControllerCenter = characterController.center;
            if (mainCamera != null)
                originalCameraLocalY = mainCamera.transform.localPosition.y;
        }

        private void InitializeCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            // Обновляем камеру и присед даже если движение заблокировано, чтобы не прерывать анимацию камеры
            HandleCrouching();

            if (!playerStateController.CanMove())
            {
                HandleJumpingItemSelection();
                HandleCameraBobbing(isMoving: false);
                return;
            }

            HandleMovement();

            if (playerStateController.CanRotate())
            {
                HandleRotation();
            }

            bool isMoving = characterController.isGrounded && playerInputHandler.MovementInput.sqrMagnitude > 0.01f;
            HandleCameraBobbing(isMoving);
            HandleFootsteps(isMoving);
        }

        private void HandleCrouching()
        {
            // Логика перехода в присед и обратно
            if (playerInputHandler.CrouchTriggered && !isCrouching)
            {
                isCrouching = true;
            }
            else if (!playerInputHandler.CrouchTriggered && isCrouching)
            {
                // Пытаемся встать. Если сверху нет потолка - встаем.
                if (!IsCeilingAbove())
                {
                    isCrouching = false;
                }
            }

            // Целевые значения для физики
            float targetHeight = isCrouching ? (originalControllerHeight / crouchHeightRatio) : originalControllerHeight;
            Vector3 targetCenter = isCrouching ? (originalControllerCenter / crouchHeightRatio) : originalControllerCenter;

            // Мгновенно меняем коллайдер, чтобы избежать застреваний
            if (Mathf.Abs(characterController.height - targetHeight) > 0.001f)
            {
                characterController.height = targetHeight;
                characterController.center = targetCenter;
            }

            // Плавно интерполируем позицию камеры
            if (mainCamera != null)
            {
                float targetCameraY = isCrouching ? originalCameraLocalY - (originalControllerHeight - targetHeight) : originalCameraLocalY;
                Vector3 camPos = mainCamera.transform.localPosition;
                camPos.y = Mathf.Lerp(camPos.y, targetCameraY, Time.deltaTime * crouchCameraTransitionSpeed);
                mainCamera.transform.localPosition = camPos;
            }
        }

        private bool IsCeilingAbove()
        {
            // Рассчитываем текущую высоту (высоту в приседе)
            float currentCrouchHeight = originalControllerHeight / crouchHeightRatio;

            // Центр сферы начинается прямо на макушке игрока в приседе
            Vector3 origin = transform.position + (Vector3.up * currentCrouchHeight);
            float castDistance = originalControllerHeight - currentCrouchHeight;

            // Пускаем сферу (радиусом с игрока) вверх на разницу в высоте с небольшим запасом (0.1f)
            return Physics.SphereCast(origin, characterController.radius, Vector3.up, out _, castDistance + 0.1f, ceilingCheckMask);
        }

        private void HandleFootsteps(bool isMoving)
        {
            if (!isMoving)
            {
                wasMoving = false;
                if (stepTimer > 0) stepTimer -= Time.deltaTime;
                return;
            }

            float currentStepInterval = isCrouching ? crouchStepInterval : (playerInputHandler.SprintTriggered ? sprintStepInterval : walkStepInterval);

            if (!wasMoving && stepTimer <= 0f)
            {
                PlayFootstepSound();
                stepTimer = currentStepInterval;
            }
            else
            {
                stepTimer -= Time.deltaTime;
                if (stepTimer <= 0f)
                {
                    PlayFootstepSound();
                    stepTimer = currentStepInterval;
                }
            }

            wasMoving = true;
        }

        private void PlayFootstepSound()
        {
            if (audioService != null)
            {
                audioService.PlayFMODEvent(footstepEvent, transform.position);
            }
        }

        private void HandleCameraBobbing(bool isMoving)
        {
            if (cameraNoise == null) return;

            float targetAmplitude = idleAmplitude;
            float targetFrequency = idleFrequency;

            if (isMoving)
            {
                // Если игрок присел, отключаем спринт-боббинг
                if (playerInputHandler.SprintTriggered && !isCrouching)
                {
                    targetAmplitude = sprintAmplitude;
                    targetFrequency = sprintFrequency;
                }
                else
                {
                    targetAmplitude = walkAmplitude;
                    targetFrequency = walkFrequency;
                }
            }

            cameraNoise.AmplitudeGain = Mathf.Lerp(cameraNoise.AmplitudeGain, targetAmplitude, Time.deltaTime * noiseTransitionSpeed);
            cameraNoise.FrequencyGain = Mathf.Lerp(cameraNoise.FrequencyGain, targetFrequency, Time.deltaTime * noiseTransitionSpeed);
        }

        private void HandleJumpingItemSelection()
        {
            ResetHorizontalMovement();
            ApplyGravityOnly();
            characterController.Move(currentMovement * Time.deltaTime);
        }

        private void ResetHorizontalMovement()
        {
            currentMovement.x = 0;
            currentMovement.z = 0;
        }

        private void ApplyGravityOnly()
        {
            if (characterController.isGrounded)
            {
                currentMovement.y = GROUNDED_GRAVITY;
            }
            else
            {
                currentMovement.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
            }
        }

        private Vector3 CalculateWorldDirection()
        {
            Vector3 inputDirection = new Vector3(playerInputHandler.MovementInput.x, 0f, playerInputHandler.MovementInput.y);
            Vector3 worldDirection = transform.TransformDirection(inputDirection);
            return worldDirection.normalized;
        }

        private void HandleJumping()
        {
            if (characterController.isGrounded)
            {
                currentMovement.y = GROUNDED_GRAVITY;

                // Блокируем прыжок, если игрок сидит
                if (playerInputHandler.JumpTriggered && !isCrouching)
                {
                    currentMovement.y = jumpForce;
                }
            }
            else
            {
                currentMovement.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
            }
        }

        private void HandleMovement()
        {
            Vector3 worldDirection = CalculateWorldDirection();
            SetHorizontalMovement(worldDirection);
            HandleJumping();
            characterController.Move(currentMovement * Time.deltaTime);
        }

        private void SetHorizontalMovement(Vector3 worldDirection)
        {
            currentMovement.x = worldDirection.x * CurrentSpeed;
            currentMovement.z = worldDirection.z * CurrentSpeed;
        }

        private void ApplyHorizontalRotation(float rotationAmount)
        {
            transform.Rotate(0, rotationAmount, 0);
        }

        private void ApplyVerticalRotation(float rotationAmount)
        {
            verticalRotation = Mathf.Clamp(verticalRotation - rotationAmount, -upDownLookRange, upDownLookRange);
            mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        }

        private void HandleRotation()
        {
            if (Time.timeScale == 0f)
                return;

            Vector2 rotationInput = playerInputHandler.RotationInput;
            float mouseXRotation = rotationInput.x * mouseSensitivity;
            float mouseYRotation = rotationInput.y * mouseSensitivity;

            ApplyHorizontalRotation(mouseXRotation);
            ApplyVerticalRotation(mouseYRotation);
        }

        //test
        private void OnEnable()
        {
            SettingsMenu.OnSensitivityChanged += UpdateSensitivity;
            mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        }

        private void OnDisable()
        {
            SettingsMenu.OnSensitivityChanged -= UpdateSensitivity;
        }

        private void UpdateSensitivity(float newSensitivity)
        {
            mouseSensitivity = newSensitivity;
        }
        //test
    }
}