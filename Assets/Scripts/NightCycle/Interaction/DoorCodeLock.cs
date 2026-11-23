using UnityEngine;

namespace NightCycle
{
    /// <summary>
    /// Кодовый замок, запирающий дверь. Когда замок решён (AssemblyInteractable), отпирает дверь,
    /// включает комнату за ней, гасит подсветку и растворяет модель замка.
    /// Факт открытия хранит WorldEventSaver: его onStateRestored нужно направить в RestoreUnlocked.
    /// </summary>
    [RequireComponent(typeof(AssemblyInteractable), typeof(Interactable), typeof(Collider))]
    public class DoorCodeLock : MonoBehaviour
    {
        /// <summary>Замок двери, который снимается при открытии.</summary>
        [SerializeField] private LockObject _doorLock;

        /// <summary>Комната за дверью: выключена в сцене и включается при открытии.</summary>
        [SerializeField] private GameObject _room;

        /// <summary>Подсветка замка (эффект Important item). Гаснет при открытии, может быть пустой.</summary>
        [SerializeField] private ParticleFadeOut _highlight;

        /// <summary>Растворение модели замка. Пустое поле — замок выключается сразу.</summary>
        [SerializeField] private DissolveAppear _dissolve;

        /// <summary>Сохраняет факт открытия. Пустое поле — открытие не сохраняется.</summary>
        [SerializeField] private WorldEventSaver _saver;

        private AssemblyInteractable _assembly;
        private Interactable _interactable;
        private Collider _collider;

        private void Awake()
        {
            _assembly = GetComponent<AssemblyInteractable>();
            _interactable = GetComponent<Interactable>();
            _collider = GetComponent<Collider>();
            _assembly.onAssemblyCompleted.AddListener(Unlock);
        }

        private void OnDestroy()
        {
            _assembly.onAssemblyCompleted.RemoveListener(Unlock);
        }

        /// <summary>Восстанавливает открытое состояние после загрузки: без эффектов, замок и подсветка выключаются.</summary>
        public void RestoreUnlocked()
        {
            OpenPassage();
            if (_highlight != null)
            {
                _highlight.gameObject.SetActive(false);
            }
            gameObject.SetActive(false);
        }

        private void Unlock()
        {
            OpenPassage();
            if (_saver != null)
            {
                _saver.SaveEventState();
            }

            _interactable.DisableOutline();
            _interactable.enabled = false;
            _collider.enabled = false;

            // ParticleFadeOut запускает корутину, на выключенном объекте она упадёт.
            if (_highlight != null && _highlight.gameObject.activeInHierarchy)
            {
                _highlight.FadeOut();
            }

            if (_dissolve != null)
            {
                _dissolve.Disappear();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OpenPassage()
        {
            if (_doorLock != null)
            {
                _doorLock.Unlock();
            }
            if (_room != null)
            {
                _room.SetActive(true);
            }
        }
    }
}
