using UnityEngine;
using Zenject;
using Unity.Cinemachine;
using Core;
using FMODUnity;
using FMOD.Studio;

namespace NightCycle
{
    public class FirstPersonController : MonoBehaviour
    {
        private const float GROUNDED_GRAVITY = -0.5f;
        private const float CEILING_CHECK_INSET = 0.05f;

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
        /// <summary>Сила тряски при ходьбе в приседе. Ниже ходьбы: шаг в приседе мягче.</summary>
        [SerializeField] private float crouchAmplitude = 0.7f;
        /// <summary>Скорость тряски при ходьбе в приседе. Подбирается под crouchStepInterval, чтобы покачивание совпадало с шагами.</summary>
        [SerializeField] private float crouchFrequency = 0.7f;

        [Header("Footsteps (FMOD)")]
        [SerializeField] private EventInstance footstepInstance;
        [SerializeField] private EventReference footstepEvent;
        [SerializeField] private float walkStepInterval = 0.5f;
        [SerializeField] private float sprintStepInterval = 0.3f;
        [SerializeField] private float crouchStepInterval = 0.7f; // Интервал шагов в приседе

        RaycastHit rh; //контейнер для типа поверхности
        public LayerMask lm;
        float Material = 1f;

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

        // Буфер для проверки места над головой при вставании, чтобы не выделять память каждый кадр.
        private readonly Collider[] _ceilingHits = new Collider[16];

        // Прыжок срабатывает на нажатие, а не пока клавиша зажата: иначе при удержании игрок прыгает при каждом приземлении.
        private bool _jumpHeld;

        public float SpeedMultiplier { get; set; } = 1f;

        /// <summary>Сидит ли игрок: присед держится и под низким потолком, даже если клавиша уже отпущена.</summary>
        public bool IsCrouching => isCrouching;

        /// <summary>Камера игрока (Cinemachine). Пока игрок не может двигаться, контроллер её не поворачивает.</summary>
        public Transform View => mainCamera.transform;

        private float CurrentSpeed
        {
            get
            {
                if (isCrouching) return walkSpeed * crouchSpeedMultiplier * SpeedMultiplier;
                return walkSpeed * (playerInputHandler.SprintTriggered ? sprintMultiplier : 1) * SpeedMultiplier;
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
            // Низ капсулы остаётся на месте: центр опускается на половину потерянной высоты.
            // У префаба центр (0,0,0), поэтому простое деление центра сжимало капсулу к середине,
            // и при вставании она уходила под пол — в углах игрок застревал.
            Vector3 targetCenter = originalControllerCenter + Vector3.down * ((originalControllerHeight - targetHeight) * 0.5f);

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
            // Проверяем объём, который добавится сверху при вставании: от верхней сферы сидячей капсулы до верхней сферы стоячей.
            // Радиус чуть меньше, чтобы стены, к которым игрок прижат в углу, не считались потолком.
            float radius = characterController.radius;
            Vector3 bottom = transform.TransformPoint(originalControllerCenter) + Vector3.down * (originalControllerHeight * 0.5f);
            Vector3 crouchTopSphere = bottom + Vector3.up * (originalControllerHeight / crouchHeightRatio - radius);
            Vector3 standTopSphere = bottom + Vector3.up * (originalControllerHeight - radius);

            int count = Physics.OverlapCapsuleNonAlloc(crouchTopSphere, standTopSphere, radius - CEILING_CHECK_INSET, _ceilingHits, ceilingCheckMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                // Собственные коллайдеры игрока потолком не считаются.
                if (!_ceilingHits[i].transform.IsChildOf(transform))
                    return true;
            }
            return false;
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
                MaterialCheck();
                PlayFootstepSound();
                stepTimer = currentStepInterval;
            }
            else
            {
                stepTimer -= Time.deltaTime;
                if (stepTimer <= 0f)
                {
                    MaterialCheck();
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
                footstepInstance = RuntimeManager.CreateInstance(footstepEvent);
                footstepInstance.setParameterByName("Surface", Material);
                //Паша, 1/0 на параметр для тебя, когда приседаем - 1, иначе 0. Сейчас для теста когда приседаем - 2f на Surface
                float CrouchParam = IsCrouching ? 1f : 0f;
                if (IsCrouching) { footstepInstance.setParameterByName("Surface", 2f); }//Это для теста. тут подставишь нужный параметр
                footstepInstance.start();
                footstepInstance.release();
                // audioService.PlayFMODEvent(footstepEvent, transform.position);
            }
        }

        //Чекаем материл поверхности
        void MaterialCheck()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out rh, 1.2f, lm))
            {
                Debug.Log(rh.collider.tag);
                if (rh.collider.tag == "Wood") Material = 1f;
                else if (rh.collider.tag == "Cobblestone") Material = 2f;
                else if (rh.collider.tag == "Cobblestone_Heels") Material = 3f;
                else if (rh.collider.tag == "Carpet") Material = 4f;
                else Material = 0f;
            }
        }


        private void HandleCameraBobbing(bool isMoving)
        {
            if (cameraNoise == null) return;

            float targetAmplitude = idleAmplitude;
            float targetFrequency = idleFrequency;

            if (isMoving)
            {
                // В приседе спринта нет, а шаги реже: своя, более мягкая тряска
                if (isCrouching)
                {
                    targetAmplitude = crouchAmplitude;
                    targetFrequency = crouchFrequency;
                }
                else if (playerInputHandler.SprintTriggered)
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
            bool jumpPressed = playerInputHandler.JumpTriggered && !_jumpHeld;
            _jumpHeld = playerInputHandler.JumpTriggered;

            if (characterController.isGrounded)
            {
                currentMovement.y = GroundedVerticalSpeed();

                // Блокируем прыжок, если игрок сидит
                if (jumpPressed && !isCrouching)
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
            bool wasGrounded = characterController.isGrounded;
            HandleJumping();
            CollisionFlags flags = characterController.Move(currentMovement * Time.deltaTime);

            // Прижим к склону не должен разгонять падение с обрыва: сошёл с края — падение начинается с места.
            if (wasGrounded && !characterController.isGrounded && currentMovement.y < 0f)
                currentMovement.y = 0f;

            // Удар головой гасит подъём: иначе скорость прыжка держит игрока прижатым к потолку или балке, пока её не съест гравитация.
            if ((flags & CollisionFlags.Above) != 0 && currentMovement.y > 0f)
                currentMovement.y = 0f;
        }

        // На спуске слабый GROUNDED_GRAVITY не успевает за склоном: CharacterController на кадр отрывается от земли,
        // пропадают шаги и тряска камеры, игрок будто скользит, а доски и ульи не засчитывают бег. Поэтому на земле
        // прижимаем его со скоростью спуска по склону предельной крутизны. Отдельный Move для прижима не годится:
        // CharacterController.velocity берётся из последнего Move, и горизонтальная скорость для доски и улья обнулялась бы.
        private float GroundedVerticalSpeed()
        {
            float horizontalSpeed = new Vector2(currentMovement.x, currentMovement.z).magnitude;
            return Mathf.Min(GROUNDED_GRAVITY, -horizontalSpeed * Mathf.Tan(characterController.slopeLimit * Mathf.Deg2Rad));
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

        /// <summary>
        /// Turns the player to look along a world direction: body yaw plus camera pitch.
        /// Mouse look continues from the new orientation.
        /// </summary>
        public void LookAlong(Vector3 worldDirection)
        {
            if (worldDirection.sqrMagnitude < 0.0001f) return;

            Vector3 dir = worldDirection.normalized;
            float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            float pitch = -Mathf.Asin(Mathf.Clamp(dir.y, -1f, 1f)) * Mathf.Rad2Deg;
            verticalRotation = Mathf.Clamp(pitch, -upDownLookRange, upDownLookRange);
            mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        }

        /// <summary>
        /// Places the player so that the player camera sees exactly what the reference
        /// transform sees: same view direction and same camera position.
        /// Used to hand control back after a cutscene without a camera jump.
        /// </summary>
        public void MatchView(Transform reference)
        {
            if (reference == null) return;

            LookAlong(reference.forward);

            // Offset from the body to the camera with the new orientation applied.
            Vector3 cameraOffset = mainCamera.transform.position - transform.position;
            Vector3 targetBodyPosition = reference.position - cameraOffset;

            bool controllerWasEnabled = characterController.enabled;
            characterController.enabled = false;
            transform.position = targetBodyPosition;
            Physics.SyncTransforms();
            characterController.enabled = controllerWasEnabled;

            currentMovement = Vector3.zero;
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