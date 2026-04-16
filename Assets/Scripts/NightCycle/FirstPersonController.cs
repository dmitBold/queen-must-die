using UnityEngine;

namespace NightCycle
{
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Movement Speeds")]
        [SerializeField] private float walkSpeed = 3.0f;
        //[SerializeField] private float sprintMultiplier = 2.0f;


        [Header("Jump Parameters")]
        [SerializeField] private float jumpForce = 5.0f;
        [SerializeField] private float gravityMultiplier = 1.0f;


        [Header("Look Parameters")]
        [SerializeField] private float mouseSensitivity = 0.1f;
        [SerializeField] private float upDownLookRange = 80f;


        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private PlayerInputManager playerInputHandler;


        private Vector3 currentMovement;
        private float verticalRotation;
        private float CurrentSpeed => walkSpeed;


        // Start is called before the first frame update
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }


        // Update is called once per frame
        void Update()
        {
            if (PlayerStateController.Instance == null)
            {
                Debug.Log("PlayerStateController IS NULL!");
                return;
            }

            if (!PlayerStateController.Instance.CanMove())
            {
                //TEST
                //HandleJumping();
                HandleJumpingItemSelection();
                return;
            }

            HandleMovement();
            //TEST
            if (!PlayerStateController.Instance.CanRotate())
            {
                return;
            }
            //TEST
            HandleRotation();
        }

        //test
        private void HandleJumpingItemSelection()
        {
            currentMovement.x = 0;
            currentMovement.z = 0;

            if (characterController.isGrounded)
            {
                currentMovement.y = -0.5f;


                /*if (playerInputHandler.JumpTriggered)
            {
                currentMovement.y = jumpForce;
            }*/
            }
            else
            {
                currentMovement.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
            }
            characterController.Move(currentMovement * Time.deltaTime);
        }
        //test

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
                currentMovement.y = -0.5f;


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
            currentMovement.x = worldDirection.x * CurrentSpeed;
            currentMovement.z = worldDirection.z * CurrentSpeed;


            HandleJumping();
            //TEST
            characterController.Move(currentMovement * Time.deltaTime);
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
            float mouseXRotation = playerInputHandler.RotationInput.x * mouseSensitivity;
            float mouseYRotation = playerInputHandler.RotationInput.y * mouseSensitivity;


            ApplyHorizontalRotation(mouseXRotation);
            ApplyVerticalRotation(mouseYRotation);
        }
    }
}
