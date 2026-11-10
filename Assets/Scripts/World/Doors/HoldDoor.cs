using System.Collections;
using Core;
using FMODUnity;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace NightCycle
{
    /// <summary>
    /// Дверь, которая открывается и закрывается удержанием взаимодействия.
    /// При прерывании замирает на месте, следующее удержание продолжает с той же точки.
    /// Статуи выбивают её от себя (TryKick). Pivot объекта должен стоять на оси петель.
    /// </summary>
    [RequireComponent(typeof(Interactable), typeof(BoxCollider))]
    public class HoldDoor : MonoBehaviour
    {
        private const float AutoOpenSpeed = 5f;
        private const float AutoOpenSnapAngle = 0.5f;
        private const float KickDuration = 0.15f;
        private const float FullyOpenAmount = 0.99f;
        private const float DoorwayDepth = 1.5f;
        private const float DoorwaySideMargin = 0.3f;
        private const float DoorwayHeight = 2f;
        private const float PushSkin = 0.02f;

        /// <summary>Угол поворота полностью открытой створки в градусах.</summary>
        [SerializeField] private float _openAngle = 90f;

        /// <summary>Подсказка, пока дверь открывается.</summary>
        [SerializeField] private string _openMessage = "Открыть";

        /// <summary>Подсказка, пока дверь закрывается.</summary>
        [SerializeField] private string _closeMessage = "Закрыть";

        /// <summary>Звук удара, когда статуя выбивает дверь. Пустое поле — без звука.</summary>
        [SerializeField] private EventReference _kickSound;

        private Interactable _interactable;
        private BoxCollider _collider;
        private LockObject _lock;
        private NavMeshObstacle _lockedObstacle;
        private AudioService _audioService;
        private CharacterController _player;
        private Quaternion _closedRotation;
        private Vector3 _closedLeafDirection;
        private Vector3 _doorwayCenter;
        private float _doorwayHalfWidth;
        private float _openAmount;
        private float _swingSign = 1f;
        private bool _isOpening = true;
        private bool _autoOpenStarted;
        private bool _isAutoOpening;
        private bool _isKicking;

        /// <summary>Заполнение прогресс-бара для текущего направления: от 0 до 1.</summary>
        public float Progress => _isOpening ? _openAmount : 1f - _openAmount;

        private bool IsLocked => _lock != null && _lock.enabled && _lock.isLocked;

        private bool IsBusy => _isAutoOpening || _isKicking;

        [Inject]
        public void Construct(AudioService audioService, [InjectOptional] FirstPersonController playerController)
        {
            _audioService = audioService;
            _player = playerController != null ? playerController.GetComponent<CharacterController>() : null;
        }

        private void Awake()
        {
            _interactable = GetComponent<Interactable>();
            _collider = GetComponent<BoxCollider>();
            _lock = GetComponent<LockObject>();
            _lockedObstacle = GetComponent<NavMeshObstacle>();
            _closedRotation = transform.rotation;
            _interactable.message = _openMessage;

            Vector3 toLeafCenter = transform.TransformPoint(_collider.center) - transform.position;
            toLeafCenter.y = 0f;
            _closedLeafDirection = toLeafCenter.normalized;
            _doorwayHalfWidth = toLeafCenter.magnitude;
            _doorwayCenter = transform.position + toLeafCenter;
        }

        // У LockObject нет события разблокировки: Unlock вызывают из сцены и из сохранений.
        private void Update()
        {
            _interactable.enabled = !IsBusy && !IsLocked;

            // Запертый проём вырезается из навмеша, чтобы статуи искали обход.
            if (_lockedObstacle != null)
            {
                _lockedObstacle.enabled = IsLocked;
            }
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

        /// <summary>
        /// Удар статуи: если она идёт через проём, дверь резко распахивается от неё до конца.
        /// Запертые двери и двери, уже открытые от статуи до конца, не трогаются.
        /// </summary>
        /// <param name="kickerPosition">Позиция статуи.</param>
        /// <param name="moveDirection">Куда статуя движется.</param>
        public void TryKick(Vector3 kickerPosition, Vector3 moveDirection)
        {
            if (IsBusy || IsLocked || !IsApproachingDoorway(kickerPosition, moveDirection))
            {
                return;
            }

            float awaySign = GetSwingSignAwayFrom(kickerPosition);
            float signedAmount = _openAmount * _swingSign;
            if (signedAmount * awaySign >= FullyOpenAmount)
            {
                return;
            }

            StartCoroutine(KickRoutine(signedAmount, awaySign));
        }

        /// <summary>
        /// Продвигает дверь на один кадр удержания. Пока створка упирается в игрока, движения нет.
        /// </summary>
        /// <returns>true, если в этом кадре дверь дошла до конца и сменила направление.</returns>
        public bool AdvanceHold(CharacterController player, float deltaTime)
        {
            if (IsBusy)
            {
                return false;
            }

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
            SetFullyOpen();
            _isAutoOpening = false;
        }

        // Створка идёт через закрытое положение, если до удара была открыта к статуе.
        private IEnumerator KickRoutine(float fromSignedAmount, float awaySign)
        {
            _isKicking = true;
            _audioService?.PlayFMODEvent(_kickSound, _doorwayCenter);

            float elapsed = 0f;
            while (elapsed < KickDuration)
            {
                elapsed += Time.deltaTime;
                float signedAmount = Mathf.Lerp(fromSignedAmount, awaySign, Mathf.Clamp01(elapsed / KickDuration));
                transform.rotation = _closedRotation * Quaternion.Euler(0f, signedAmount * _openAngle, 0f);
                PushPlayerOut();
                yield return null;
            }

            _swingSign = awaySign;
            SetFullyOpen();
            _isKicking = false;
        }

        private void SetFullyOpen()
        {
            _openAmount = 1f;
            _isOpening = false;
            _interactable.message = _closeMessage;
        }

        private bool IsApproachingDoorway(Vector3 kickerPosition, Vector3 moveDirection)
        {
            Vector3 offset = kickerPosition - _doorwayCenter;
            Vector3 normal = Vector3.Cross(Vector3.up, _closedLeafDirection);
            float depth = Vector3.Dot(offset, normal);

            bool inDoorway = Mathf.Abs(Vector3.Dot(offset, _closedLeafDirection)) <= _doorwayHalfWidth + DoorwaySideMargin
                && Mathf.Abs(depth) <= DoorwayDepth
                && Mathf.Abs(kickerPosition.y - transform.position.y) <= DoorwayHeight;

            return inDoorway && Vector3.Dot(moveDirection, normal) * depth < 0f;
        }

        private void PushPlayerOut()
        {
            if (_player == null)
            {
                return;
            }

            if (Physics.ComputePenetration(
                    _collider, transform.position, transform.rotation,
                    _player, _player.transform.position, _player.transform.rotation,
                    out Vector3 doorDirection, out float distance))
            {
                // ComputePenetration возвращает сдвиг для двери, игрока двигаем в обратную сторону.
                _player.Move(-doorDirection * (distance + PushSkin));
            }
        }

        private Quaternion GetRotationAt(float openAmount)
        {
            return _closedRotation * Quaternion.Euler(0f, openAmount * _openAngle * _swingSign, 0f);
        }

        // Считается по закрытому положению: у открытой створки направление уже другое.
        private float GetSwingSignAwayFrom(Vector3 position)
        {
            Vector3 positiveSwingDirection = Vector3.Cross(Vector3.up, _closedLeafDirection);
            return Vector3.Dot(positiveSwingDirection, position - transform.position) > 0f ? -1f : 1f;
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
