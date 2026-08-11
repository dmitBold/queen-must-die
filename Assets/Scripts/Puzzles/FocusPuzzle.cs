using NightCycle;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

namespace NightCycle.Puzzles
{
    public abstract class FocusPuzzle : BasePuzzle, IFocusable
    {
        [Header("Focus Settings")]
        [SerializeField] protected CinemachineCamera puzzleCamera; // Камера паззла
        protected HUDController controller;
        protected PlayerInteraction playerInteraction;
        protected PlayerFlashlight flashlight;
        public BoxCollider puzzleCollider;

        [Inject]
        private void Construct(PlayerFlashlight Flashlight, PlayerInteraction playerInteraction)
        {
            this.playerInteraction = playerInteraction;
            this.flashlight = Flashlight;
        }


        protected bool isFocused;

        public virtual void OnEnterFocus()
        {
            if(puzzleCollider != null)
            {
                puzzleCollider.enabled = false;
            }

            if (flashlight != null)
            {
                flashlight.TurnOFF();
            }

            isFocused = true;
            puzzleCamera.gameObject.SetActive(true); // Cinemachine сделает плавный blend

            if (controller != null)
            {
                controller.DisableInteractionText();
                controller.SetCrosshairActivity(false);
            }
            // Включаем курсор
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;

        }

        public virtual void OnExitFocus()
        {
            if (puzzleCollider != null)
            {
                puzzleCollider.enabled = true;
            }

            if (flashlight != null)
            {
                flashlight.TurnOn();
            }

            isFocused = false;
            puzzleCamera.gameObject.SetActive(false);

            if (controller != null)
            {
                controller.SetCrosshairActivity(true);
            }
            // Выключаем курсор
            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;
        }
    }
}