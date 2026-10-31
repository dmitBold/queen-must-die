using UnityEngine;
using UnityEngine.Events;

namespace NightCycle
{
    /// <summary>
    /// Компонент на префабе замка для работы в AssemblyController.
    /// Наследуется от InteractableView для унификации с AssemblyView.
    /// </summary>
    public class LockView : InteractableView
    {
        [Header("Lock Settings")]
        [SerializeField] private CodeLock codeLock;

        [Header("Lock Events")]
        public UnityEvent onLockUnlocked;

        private bool isLockUnlocked = false;

        public CodeLock CodeLock => codeLock;
        public bool IsLockUnlocked => isLockUnlocked;
        public override bool IsCompleted => isLockUnlocked;

        public override void OnEnterFocus()
        {
            base.OnEnterFocus();
            
            // Активируем кодовый замок при входе в фокус
            if (codeLock != null)
            {
                codeLock.Enter();
            }
        }

        public override void OnExitFocus()
        {
            base.OnExitFocus();
            
            // Деактивируем кодовый замок при выходе из фокуса
            if (codeLock != null)
            {
                codeLock.Exit();
            }
        }

        private void Start()
        {
            // Подписываемся на событие разблокировки замка
            if (codeLock != null)
            {
                codeLock.OnUnlocked.AddListener(HandleLockUnlocked);
            }
        }

        private void OnDestroy()
        {
            // Отписываемся от событий при уничтожении
            if (codeLock != null)
            {
                codeLock.OnUnlocked.RemoveListener(HandleLockUnlocked);
            }
        }

        private void HandleLockUnlocked()
        {
            if (isLockUnlocked)
                return;

            isLockUnlocked = true;
            

            onLockUnlocked?.Invoke();
            OnInteractionCompleted();
            
            Debug.Log("<color=green>Lock unlocked successfully!</color>");
        }
    }
}
