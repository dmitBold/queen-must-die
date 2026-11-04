using System.Collections;
using Core;
using FMODUnity;
using Inventory;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace NightCycle
{
    /// <summary>
    /// Состояние сокета сборки. Filling существует только пока играет анимация заполнения.
    /// </summary>
    public enum SocketState
    {
        Empty,
        Filling,
        Filled
    }

    public class AssemblySocket : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] ItemData requiredItem;
        [SerializeField] GameObject visualPart;

        [Header("Fill Animation (optional)")]
        [Tooltip("Animator, который проигрывает приход part'а к основному объекту. Если не задан, visual part просто включается.")]
        [SerializeField] private Animator fillAnimator;
        [Tooltip("Имя состояния Animator с анимацией заполнения. Без имени анимация не запускается.")]
        [SerializeField] private string fillStateName;

        public EventReference SocketFillEvent;
        /// <summary>
        /// Срабатывает в начале заполнения (звук, сохранение через WorldEventSaver).
        /// </summary>
        public UnityEvent SocketEvent;
        /// <summary>
        /// Срабатывает, когда сокет стал заполненным и логически, и визуально.
        /// </summary>
        public UnityEvent OnFilled;
        public bool triggerOnnce = true;
        private AudioService _audioService;
        Outline outline;
        private WorldEventSaver _eventSaver;
        private Coroutine _fillRoutine;
        private bool _snapPoseOnEnable;

        private SocketState _state = SocketState.Empty;

        /// <summary>
        /// Истина только когда заполнение полностью завершено (после анимации).
        /// </summary>
        public bool IsFilled => _state == SocketState.Filled;
        /// <summary>
        /// Истина, пока в сокет можно вставить part: он не заполнен и не заполняется.
        /// </summary>
        public bool IsEmpty => _state == SocketState.Empty;
        public SocketState State => _state;
        public ItemData RequiredItem => requiredItem;

        [Inject]
        public void Constructor(AudioService audioService)
        {
            _audioService = audioService;
        }

        void Awake()
        {
            if (visualPart != null) {
                visualPart.SetActive(false);
            }
            outline = GetComponent<Outline>();
            // WorldEventSaver необязателен: он есть только у сокетов, состояние которых сохраняется.
            TryGetComponent(out _eventSaver);
            //outline.enabled = false;
            SetHighlight(false);
        }

        private void OnEnable()
        {
            // Animator на неактивном объекте не вычисляет позу, поэтому отложенную позу применяем при включении.
            if (_snapPoseOnEnable)
            {
                _snapPoseOnEnable = false;
                SnapToFinalPose();
            }
        }

        private void OnDisable()
        {
            if (_state != SocketState.Filling)
                return;

            // Unity уже остановила корутину, а отключённый сокет не должен остаться в Filling навсегда.
            // OnFilled здесь намеренно не вызывается: в момент отключения цепочку завершения запускать небезопасно.
            _fillRoutine = null;
            _state = SocketState.Filled;
            _snapPoseOnEnable = true;
        }

        public bool CanAccept(ItemData item)
        {
            return IsEmpty && item == requiredItem;
        }

        /// <summary>
        /// Вставляет part в сокет. SocketEvent и звук срабатывают сразу. Если задана анимация заполнения,
        /// сокет переходит в Filling и становится Filled после её завершения, иначе становится Filled сразу.
        /// Внимание: без анимации OnFilled вызывается синхронно внутри этого метода.
        /// </summary>
        public void Apply(ItemData item)
        {
            if (!CanAccept(item))
                return;

            _state = SocketState.Filling;

            if (visualPart != null)
            {
                visualPart.SetActive(true);
            }

            SetHighlight(false);
            outline.enabled = false;

            //test
            if (!SocketFillEvent.IsNull)
            {
                _audioService.PlayFMODEvent(SocketFillEvent, gameObject.transform.position);
            }
            //test

            if (triggerOnnce)
            {
                SocketEvent?.Invoke();
            }

            if (TryPlayFillAnimation())
                _fillRoutine = StartCoroutine(WaitFillAnimation());
            else
                FinishFill();
        }

        /// <summary>
        /// Тихо восстанавливает заполненный сокет (вызывается из WorldEventSaver при загрузке).
        /// Не вызывает SocketEvent, OnFilled и проверку завершения сборки. Имя метода используется в префабах.
        /// </summary>
        public void Apply_now()
        {
            if (IsFilled)
                return;

            StopFillRoutine();
            _state = SocketState.Filled;

            if (visualPart != null)
            {
                visualPart.SetActive(true);
            }

            SetHighlight(false);
            SnapToFinalPose();
        }

        /// <summary>
        /// Мгновенно завершает идущее заполнение: ставит финальную позу и переводит сокет в Filled.
        /// Если сокет не в Filling, ничего не делает.
        /// </summary>
        public void CompleteImmediately()
        {
            if (_state != SocketState.Filling)
                return;

            StopFillRoutine();
            FinishFill();
        }

        /// <summary>
        /// Возвращает сокет в пустое состояние для новой сессии сборки и сбрасывает его сохранённое событие.
        /// </summary>
        public void ResetState()
        {
            StopFillRoutine();
            _state = SocketState.Empty;
            _snapPoseOnEnable = false;

            if (visualPart != null)
            {
                visualPart.SetActive(false);
            }

            // Animator мог быть выключен ради сохранения финальной позы: после сброса он снова нужен.
            if (fillAnimator != null)
            {
                fillAnimator.enabled = true;
            }

            if (_eventSaver != null)
            {
                _eventSaver.ClearEventState();
            }

            SetHighlight(false);
        }

        public void SetHighlight(bool state)
        {
            if (IsEmpty)
            {
                outline.enabled = state;
            }
        }

        public void SetEvent(bool state)
        {
            triggerOnnce = state;
        }

        private bool TryPlayFillAnimation()
        {
            if (fillAnimator == null || string.IsNullOrEmpty(fillStateName))
                return false;

            if (!fillAnimator.gameObject.activeInHierarchy)
                return false;

            int stateHash = Animator.StringToHash(fillStateName);
            if (!fillAnimator.HasState(0, stateHash))
            {
                Debug.LogWarning($"[AssemblySocket] В Animator нет состояния '{fillStateName}', сокет заполнится без анимации.", this);
                return false;
            }

            fillAnimator.enabled = true;
            fillAnimator.Play(stateHash, 0, 0f);
            // Без принудительного шага GetCurrentAnimatorStateInfo до следующего кадра вернёт старое состояние.
            fillAnimator.Update(0f);
            return true;
        }

        private IEnumerator WaitFillAnimation()
        {
            int stateHash = Animator.StringToHash(fillStateName);

            while (fillAnimator != null)
            {
                yield return null;

                if (fillAnimator == null || fillAnimator.IsInTransition(0))
                    continue;

                AnimatorStateInfo info = fillAnimator.GetCurrentAnimatorStateInfo(0);
                // Если Animator ушёл из состояния сам (выходной переход), анимацию тоже считаем завершённой.
                if (info.shortNameHash != stateHash || info.normalizedTime >= 1f)
                    break;
            }

            _fillRoutine = null;
            FinishFill();
        }

        private void FinishFill()
        {
            if (IsFilled)
                return;

            _state = SocketState.Filled;
            // Фиксируем позу: при повторной активации вью Animator иначе вернулся бы в idle и part «отъехал» бы назад.
            SnapToFinalPose();
            OnFilled?.Invoke();
        }

        /// <summary>
        /// Ставит последний кадр анимации заполнения и выключает Animator, чтобы поза не сбрасывалась.
        /// Работает, пока WriteDefaultValuesOnDisable у Animator выключен (как в текущих префабах).
        /// </summary>
        private void SnapToFinalPose()
        {
            if (fillAnimator == null || string.IsNullOrEmpty(fillStateName))
                return;

            if (!fillAnimator.gameObject.activeInHierarchy)
            {
                _snapPoseOnEnable = true;
                return;
            }

            int stateHash = Animator.StringToHash(fillStateName);
            if (!fillAnimator.HasState(0, stateHash))
                return;

            fillAnimator.enabled = true;
            fillAnimator.Play(stateHash, 0, 1f);
            fillAnimator.Update(0f);
            fillAnimator.enabled = false;
        }

        private void StopFillRoutine()
        {
            if (_fillRoutine == null)
                return;

            StopCoroutine(_fillRoutine);
            _fillRoutine = null;
        }

    }
}
