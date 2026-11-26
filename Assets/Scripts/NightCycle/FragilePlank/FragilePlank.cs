using Core;
using UnityEngine;
using Zenject;

namespace NightCycle
{
    /// <summary>
    /// Хрупкая доска: копит нагрузку от бега Бет и приземлений на неё, прогибается и сыплет трухой,
    /// а на пороге ломается на куски, и Бет проваливается вниз. Ходьба доску не нагружает.
    /// </summary>
    public class FragilePlank : MonoBehaviour
    {
        private const float MinMoveSpeed = 0.1f;
        private const float GroundProbeRadiusFactor = 0.9f;
        private const float GroundProbeExtraDistance = 0.15f;

        /// <summary>Цельный коллайдер доски, на котором стоит Бет. При сломе выключается.</summary>
        [SerializeField] private Collider _support;
        /// <summary>Две половины доски. Пивот каждой — на нижнем ребре внешнего торца: вокруг него половина прогибается.</summary>
        [SerializeField] private Transform[] _halves;
        /// <summary>Все куски доски, включая половины: кинематические, коллайдеры выключены до слома.</summary>
        [SerializeField] private Rigidbody[] _pieces;
        /// <summary>Труха из щели: сыплется во время бега, тем гуще, чем ближе слом. Необязательно.</summary>
        [SerializeField] private ParticleSystem _dust;
        /// <summary>Облако пыли в момент слома. Необязательно.</summary>
        [SerializeField] private ParticleSystem _breakDust;

        /// <summary>Нагрузка, на которой доска ломается, в секундах бега.</summary>
        [Header("Нагрузка")]
        [SerializeField] private float _breakThreshold = 3f;
        /// <summary>Нагрузка за секунду бега по доске.</summary>
        [SerializeField] private float _sprintStress = 1f;
        /// <summary>Нагрузка за приземление на доску.</summary>
        [SerializeField] private float _landingStress = 1f;
        /// <summary>Сколько нагрузки спадает за секунду, пока Бет не на доске. На доске нагрузка не спадает, даже если Бет стоит.</summary>
        [SerializeField] private float _decayPerSecond = 1f;
        /// <summary>Сколько секунд Бет должна пробыть в воздухе, чтобы касание доски считалось приземлением. Отсекает дрожание isGrounded.</summary>
        [SerializeField] private float _minAirTime = 0.25f;

        /// <summary>Угол прогиба каждой половины на пороге слома, в градусах.</summary>
        [Header("Прогиб")]
        [SerializeField] private float _maxSagAngle = 3f;
        /// <summary>Скорость, с которой прогиб догоняет нагрузку, градусов в секунду.</summary>
        [SerializeField] private float _sagSpeed = 6f;
        /// <summary>Размах дрожи половин во время бега на пороге слома, в градусах.</summary>
        [SerializeField] private float _shakeAngle = 0.4f;
        [SerializeField] private float _shakeFrequency = 20f;

        /// <summary>Частиц трухи в секунду во время бега на пороге слома.</summary>
        [Header("Труха")]
        [SerializeField] private float _maxDustRate = 40f;
        /// <summary>Частиц трухи за одно приземление.</summary>
        [SerializeField] private int _landingDustCount = 15;

        /// <summary>Скорость, с которой точка половины под ногами Бет уходит вниз при сломе, м/с.</summary>
        [Header("Слом")]
        [SerializeField] private float _breakPush = 1.5f;
        /// <summary>Скорость, с которой щепки отскакивают при сломе, м/с.</summary>
        [SerializeField] private float _splinterPush = 1.5f;
        /// <summary>Закрутка щепок при сломе, рад/с.</summary>
        [SerializeField] private float _splinterSpin = 15f;

        private FirstPersonController _player;
        private PlayerInputManager _input;
        private CharacterController _controller;

        private PlankStress _stress;
        private Quaternion[] _restRotations;
        private Vector3[] _sagAxes;
        private float _sag;
        private float _airTime;
        private bool _playerOnPlank;
        private bool _broken;

        [Inject]
        private void Construct(FirstPersonController player, PlayerInputManager input)
        {
            _player = player;
            _input = input;
        }

        private void Awake()
        {
            _stress = new PlankStress(_breakThreshold, _decayPerSecond);
        }

        private void Start()
        {
            if (_support == null || _halves == null || _halves.Length == 0 || _pieces == null || _pieces.Length == 0)
            {
                Debug.LogError($"{nameof(FragilePlank)} на {name}: не назначены коллайдер, половины или куски.", this);
                enabled = false;
                return;
            }

            _controller = _player.GetComponent<CharacterController>();
            CacheSagAxes();
            SetDustRate(0f);
        }

        private void Update()
        {
            if (_broken) return;

            float deltaTime = Time.deltaTime;
            bool running = false;

            if (!_controller.isGrounded)
            {
                // В воздухе Бет остаётся на той поверхности, с которой прыгнула: прыжок на месте не сбрасывает нагрузку.
                _airTime += deltaTime;
            }
            else
            {
                _playerOnPlank = IsPlayerOnPlank();
                if (_playerOnPlank && _airTime >= _minAirTime)
                {
                    _stress.Add(_landingStress);
                    if (_dust != null) _dust.Emit(_landingDustCount);
                }
                _airTime = 0f;
                running = _playerOnPlank && IsPlayerRunning();
            }

            if (running) _stress.Add(_sprintStress * deltaTime);
            if (!_playerOnPlank) _stress.Decay(deltaTime);

            if (_stress.IsOverloaded)
            {
                Break();
                return;
            }

            UpdateSag(running, deltaTime);
            SetDustRate(running ? _maxDustRate * _stress.Ratio : 0f);
        }

        // Ось прогиба лежит в плоскости доски поперёк её длины: поворот вокруг неё опускает внутренний конец половины.
        private void CacheSagAxes()
        {
            _restRotations = new Quaternion[_halves.Length];
            _sagAxes = new Vector3[_halves.Length];
            Vector3 plankCenter = _support.bounds.center;

            for (int i = 0; i < _halves.Length; i++)
            {
                Transform half = _halves[i];
                Transform parent = half.parent;
                Vector3 up = parent.InverseTransformDirection(Vector3.up);
                Vector3 inward = Vector3.ProjectOnPlane(parent.InverseTransformPoint(plankCenter) - half.localPosition, up);

                _restRotations[i] = half.localRotation;
                _sagAxes[i] = Vector3.Cross(up, inward).normalized;
            }
        }

        private bool IsPlayerOnPlank()
        {
            Bounds bounds = _controller.bounds;
            float radius = _controller.radius * _controller.transform.lossyScale.x;
            float distance = bounds.extents.y - radius + _controller.skinWidth + GroundProbeExtraDistance;

            return Physics.SphereCast(bounds.center, radius * GroundProbeRadiusFactor, Vector3.down, out RaycastHit hit,
                       distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
                   && hit.collider == _support;
        }

        private bool IsPlayerRunning()
        {
            Vector3 velocity = _controller.velocity;
            velocity.y = 0f;
            return velocity.magnitude >= MinMoveSpeed && _input.SprintTriggered && !_player.IsCrouching;
        }

        private void UpdateSag(bool running, float deltaTime)
        {
            float ratio = _stress.Ratio;
            _sag = Mathf.MoveTowards(_sag, _maxSagAngle * ratio, _sagSpeed * deltaTime);

            for (int i = 0; i < _halves.Length; i++)
            {
                float shake = running ? (Mathf.PerlinNoise(Time.time * _shakeFrequency, i) - 0.5f) * 2f * _shakeAngle * ratio : 0f;
                _halves[i].localRotation = Quaternion.AngleAxis(_sag + shake, _sagAxes[i]) * _restRotations[i];
            }
        }

        private void SetDustRate(float rate)
        {
            if (_dust == null) return;
            ParticleSystem.EmissionModule emission = _dust.emission;
            emission.rateOverTimeMultiplier = rate;
        }

        private void Break()
        {
            _broken = true;
            _support.enabled = false;
            SetDustRate(0f);
            if (_breakDust != null) _breakDust.Play();

            Bounds playerBounds = _controller.bounds;
            Vector3 foot = playerBounds.center - Vector3.up * playerBounds.extents.y;

            foreach (Rigidbody piece in _pieces)
            {
                foreach (Collider pieceCollider in piece.GetComponents<Collider>())
                {
                    pieceCollider.enabled = true;
                    // Иначе Бет встала бы на падающую половину вместо того, чтобы провалиться.
                    Physics.IgnoreCollision(pieceCollider, _controller);
                }
            }
            IgnoreCollisionsBetweenPieces();

            foreach (Rigidbody piece in _pieces)
            {
                piece.isKinematic = false;
                if (System.Array.IndexOf(_halves, piece.transform) >= 0)
                {
                    piece.AddForceAtPosition(Vector3.down * _breakPush, piece.ClosestPointOnBounds(foot), ForceMode.VelocityChange);
                }
                else
                {
                    piece.AddForce((Vector3.up + Random.insideUnitSphere) * _splinterPush, ForceMode.VelocityChange);
                    piece.AddTorque(Random.insideUnitSphere * _splinterSpin, ForceMode.VelocityChange);
                }
            }
        }

        // Зубцы излома заходят друг в друга, и коллайдеры соседних кусков в сборе пересекаются: без этого физика расталкивает их при сломе.
        private void IgnoreCollisionsBetweenPieces()
        {
            for (int a = 0; a < _pieces.Length; a++)
            {
                Collider[] collidersA = _pieces[a].GetComponents<Collider>();
                for (int b = a + 1; b < _pieces.Length; b++)
                {
                    foreach (Collider colliderA in collidersA)
                    foreach (Collider colliderB in _pieces[b].GetComponents<Collider>())
                        Physics.IgnoreCollision(colliderA, colliderB);
                }
            }
        }
    }
}
