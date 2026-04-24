using UnityEngine;
using UnityEngine.Events;

namespace NightCycle
{
    /// <summary>
    /// Базовый класс для всех интерактивных объектов, которые спавнятся в AssemblyController.
    /// Определяет общий интерфейс для вращения и событий.
    /// </summary>
    public abstract class InteractableView : MonoBehaviour
    {
        [Header("Base Settings")] [SerializeField]
        protected Transform rotationRoot;
        public string HintText;

        [Header("Events")] public UnityEvent onInteractionCompleted;

        public Transform RotationRoot => rotationRoot;

        public virtual bool IsCompleted => false;

        /// <summary>
        /// Вызывается при входе в фокусный режим
        /// </summary>
        public virtual void OnEnterFocus()
        {
            // Базовая реализация может быть пустой
        }

        /// <summary>
        /// Вызывается при выходе из фокусного режима
        /// </summary>
        public virtual void OnExitFocus()
        {
            // Базовая реализация может быть пустой
        }

        /// <summary>
        /// Вызывается при завершении взаимодействия с объектом
        /// </summary>
        protected virtual void OnInteractionCompleted()
        {
            onInteractionCompleted?.Invoke();
        }

        /// <summary>
        /// Обновление состояния объекта в режиме фокуса
        /// </summary>
        public virtual void UpdateFocusState()
        {
            // Базовая реализация может быть пустой
        }
    }
}