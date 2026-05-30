using UnityEngine;
using Zenject;
using Unity.Cinemachine;

namespace NightCycle
{
    public class FirstPersonController : MonoBehaviour
    {
        private const float GROUNDED_GRAVITY = -0.5f;

        [Header("Movement Speeds")] 
        [SerializeField] private float walkSpeed = 3.0f;
        [SerializeField] private float sprintMultiplier = 2.0f;

        [Header("Jump Parameters")] [SerializeField]
        private float jumpForce = 5.0f;

        [SerializeField] private float gravityMultiplier = 1.0f;

        [Header("Look Parameters")] [SerializeField]
        private float mouseSensitivity = 0.1f;

        [SerializeField] private float upDownLookRange = 80f;

        [Header("References")] [SerializeField]
        private CharacterController characterController;

        [SerializeField] private CinemachineCamera mainCamera;
        [SerializeField] private PlayerStateController playerStateController;
        

        private PlayerInputManager playerInputHandler;

        private Vector3 currentMovement;
        private float verticalRotation;
        //private float CurrentSpeed => walkSpeed;
        private float CurrentSpeed => walkSpeed * (playerInputHandler.SprintTriggered ? sprintMultiplier : 1);

        [Inject]
        private void Construct(PlayerInputManager playerInputHandler)
        {
            this.playerInputHandler = playerInputHandler;
            this.playerStateController = playerStateController;
        }

        private void Start()
        {
            InitializeCursor();
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
                return;
            }

            HandleMovement();

            if (playerStateController.CanRotate())
            {
                HandleRotation();
            }
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