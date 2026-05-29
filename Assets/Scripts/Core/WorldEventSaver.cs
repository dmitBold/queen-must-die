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
                onStateRestored.Invoke();
            }
            else
            {
                Debug.Log("NOTCONTAINED");
            }
        }

        public void SaveEventState()
        {
            if (string.IsNullOrEmpty(_uniqueId)) return;
            _saveSystem.RegisterEvent(_uniqueId);
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