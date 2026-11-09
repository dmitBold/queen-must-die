using System.Collections;
using Core;
using UnityEngine;
using Zenject;

namespace NightCycle
{
    /// <summary>
    /// Факел в руке игрока: анимация подбора, покачивание при ходьбе, бросок на H и штраф к скорости.
    /// </summary>
    public class PlayerTorch : MonoBehaviour
    {
        private const string ThrowHintFlag = "hint_torch_throw_shown";
        private const float MinSwaySpeed = 0.1f;
        private const float WallMargin = 0.15f;

        [Header("Ссылки")]
        [SerializeField] private FirstPersonController _movement;
        [SerializeField] private CharacterController _controller;
        [SerializeField] private PlayerStateController _state;
        [SerializeField] private Transform _view;
        [SerializeField] private Transform _holdPoint;

        /// <summary>Множитель скорости, пока факел в руке.</summary>
        [Header("В руке")]
        [SerializeField] private float _heldSpeedMultiplier = 0.9f;
        /// <summary>Размер факела в руке.</summary>
        [SerializeField] private float _handScale = 0.3f;

        [Header("Подбор")]
        [SerializeField] private float _pullDistance = 0.25f;
        [SerializeField] private float _pullDuration = 0.3f;
        [SerializeField] private float _flyDuration = 0.6f;

        [Header("Бросок")]
        [SerializeField] private float _throwSpeed = 4.77f;
        [SerializeField] private float _throwUpSpeed = 1.02f;
        /// <summary>Скорость кувырка вперёд при броске, рад/с.</summary>
        [SerializeField] private float _throwSpin = 2.5f;
        /// <summary>Скорость старого факела, который выпадает при замене.</summary>
        [SerializeField] private float _dropSpeed = 0.5f;

        [Header("Покачивание при ходьбе")]
        [SerializeField] private float _swaySide = 0.03f;
        [SerializeField] private float _swayUp = 0.02f;
        /// <summary>Сколько циклов покачивания приходится на метр пути.</summary>
        [SerializeField] private float _swayCyclesPerMeter = 0.125f;
        [SerializeField] private float _swayFadeSpeed = 6f;

        [Header("Подсказка про бросок")]
        [TextArea(1, 2)]
        [SerializeField] private string _throwHintText = "H — выкинуть";
        [SerializeField] private float _throwHintDuration = 4f;
        [SerializeField] private float _throwHintFade = 0.5f;

        private PlayerInputManager _input;
        private HintManager _hintManager;
        private WorldState _worldState;
        private TorchItem _held;
        private Vector3 _holdPointBase;
        private float _swayPhase;
        private float _swayWeight;
        private bool _animating;

        /// <summary>Контроллер игрока: брошенный факел не должен с ним сталкиваться.</summary>
        public CharacterController Controller => _controller;

        /// <summary>Можно ли сейчас взять факел: во время анимации подбора нельзя.</summary>
        public bool CanTake => !_animating;

        [Inject]
        private void Construct(PlayerInputManager input,
            [InjectOptional] HintManager hintManager,
            [InjectOptional] WorldState worldState)
        {
            _input = input;
            _hintManager = hintManager;
            _worldState = worldState;
        }

        private void Awake() => _holdPointBase = _holdPoint.localPosition;

        private void Update()
        {
            if (_held == null) return;

            UpdateSway();

            if (!_animating && _state.CurrentMode == PlayerMode.FreeMovement && _input.ThrowTorchPressed)
                Throw();
        }

        /// <summary>Берёт факел в руку. Если в руке уже есть другой, он выпадает на пол.</summary>
        public void Take(TorchItem torch, Vector3 pullDirection)
        {
            if (_held != null)
                ReleaseHeld(_view.forward * _dropSpeed, _view.right * _throwSpin);

            _held = torch;
            _animating = true;
            torch.TakeInHand();
            _movement.SpeedMultiplier = _heldSpeedMultiplier;
            ShowThrowHint();
            StartCoroutine(BringToHand(torch, pullDirection));
        }

        private IEnumerator BringToHand(TorchItem torch, Vector3 pullDirection)
        {
            Transform t = torch.transform;
            Vector3 startPosition = t.position;
            Vector3 pulledPosition = startPosition + pullDirection * _pullDistance;
            Quaternion startRotation = t.rotation;
            Vector3 startScale = t.localScale;
            Vector3 handScale = Vector3.one * _handScale;

            for (float time = 0f; time < _pullDuration; time += Time.deltaTime)
            {
                t.position = Vector3.Lerp(startPosition, pulledPosition, Mathf.SmoothStep(0f, 1f, time / _pullDuration));
                yield return null;
            }

            // Цель берётся заново каждый кадр: игрок может идти и поворачиваться во время полёта.
            for (float time = 0f; time < _flyDuration; time += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, time / _flyDuration);
                t.position = Vector3.Lerp(pulledPosition, _holdPoint.position, k);
                t.rotation = Quaternion.Slerp(startRotation, _holdPoint.rotation, k);
                t.localScale = Vector3.Lerp(startScale, handScale, k);
                torch.SetHandBlend(k);
                yield return null;
            }

            t.SetPositionAndRotation(_holdPoint.position, _holdPoint.rotation);
            t.localScale = handScale;
            t.SetParent(_holdPoint, true);
            torch.SetHandBlend(1f);
            _animating = false;
        }

        private void Throw()
        {
            Vector3 velocity = _view.forward * _throwSpeed + Vector3.up * _throwUpSpeed + _controller.velocity;
            ReleaseHeld(velocity, _view.right * _throwSpin);
        }

        private void ReleaseHeld(Vector3 velocity, Vector3 spin)
        {
            _held.Release(GetReleasePosition(), velocity, spin);
            _held = null;
            _movement.SpeedMultiplier = 1f;
            _holdPoint.localPosition = _holdPointBase;
        }

        // Рука может оказаться за стеной, и тогда факел улетел бы за пределы комнаты.
        private Vector3 GetReleasePosition()
        {
            Vector3 origin = _view.position;
            Vector3 toTorch = _held.transform.position - origin;
            float distance = toTorch.magnitude;
            Vector3 direction = toTorch / distance;

            if (TorchItem.RaycastTwoSided(origin, direction, distance + WallMargin, out RaycastHit hit))
                return origin + direction * Mathf.Max(0f, hit.distance - WallMargin);

            return _held.transform.position;
        }

        private void UpdateSway()
        {
            Vector3 velocity = _controller.velocity;
            velocity.y = 0f;
            float speed = velocity.magnitude;

            float targetWeight = _state.CanMove() && speed > MinSwaySpeed ? 1f : 0f;
            _swayWeight = Mathf.MoveTowards(_swayWeight, targetWeight, _swayFadeSpeed * Time.deltaTime);
            _swayPhase += speed * _swayCyclesPerMeter * 2f * Mathf.PI * Time.deltaTime;

            // Вдвое большая частота по вертикали даёт движение восьмёркой, как при шаге.
            Vector3 offset = new Vector3(Mathf.Sin(_swayPhase) * _swaySide, Mathf.Sin(_swayPhase * 2f) * _swayUp, 0f);
            _holdPoint.localPosition = _holdPointBase + offset * _swayWeight;
        }

        private void ShowThrowHint()
        {
            if (_hintManager == null || _worldState == null) return;
            if (_worldState.HasFlag(ThrowHintFlag)) return;

            _worldState.AddFlag(ThrowHintFlag);
            _hintManager.ShowHint(_throwHintText, _throwHintDuration, _throwHintFade);
        }
    }
}
