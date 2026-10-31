using DI;
using System;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace NightCycle
{
    // Структура для настройки ивентов в инспекторе
    [Serializable]
    public struct LoopEvent
    {
        [Tooltip("На каком цикле должен сработать ивент")]
        public int loopNumber;
        public UnityEvent onLoopReached;
    }

    public class SeamlessTeleporter : MonoBehaviour
    {
        [Header("Teleportation Setup")]
        [Tooltip("Transform триггера в первой (начальной) комнате, куда мы телепортируем игрока")]
        [SerializeField] private Transform destinationTrigger;
        [SerializeField] private string playerTag = "Player";

        [Header("Loop Events")]
        [Tooltip("Список ивентов, которые будут вызываться на определенных кругах")]
        [SerializeField] private LoopEvent[] loopEvents;

        private int _currentLoopCount = 0;
        private IPlayerProvider _playerProvider;

        [Inject]
        public void Construct(IPlayerProvider playerProvider)
        {
            _playerProvider = playerProvider;
        }

        /*private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                TeleportPlayerSeamlessly();
                ProcessLoopEvents();
            }
        }*/

        public void TeleportPlayerSeamlessly()
        {
            var player = _playerProvider.CurrentPlayer;
            var rb = player.Rigidbody;
            CharacterController controller = null;

            if (rb != null)
            {
                controller = rb.gameObject.GetComponentInParent<CharacterController>();
            }

            //Узнаем локальную позицию игрока относительно ТЕКУЩЕГО триггера (из которого он выходит)
            Vector3 relativePosition = transform.InverseTransformPoint(player.Position);

            //Превращаем эту локальную позицию обратно в мировую, но уже относительно ЦЕЛЕВОГО триггера
            Vector3 targetPosition = destinationTrigger.TransformPoint(relativePosition);

            // То же самое проделываем с поворотом (если комнаты повернуты по-разному в пространстве)
            Quaternion relativeRotation = Quaternion.Inverse(transform.rotation) * player.Rigidbody.rotation;
            Quaternion targetRotation = destinationTrigger.rotation * relativeRotation;

            // Как и в PlayerStateBridg отключаем контроллер перед перемещением
            if (controller != null)
            {
                controller.enabled = false;
                controller.transform.position = targetPosition;
                controller.transform.rotation = targetRotation;
                Physics.SyncTransforms();
                controller.enabled = true;
            }
            else
            {
                player.Position = targetPosition;
                player.Rigidbody.rotation = targetRotation;
                Physics.SyncTransforms();
            }
        }

        public void ProcessLoopEvents()
        {
            _currentLoopCount++;
            Debug.Log($"[SeamlessTeleporter] Игрок прошел круг номер: {_currentLoopCount}");

            // Проверяем, есть ли ивент для текущего круга
            foreach (var loopEvent in loopEvents)
            {
                if (loopEvent.loopNumber == _currentLoopCount)
                {
                    loopEvent.onLoopReached?.Invoke();
                }
            }
        }

        // Метод для ручного сброса циклов
        public void ResetLoops()
        {
            _currentLoopCount = 0;
        }

        public void SetLoopNumber(int loopNumber)
        {
            _currentLoopCount = loopNumber;
        }
    }
}