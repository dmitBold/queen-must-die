using UnityEngine;
using Unity.Cinemachine;
using NightCycle;

namespace NightCycle.Puzzles
{
    public abstract class FocusPuzzle : BasePuzzle, IFocusable
    {
        [Header("Focus Settings")]
        [SerializeField] protected CinemachineCamera puzzleCamera; // Камера паззла
        protected HUDController controller;


        protected bool isFocused;

        public virtual void OnEnterFocus()
        {
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