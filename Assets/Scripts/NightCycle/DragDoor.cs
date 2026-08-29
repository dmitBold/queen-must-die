using Core;
using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class DragDoor : MonoBehaviour
    {
        [SerializeField] LayerMask doorLayer;
        [SerializeField] Sprite HandImage;
        [SerializeField] Sprite DragImage;
        [SerializeField] Sprite og;

        [SerializeField] float motorForce = 1500f;
        [SerializeField] float speedMultiplier = 400f;

        [SerializeField] private PlayerStateController playerStateController;


        Transform selectedDoor;
        HingeJoint joint;
        bool isDragging = false;
        float sideMultiplier = 1f;
        HUDController controller;
        Camera cam;

        //TEST
        LockDoor Lock;
        [SerializeField] Sprite LockImage;

        private AudioService _audioService;

        [Inject]
        public void Constructor(AudioService audioService, HUDController hudController)
        {
            _audioService = audioService;
            controller = hudController;
        }

        //TEST
        void Start()
        {
            controller = HUDController.instance;
            cam = Camera.main;
            //OgImage = HUDController.instance.CrosshairImage.sprite;
        }

        void Update()
        {
            if (isDragging)
            {
                HandleDragging();
                return;
            }

            RaycastHit hit;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 3f, doorLayer))
            {
                selectedDoor = hit.collider.transform;
                LockDoor lockDoor = selectedDoor.GetComponent<LockDoor>();

                if (lockDoor != null && lockDoor.enabled)
                {

                    controller.DoNormalSize();

                    controller.ChangeCrosshairImage(LockImage);

                    if (Input.GetMouseButtonDown(0))
                    {
                        if (lockDoor.LockSound != null)
                        {
                            _audioService.PlaySound(lockDoor.LockSound);
                        }
                    }
                }
                else
                {
                    controller.DoNormalSize();

                    controller.ChangeCrosshairImage(HandImage);

                    if (Input.GetMouseButtonDown(0))
                    {
                        StartDragging();
                    }
                }
            }
            else
            {
                controller.DoSmallSize();

                controller.ChangeCrosshairImage(controller.DefaultImage);
            }
        }

        private void StartDragging()
        {
            joint = selectedDoor.GetComponent<HingeJoint>();
            if (joint != null)
            {
                isDragging = true;
                joint.useMotor = true;
                playerStateController.SetMode(PlayerMode.DoorState);

                Vector3 doorToCam = cam.transform.position - selectedDoor.position;
                sideMultiplier = Mathf.Sign(Vector3.Dot(selectedDoor.forward, doorToCam));

                controller.DoNormalSize();

                controller.ChangeCrosshairImage(DragImage);
            }
        }

        private void HandleDragging()
        {
            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                joint.useMotor = false;
                joint = null;
                selectedDoor = null;
                playerStateController.SetMode(PlayerMode.FreeMovement);
                return;
            }

            if (joint != null)
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                float combinedInput = mouseX + mouseY;

                JointMotor motor = joint.motor;
                motor.force = motorForce;
                motor.targetVelocity = combinedInput * speedMultiplier * sideMultiplier;
                joint.motor = motor;
            }
        }
    }
}