using System.Collections;
using UnityEngine;

namespace NightCycle
{
    /// <summary>
    /// Дверь, которая открывается и закрывается удержанием взаимодействия.
    /// При прерывании замирает на месте, следующее удержание продолжает с той же точки.
    /// Pivot объекта должен стоять на оси петель.
    /// </summary>
    [RequireComponent(typeof(Interactable), typeof(BoxCollider))]
    public class HoldDoor : MonoBehaviour
    {
        private const float AutoOpenSpeed = 5f;
        private const float AutoOpenSnapAngle = 0.5f;

        /// <summary>Угол поворота полностью открытой створки в градусах.</summary>
        [SerializeField] private float _openAngle = 90f;

        /// <summary>Подсказка, пока дверь открывается.</summary>
        [SerializeField] private string _openMessage = "Открыть";

        /// <summary>Подсказка, пока дверь закрывается.</summary>
        [SerializeField] private string _closeMessage = "Закрыть";

        private Interactable _interactable;
        private BoxCollider _collider;
        private LockObject _lock;
        private Quaternion _closedRotation;
        private float _openAmount;
        private float _swingSign = 1f;
        private bool _isOpening = true;
        private bool _autoOpenStarted;
        private bool _isAutoOpening;

        /// <summary>Заполнение прогресс-бара для текущего направления: от 0 до 1.</summary>
        public float Progress => _isOpening ? _openAmount : 1f - _openAmount;

        private bool IsLocked => _lock != null && _lock.enabled && _lock.isLocked;

        private void Awake()
        {
            _interactable = GetComponent<Interactable>();
            _collider = GetComponent<BoxCollider>();
            _lock = GetComponent<LockObject>();
            _closedRotation = transform.rotation;
            _interactable.message = _openMessage;
        }

        // У LockObject нет события разблокировки: Unlock вызывают из сцены и из сохранений.
        private void Update()
        {
            _interactable.enabled = !_isAutoOpening && !IsLocked;
        }

        /// <summary>
        /// Открывает дверь сама: для триггеров сцены. Срабатывает один раз, замок не учитывает.
        /// </summary>
        /// <param name="signedAngle">Угол открытия, знак задаёт сторону.</param>
        public void OpenAutomatically(float signedAngle)
        {
            if (_autoOpenStarted)
            {
                return;
            }

            _autoOpenStarted = true;
            _swingSign = Mathf.Sign(signedAngle);
            _openAngle = Mathf.Abs(signedAngle);
            StartCoroutine(AutoOpenRoutine());
        }

        private IEnumerator AutoOpenRoutine()
        {
            _isAutoOpening = true;
            Quaternion openRotation = GetRotationAt(1f);

            while (Quaternion.Angle(transform.rotation, openRotation) > AutoOpenSnapAngle)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, openRotation, Time.deltaTime * AutoOpenSpeed);
                yield return null;
            }

            transform.rotation = openRotation;
            _openAmount = 1f;
            _isOpening = false;
            _interactable.message = _closeMessage;
            _isAutoOpening = false;
        }

        /// <summary>
        /// Продвигает дверь на один кадр удержания. Пока створка упирается в игрока, движения нет.
        /// </summary>
        /// <returns>true, если в этом кадре дверь дошла до конца и сменила направление.</returns>
        public bool AdvanceHold(CharacterController player, float deltaTime)
        {
            if (_isOpening && _openAmount <= 0f)
            {
                _swingSign = GetSwingSignAwayFrom(player.transform.position);
            }

            float step = deltaTime / _interactable.holdDuration;
            float nextAmount = Mathf.Clamp01(_openAmount + (_isOpening ? step : -step));
            Quaternion nextRotation = GetRotationAt(nextAmount);

            if (OverlapsPlayer(player, nextRotation))
            {
                return false;
            }

            _openAmount = nextAmount;
            transform.rotation = nextRotation;

            bool reachedEnd = _isOpening ? nextAmount >= 1f : nextAmount <= 0f;
            if (reachedEnd)
            {
                _isOpening = !_isOpening;
                _interactable.message = _isOpening ? _openMessage : _closeMessage;
            }

            return reachedEnd;
        }

        private Quaternion GetRotationAt(float openAmount)
        {
            return _closedRotation * Quaternion.Euler(0f, openAmount * _openAngle * _swingSign, 0f);
        }

        private float GetSwingSignAwayFrom(Vector3 playerPosition)
        {
            Vector3 leafDirection = transform.TransformPoint(_collider.center) - transform.position;
            Vector3 positiveSwingDirection = Vector3.Cross(transform.up, leafDirection);
            return Vector3.Dot(positiveSwingDirection, playerPosition - transform.position) > 0f ? -1f : 1f;
        }

        private bool OverlapsPlayer(CharacterController player, Quaternion doorRotation)
        {
            return Physics.ComputePenetration(
                _collider, transform.position, doorRotation,
                player, player.transform.position, player.transform.rotation,
                out _, out _);
        }
    }
}
