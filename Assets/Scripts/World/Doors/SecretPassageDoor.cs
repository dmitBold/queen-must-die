using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

namespace NightCycle
{
    /// <summary>
    /// Стена-дверь в секретное помещение (архив, комната телепорта в библиотеку).
    /// Закрывает дверь анимацией, когда игрок вышел, и после анимации выключает помещение.
    /// Гул руны затихает, когда дверь открылась, и возвращается, когда она снова закрыта.
    /// Контроллер двери должен содержать триггеры play и close и состояние close.
    /// </summary>
    public class SecretPassageDoor : MonoBehaviour
    {
        private const string OpenTrigger = "play";
        private const string CloseTrigger = "close";
        private const float HumFadeDuration = 1f;
        private static readonly int ClosedStateHash = Animator.StringToHash("close");

        [SerializeField] private Animator _animator;

        /// <summary>Помещение за дверью: выключается, когда дверь закрылась.</summary>
        [SerializeField] private GameObject _room;

        /// <summary>Имя состояния открытия в контроллере двери: нужно, чтобы при загрузке поставить дверь сразу открытой.</summary>
        [SerializeField] private string _openStateName;

        /// <summary>Гул руны у двери. Можно не задавать, тогда звук не трогается.</summary>
        [SerializeField] private StudioEventEmitter _runeHum;

        /// <summary>Вызывается после закрытия двери и выключения помещения: сброс руны, сохранённого события.</summary>
        [SerializeField] private UnityEvent _onClosed;

        private Coroutine _closeRoutine;
        private Coroutine _humFadeRoutine;
        private bool _humSilenced;
        private int _openStateHash;

        private void Awake()
        {
            _openStateHash = Animator.StringToHash(_openStateName);
        }

        // Конец открытия ловим по состоянию аниматора, а не событием клипа: клип открытия библиотеки
        // используется и в дневной сцене, где этого компонента нет и событие осталось бы без получателя.
        // Так же покрываются рычаг архива и мгновенное открытие при загрузке.
        private void Update()
        {
            if (_runeHum == null || _humSilenced) return;

            if (!_animator.IsInTransition(0) && IsPlaying(_openStateHash, out float openTime) && openTime >= 1f)
            {
                _humSilenced = true;
                _humFadeRoutine = StartCoroutine(FadeOutHum());
            }
        }

        /// <summary>Включает помещение и ставит дверь в конец анимации открытия без звука и движения.</summary>
        public void OpenImmediately()
        {
            if (_closeRoutine != null)
            {
                StopCoroutine(_closeRoutine);
                _closeRoutine = null;
            }

            _room.SetActive(true);
            _animator.ResetTrigger(OpenTrigger);
            _animator.ResetTrigger(CloseTrigger);
            _animator.Play(_openStateName, 0, 1f);
        }

        /// <summary>Закрывает дверь анимацией, затем выключает помещение.</summary>
        public void Close()
        {
            if (_closeRoutine != null) return;

            _closeRoutine = StartCoroutine(CloseRoutine());
        }

        private IEnumerator CloseRoutine()
        {
            // Переход open → close без exit time (с exit time 1 он не срабатывает, если триггер пришёл после конца клипа),
            // поэтому конец открытия ждём здесь, иначе дверь прыгнет.
            while (_animator.IsInTransition(0) || IsPlaying(_openStateHash, out float openTime) && openTime < 1f)
                yield return null;

            if (IsPlaying(_openStateHash, out _))
            {
                // Рычаг архива ставит play и при уже открытой двери: несброшенный триггер открыл бы её сразу после закрытия.
                _animator.ResetTrigger(OpenTrigger);
                _animator.SetTrigger(CloseTrigger);

                while (!IsPlaying(ClosedStateHash, out float closeTime) || closeTime < 1f || _animator.IsInTransition(0))
                    yield return null;
            }

            _closeRoutine = null;
            _room.SetActive(false);
            RestoreHum();
            _onClosed.Invoke();
        }

        private IEnumerator FadeOutHum()
        {
            for (float time = 0f; time < HumFadeDuration; time += Time.deltaTime)
            {
                // Эмиттер пересоздаёт экземпляр события, когда игрок входит в радиус слышимости, поэтому берём его каждый кадр.
                if (_runeHum.EventInstance.isValid())
                    _runeHum.EventInstance.setVolume(1f - time / HumFadeDuration);
                yield return null;
            }

            _runeHum.Stop();
            _humFadeRoutine = null;
        }

        private void RestoreHum()
        {
            if (_runeHum == null || !_humSilenced) return;

            if (_humFadeRoutine != null)
            {
                StopCoroutine(_humFadeRoutine);
                _humFadeRoutine = null;
            }

            _humSilenced = false;
            _runeHum.Play();

            // Остановленный с затуханием экземпляр ещё может быть жив и переиспользоваться с нулевой громкостью.
            if (_runeHum.EventInstance.isValid())
                _runeHum.EventInstance.setVolume(1f);
        }

        private bool IsPlaying(int stateHash, out float normalizedTime)
        {
            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            normalizedTime = state.normalizedTime;
            return state.shortNameHash == stateHash;
        }
    }
}
