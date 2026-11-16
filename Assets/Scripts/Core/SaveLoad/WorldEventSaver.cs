using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace NightCycle
{
    public class WorldEventSaver : MonoBehaviour
    {
        [Tooltip("Сгенерируется автоматически")]
        [SerializeField] private string _uniqueId;

        [Tooltip("Что сделать СРАЗУ при загрузке сцены, если этот ивент уже был активирован?")]
        [SerializeField] private UnityEvent onStateRestored;

        /// <summary>Вызвать onStateRestored кадром позже, когда все Start сцены уже отработали и не перетрут восстановленное состояние.</summary>
        [SerializeField] private bool _restoreNextFrame;

        private SaveSystem _saveSystem;

        [Inject]
        public void Construct(SaveSystem saveSystem)
        {
            _saveSystem = saveSystem;
        }

        private void Start()
        {
            if (_saveSystem.SessionTriggeredEvents.Contains(_uniqueId))
            {
                Debug.Log(_uniqueId);
                if (_restoreNextFrame) StartCoroutine(RestoreNextFrame());
                else onStateRestored.Invoke();
            }
            else
            {
                Debug.Log("NOTCONTAINED");
            }
        }

        private IEnumerator RestoreNextFrame()
        {
            yield return null;
            onStateRestored.Invoke();
        }

        public void SaveEventState()
        {
            if (string.IsNullOrEmpty(_uniqueId)) return;
            _saveSystem.RegisterEvent(_uniqueId);
        }

        public void ClearEventState()
        {
            if (string.IsNullOrEmpty(_uniqueId)) return;
            _saveSystem.RemoveEvent(_uniqueId);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_uniqueId))
            {
                _uniqueId = System.Guid.NewGuid().ToString();
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}