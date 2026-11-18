using System.Collections.Generic;
using Core;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Zenject;

namespace NightCycle
{
    /// <summary>
    /// Улей: слушает шум Бет в своей зоне и при пороге спускает рой. Точка улья — позиция этого объекта.
    /// Рой не покидает зону, не залетает в радиус горящего факела и убивает касанием.
    /// </summary>
    public class Hive : MonoBehaviour
    {
        private enum State { Calm, Attack, Return }

        private const int TorchBufferSize = 32;
        private const float MinMoveSpeed = 0.1f;

        [SerializeField] private HiveSwarm _swarm;
        [SerializeField] private StingDeath _death;
        /// <summary>Точка над ульем, где рой держится в покое.</summary>
        [SerializeField] private Transform _swarmHome;

        /// <summary>Радиус зоны слышимости. Стены звук не глушат. За эту границу рой не вылетает.</summary>
        [Header("Зона")]
        [SerializeField] private float _zoneRadius = 8f;
        /// <summary>Если центр Бет ближе этого к точке улья, рой нападает сразу, без шума.</summary>
        [SerializeField] private float _instantAttackRadius = 1.5f;

        /// <summary>Шум, на котором рой нападает, в секундах шума.</summary>
        [Header("Шум")]
        [SerializeField] private float _attackThreshold = 2.5f;
        /// <summary>За сколько реальных секунд спадает одна секунда шума.</summary>
        [SerializeField] private float _secondsPerNoiseUnit = 5f;
        /// <summary>Шум за секунду бега, ходьбы и ходьбы в приседе.</summary>
        [SerializeField] private float _sprintNoise = 1f;
        [SerializeField] private float _walkNoise = 1f / 3f;
        [SerializeField] private float _crouchNoise = 0f;
        /// <summary>Шум за приземление Бет в зоне.</summary>
        [SerializeField] private float _landingNoise = 1f;
        /// <summary>Сколько секунд Бет должна пробыть в воздухе, чтобы касание земли считалось приземлением. Отсекает дрожание isGrounded на ступеньках.</summary>
        [SerializeField] private float _minAirTime = 0.25f;
        /// <summary>Шум за падение факела в зоне (где стоит Бет — неважно).</summary>
        [SerializeField] private float _torchLandingNoise = 1f;

        /// <summary>Ближе этого рой к горящему факелу не подлетает, м.</summary>
        [Header("Огонь")]
        [SerializeField] private float _fireRadius = 2f;
        /// <summary>Слои коллайдеров факелов (у TorchItem это Ignore Raycast).</summary>
        [SerializeField] private LayerMask _torchMask = 1 << 2;

        /// <summary>Расстояние от роя до центра Бет, на котором рой жалит.</summary>
        [Header("Атака")]
        [SerializeField] private float _touchRadius = 0.7f;

        /// <summary>Петля гула роя. Параметр тревоги идёт от 0 (покой) до 1 (атака).</summary>
        [Header("Звук")]
        [SerializeField] private EventReference _buzzEvent;
        [SerializeField] private string _alarmParameter = "Alarm";
        [SerializeField] private float _buzzMinDistance = 1f;
        [SerializeField] private float _buzzMaxDistance = 20f;

        private readonly Collider[] _torchHits = new Collider[TorchBufferSize];
        private readonly List<TorchItem> _seenTorches = new List<TorchItem>();
        private readonly List<Vector3> _fires = new List<Vector3>();

        private FirstPersonController _player;
        private PlayerTorch _playerTorch;
        private PlayerInputManager _input;
        private AudioService _audio;
        private CharacterController _controller;

        private HiveNoise _noise;
        private State _state = State.Calm;
        private float _airTime;
        private float _lastTorchCheck;
        private EventInstance _buzz;

        [Inject]
        private void Construct(FirstPersonController player, PlayerTorch playerTorch, PlayerInputManager input,
            [InjectOptional] AudioService audio)
        {
            _player = player;
            _playerTorch = playerTorch;
            _input = input;
            _audio = audio;
        }

        private void Awake()
        {
            _noise = new HiveNoise(_secondsPerNoiseUnit);
        }

        private void Start()
        {
            _controller = _playerTorch.Controller;
            _swarm.SetIgnoredRoot(_player.transform);
            _swarm.transform.position = _swarmHome.position;
            _lastTorchCheck = Time.time;
            if (_audio != null)
                _buzz = _audio.PlayFMODEventAtPoint(_buzzEvent, _swarm.Position, _buzzMinDistance, _buzzMaxDistance);
        }

        private void OnDestroy()
        {
            // FMOD-экземпляр переживает выгрузку сцены: петлю гасим сами.
            if (_buzz.isValid())
            {
                _buzz.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                _buzz.release();
            }
        }

        private void Update()
        {
            Vector3 playerCenter = _controller.bounds.center;

            if (_death.IsPlaying)
            {
                // Рой остаётся на Бет, пока идёт сцена смерти.
                _swarm.Chase(playerCenter, transform.position, _zoneRadius, null, 0f);
                UpdateBuzz(1f);
                return;
            }

            float sqrDistance = (playerCenter - transform.position).sqrMagnitude;
            bool inZone = sqrDistance <= _zoneRadius * _zoneRadius;
            bool tooClose = sqrDistance <= _instantAttackRadius * _instantAttackRadius;

            _noise.Decay(Time.deltaTime);
            ListenToPlayer(inZone);
            ListenToTorches();

            bool provoked = tooClose || (inZone && _noise.Value >= _attackThreshold);
            switch (_state)
            {
                case State.Calm:
                case State.Return:
                    if (provoked) _state = State.Attack;
                    break;
                case State.Attack:
                    if (!inZone) _state = State.Return;
                    break;
            }

            float agitation = _state == State.Calm ? _noise.Value / _attackThreshold : 1f;
            MoveSwarm(playerCenter, agitation);
            UpdateBuzz(agitation);

            if (_state == State.Attack && (_swarm.Position - playerCenter).sqrMagnitude <= _touchRadius * _touchRadius)
                _death.Play();
        }

        private void ListenToPlayer(bool inZone)
        {
            if (!_controller.isGrounded)
            {
                _airTime += Time.deltaTime;
                return;
            }

            if (inZone && _airTime >= _minAirTime) _noise.Add(_landingNoise);
            _airTime = 0f;

            if (!inZone) return;

            Vector3 velocity = _controller.velocity;
            velocity.y = 0f;
            if (velocity.magnitude < MinMoveSpeed) return;

            float rate = _player.IsCrouching ? _crouchNoise : _input.SprintTriggered ? _sprintNoise : _walkNoise;
            _noise.Add(rate * Time.deltaTime);
        }

        // Факелы ищутся физикой в зоне: держатель в руке выключает коллайдеры, поэтому его берём у PlayerTorch.
        private void ListenToTorches()
        {
            _fires.Clear();
            _seenTorches.Clear();

            int count = Physics.OverlapSphereNonAlloc(transform.position, _zoneRadius + _fireRadius, _torchHits, _torchMask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                TorchItem torch = _torchHits[i].GetComponentInParent<TorchItem>();
                if (torch == null || _seenTorches.Contains(torch)) continue;
                _seenTorches.Add(torch);

                Vector3 position = torch.transform.position;
                if (torch.IsLit) _fires.Add(position);

                bool landedNow = torch.LandedTime > _lastTorchCheck && torch.LandedTime <= Time.time;
                if (landedNow && (position - transform.position).sqrMagnitude <= _zoneRadius * _zoneRadius)
                    _noise.Add(_torchLandingNoise);
            }
            _lastTorchCheck = Time.time;

            TorchItem held = _playerTorch.HeldTorch;
            if (held != null && held.IsLit) _fires.Add(held.transform.position);
        }

        private void MoveSwarm(Vector3 playerCenter, float agitation)
        {
            _swarm.SetAgitation(agitation);
            switch (_state)
            {
                case State.Calm:
                    _swarm.Hover(_swarmHome.position, agitation);
                    break;
                case State.Attack:
                    _swarm.Chase(playerCenter, transform.position, _zoneRadius, _fires, _fireRadius);
                    break;
                case State.Return:
                    if (_swarm.ReturnHome(_swarmHome.position, transform.position, _zoneRadius)) _state = State.Calm;
                    break;
            }
        }

        private void UpdateBuzz(float alarm)
        {
            if (!_buzz.isValid()) return;
            _buzz.set3DAttributes(RuntimeUtils.To3DAttributes(_swarm.Position));
            if (!string.IsNullOrEmpty(_alarmParameter)) _buzz.setParameterByName(_alarmParameter, Mathf.Clamp01(alarm));
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.8f, 0f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, _zoneRadius);
            Gizmos.color = new Color(1f, 0.2f, 0f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, _instantAttackRadius);
        }
    }
}
