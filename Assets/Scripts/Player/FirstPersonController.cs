using UnityEngine;
using Zenject;
using Unity.Cinemachine;
using Core; // Подключаем пространство имен с AudioService
using FMODUnity; // Для EventReference

namespace NightCycle
{
    public class FirstPersonController : MonoBehaviour
    {
        private const float GROUNDED_GRAVITY = -0.5f;

        [Header("Movement Speeds")]
        [SerializeField] private float walkSpeed = 3.0f;
        [SerializeField] private float sprintMultiplier = 2.0f;

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

        // --- НОВЫЕ ПАРАМЕТРЫ ШАГОВ ---
        [Header("Footsteps (FMOD)")]
        [SerializeField] private EventReference footstepEvent; // Позволит выбрать ивент через UI
        [SerializeField] private float walkStepInterval = 0.5f; // Секунд между шагами при ходьбе
        [SerializeField] private float sprintStepInterval = 0.3f; // Секунд между шагами при беге

        private float stepTimer; // Внутренний таймер
        // ------------------------------
        private bool wasMoving;

        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private CinemachineCamera mainCamera;
        [SerializeField] private PlayerStateController playerStateController;

        private PlayerInputManager playerInputHandler;
        private AudioService audioService; // Ссылка на наш сервис звуков

        [SerializeField] private CinemachineBasicMultiChannelPerlin cameraNoise;

        private Vector3 currentMovement;
        private float verticalRotation;

        private float CurrentSpeed => walkSpeed * (playerInputHandler.SprintTriggered ? sprintMultiplier : 1);

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

            // Сбрасываем таймер шагов в начале
            stepTimer = 0f;
            wasMoving = false;
        }

        private void InitializeCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
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
            HandleFootsteps(isMoving); // Вызов логики шагов
        }

        // --- ЛОГИКА ШАГОВ ---
        private void HandleFootsteps(bool isMoving)
        {
            if (!isMoving)
            {
                wasMoving = false;

                // Время кулдауна продолжает уменьшаться, даже когда игрок стоит.
                // Это предотвращает мгновенный повторный щелчок звука, если игрок спамит WASD.
                if (stepTimer > 0)
                {
                    stepTimer -= Time.deltaTime;
                }
                return;
            }

            // Если игрок только что начал движение и кулдаун с прошлого шага завершен
            if (!wasMoving && stepTimer <= 0f)
            {
                PlayFootstepSound();
                stepTimer = playerInputHandler.SprintTriggered ? sprintStepInterval : walkStepInterval;
            }
            else
            {
                stepTimer -= Time.deltaTime;

                if (stepTimer <= 0f)
                {
                    PlayFootstepSound();
                    stepTimer = playerInputHandler.SprintTriggered ? sprintStepInterval : walkStepInterval;
                }
            }

            wasMoving = true;
        }

        private void PlayFootstepSound()
        {
            if (audioService != null)
            {
                // Передаем координаты игрока для 3D звука
                audioService.PlayFMODEvent(footstepEvent, transform.position);
            }
        }
        // --------------------

        private void HandleCameraBobbing(bool isMoving)
        {
            if (cameraNoise == null) return;

            float targetAmplitude = idleAmplitude;
            float targetFrequency = idleFrequency;

            if (isMoving)
            {
                if (playerInputHandler.SprintTriggered)
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

                if (playerInputHandler.JumpTriggered)
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
            Vector2 rotationInput = playerInputHandler.RotationInput;
            float mouseXRotation = rotationInput.x * mouseSensitivity;
            float mouseYRotation = rotationInput.y * mouseSensitivity;

            ApplyHorizontalRotation(mouseXRotation);
            ApplyVerticalRotation(mouseYRotation);
        }
    }
}