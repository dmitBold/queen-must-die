using UnityEngine;

namespace NightCycle.Puzzles
{
    public class RotationLever : MonoBehaviour
    {
        [Header("Targets")]
        [Tooltip("Предметы, которые повернутся при нажатии")]
        [SerializeField] private RotatableItem[] targetItems;

        [Header("Animation & Spam Protection")]
        [SerializeField] private Animator animator;
        [SerializeField] private string animationTrigger = "Pull";
        [SerializeField] private float interactCooldown = 1.0f;

        private float lastInteractTime = -9999f;

        public void TriggerLever()
        {
            // Защита от спама по времени
            if (Time.time - lastInteractTime < interactCooldown) return;

            // Защита от взаимодействия, пока предметы еще крутятся
            foreach (var item in targetItems)
            {
                if (item.IsRotating) return;
            }

            lastInteractTime = Time.time;

            // Проигрываем анимацию рычага, если есть
            if (animator != null && !string.IsNullOrEmpty(animationTrigger))
            {
                animator.SetTrigger(animationTrigger);
            }

            // Запускаем поворот
            foreach (var item in targetItems)
            {
                item.Rotate();
            }
        }
    }
}