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
