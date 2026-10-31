using Core;
using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class PlayerInteractor : MonoBehaviour
    {
        // Объедини doorLayer и LockLayer в один общий слой в Unity
        [SerializeField] LayerMask interactableLayer;

        [SerializeField] Sprite HandImage;
        [SerializeField] Sprite DragImage;
        [SerializeField] Sprite LockImage;

        [SerializeField] float motorForce = 1500f;
        [SerializeField] float speedMultiplier = 400f;

        [SerializeField] private PlayerStateController playerStateController;

        Transform currentTarget;
        HingeJoint currentJoint;
        bool isDragging = false;
        float sideMultiplier = 1f;

        HUDController controller;
        Camera cam;
        private AudioService _audioService;

        [Inject]
        public void Constructor(AudioService audioService, HUDController hudController)
        {
            _audioService = audioService;
            controller = hudController;
        }

        void Start()
        {
            cam = Camera.main;
        }

        void Update()
        {
            if (isDragging)
            {
                HandleDragging();
                return;
            }

            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, 3f, interactableLayer))
            {
                currentTarget = hit.collider.transform;

                // 1. Сначала проверяем, не заблокирован ли объект
                if (currentTarget.TryGetComponent(out LockObject lockObj) && lockObj.isLocked && lockObj.enabled)
                {
                    SetCursor(LockImage, true);

                    if (Input.GetMouseButtonDown(0) && lockObj.LockSound != null)
                    {
                        _audioService.PlaySound(lockObj.LockSound);
                    }
                    return; // Если объект закрыт, прерываем выполнение (рука не появится)
                }

                // 2. Если объект не заблокирован (или замка вообще нет), проверяем, дверь ли это
                // Можно проверять по наличию HingeJoint, как у тебя, или создать пустой скрипт-метку DoorComponent
                if (currentTarget.TryGetComponent(out HingeJoint joint))
                {
                    SetCursor(HandImage, true);

                    if (Input.GetMouseButtonDown(0))
                    {
                        StartDragging(joint);
                    }
                    return;
                }

                // 3. Здесь в будущем можно добавить проверки на другие объекты (например, Chest chest)
                // if (currentTarget.TryGetComponent(out Chest chest)) { ... }

                // Если объект на слое interactableLayer, но скриптов нет:
                ResetCursor();
            }
            else
            {
                ResetCursor();
            }
        }

        private void SetCursor(Sprite icon, bool isNormalSize)
        {
            if (isNormalSize) controller.DoNormalSize();
            else controller.DoSmallSize();

            controller.ChangeCrosshairImage(icon);
        }

        private void ResetCursor()
        {
            currentTarget = null;
            SetCursor(controller.DefaultImage, false);
        }

        private void StartDragging(HingeJoint joint)
        {
            currentJoint = joint;
            isDragging = true;
            currentJoint.useMotor = true;
            playerStateController.SetMode(PlayerMode.DoorState);

            Vector3 doorToCam = cam.transform.position - currentTarget.position;
            sideMultiplier = Mathf.Sign(Vector3.Dot(currentTarget.forward, doorToCam));

            SetCursor(DragImage, true);
        }

        private void HandleDragging()
        {
            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                currentJoint.useMotor = false;
                currentJoint = null;
                currentTarget = null;
                playerStateController.SetMode(PlayerMode.FreeMovement);
                return;
            }

            if (currentJoint != null)
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                float combinedInput = mouseX + mouseY;

                JointMotor motor = currentJoint.motor;
                motor.force = motorForce;
                motor.targetVelocity = combinedInput * speedMultiplier * sideMultiplier;
                currentJoint.motor = motor;
            }
        }
    }
}