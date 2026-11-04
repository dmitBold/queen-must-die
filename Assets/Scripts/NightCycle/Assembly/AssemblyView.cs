using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NightCycle
{
    /// <summary>
    /// Компонент на префабе сборочного объекта.
    /// Содержит корневой трансформ для вращения и список сокетов.
    /// Наследуется от InteractableView для унификации с другими интерактивными объектами.
    /// </summary>
    public class AssemblyView : InteractableView
    {
        [Header("Assembly Settings")]
        [SerializeField] private List<AssemblySocket> sockets;

        [Header("Assembly Events")]
        public UnityEvent onAssemblyCompleted;

        private bool isAssemblyCompleted = false;

        public IReadOnlyList<AssemblySocket> Sockets => sockets;
        public bool IsAssemblyCompleted => isAssemblyCompleted;
        public override bool IsCompleted => isAssemblyCompleted;

        private void Awake()
        {
            if (sockets == null)
                return;

            // Сокет сообщает о завершении заполнения сам, поэтому проверку завершения сборки ведёт вью, а не UI.
            foreach (var socket in sockets)
            {
                if (socket != null)
                    socket.OnFilled.AddListener(HandleSocketFilled);
            }
        }

        private void OnDestroy()
        {
            if (sockets == null)
                return;

            foreach (var socket in sockets)
            {
                if (socket != null)
                    socket.OnFilled.RemoveListener(HandleSocketFilled);
            }
        }

        private void HandleSocketFilled()
        {
            CheckSocketsCompletion();
        }

        /// <summary>
        /// Мгновенно доводит до конца все идущие анимации заполнения сокетов.
        /// Если это были последние сокеты, сборка завершится синхронно через OnFilled.
        /// </summary>
        public void CompletePendingFills()
        {
            if (sockets == null)
                return;

            foreach (var socket in sockets)
            {
                if (socket != null)
                    socket.CompleteImmediately();
            }
        }

        /// <summary>
        /// Истина, если указанный сокет пуст, а все остальные уже заполнены или заполняются.
        /// Вставка в такой сокет приводит к завершению сборки.
        /// </summary>
        public bool IsLastEmptySocket(AssemblySocket target)
        {
            if (target == null || !target.IsEmpty || sockets == null)
                return false;

            foreach (var socket in sockets)
            {
                if (socket != null && socket != target && socket.IsEmpty)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Сбрасывает вью для новой сессии сборки, если она осталась завершённой от предыдущей.
        /// Частично заполненную вью не трогает, чтобы не потерять прогресс.
        /// </summary>
        public void ResetIfFinished()
        {
            if (sockets == null || sockets.Count == 0)
                return;

            bool allFilled = true;
            foreach (var socket in sockets)
            {
                if (socket != null && !socket.IsFilled)
                {
                    allFilled = false;
                    break;
                }
            }

            if (!isAssemblyCompleted && !allFilled)
                return;

            foreach (var socket in sockets)
            {
                if (socket != null)
                    socket.ResetState();
            }

            isAssemblyCompleted = false;
        }

        /// <summary>
        /// Проверяет, все ли сокеты заполнены
        /// </summary>
        public bool CheckSocketsCompletion()
        {
            if (sockets == null || sockets.Count == 0)
                return false;

            foreach (var socket in sockets)
            {
                if (!socket.IsFilled)
                    return false;
            }

            if (!isAssemblyCompleted)
            {
                isAssemblyCompleted = true;
                onAssemblyCompleted?.Invoke();
                OnInteractionCompleted();
            }

            return true;
        }

        /// <summary>
        /// Получает незаполненные сокеты
        /// </summary>
        public List<AssemblySocket> GetEmptySockets()
        {
            var emptySockets = new List<AssemblySocket>();
            
            if (sockets == null)
                return emptySockets;

            foreach (var socket in sockets)
            {
                if (!socket.IsFilled)
                    emptySockets.Add(socket);
            }

            return emptySockets;
        }
    }
}
