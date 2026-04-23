using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NightCycle
{
    /// <summary>
    /// Компонент на префабе сборочного объекта.
    /// Содержит корневой трансформ для вращения и список сокетов.
    /// </summary>
    public class AssemblyView : MonoBehaviour
    {
        [SerializeField] private Transform rotationRoot;
        [SerializeField] private List<AssemblySocket> sockets;

        public UnityEvent onAssemblyCompleted;

        public Transform RotationRoot => rotationRoot;
        public IReadOnlyList<AssemblySocket> Sockets => sockets;
    }
}
