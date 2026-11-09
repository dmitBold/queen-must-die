using UnityEngine;
using Zenject;

namespace NightCycle
{
    /// <summary>
    /// Снятый со стены факел: в руке или на полу. Сам считает время горения и гасит огонь.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(FireIgnition))]
    public class TorchItem : MonoBehaviour
    {
        /// <summary>Полное время горения, сек.</summary>
        [SerializeField] private float _burnDuration = 240f;

        /// <summary>Раз в сколько секунд пламя становится меньше.</summary>
        [SerializeField] private float _flameStep = 15f;

        /// <summary>За сколько секунд пламя плавно уменьшается на один шаг.</summary>
        [SerializeField] private float _flameStepFade = 1f;

        /// <summary>Сколько секунд горения отнимает бросок или выпадение из руки.</summary>
        [SerializeField] private float _releasePenalty = 30f;

        /// <summary>Как быстро факел возвращает обычный размер после руки.</summary>
        [SerializeField] private float _looseScaleSpeed = 3f;

        /// <summary>Во сколько раз свет ярче, пока факел в руке.</summary>
        [SerializeField] private float _handLightBoost = 2.7f;

        /// <summary>Во сколько раз больше радиус света, пока факел в руке.</summary>
        [SerializeField] private float _handLightRange = 1.25f;

        /// <summary>На сколько метров над огнём висит свет свободного факела.</summary>
        [SerializeField] private float _looseLightLift = 0.5f;

        /// <summary>С какого расстояния факел отскакивает от изнаночной стороны стены.</summary>
        [SerializeField] private float _backfaceProbe = 0.35f;

        /// <summary>Какую долю скорости факел сохраняет после отскока от изнанки стены.</summary>
        [SerializeField] private float _backfaceBounce = 0.35f;

        /// <summary>Затухание вращения в полёте.</summary>
        [SerializeField] private float _flightAngularDamping = 0.5f;

        /// <summary>Затухание вращения после первого касания: факел падает и больше не катается.</summary>
        [SerializeField] private float _landedAngularDamping = 8f;

        private const float LightSurfaceMargin = 0.15f;
        private const float MinProbeSpeed = 0.5f;

        private PlayerTorch _player;
        private PlayerStateController _state;
        private FireIgnition _fire;
        private TorchFlameWind _flameWind;
        private Transform _light;
        private Transform _flameAnchor;
        private Vector3 _lightLocalPosition;
        private Light _lightSource;
        private float _baseLightRange;
        private Vector3 _lightTakeOffset;
        private float _handBlend;
        private Rigidbody _body;
        private Collider[] _colliders;
        private Interactable _interactable;
        private TorchBurn _burn;
        private Vector3 _looseScale;
        private float _flame = 1f;
        private bool _held;
        private bool _wentOut;

        [Inject]
        private void Construct(PlayerTorch player, PlayerStateController state)
        {
            _player = player;
            _state = state;
        }

        private void Awake()
        {
            _fire = GetComponent<FireIgnition>();
            _flameWind = GetComponent<TorchFlameWind>();
            if (_fire.lights.Count > 0 && _fire.lights[0] != null)
            {
                _light = _fire.lights[0].transform;
                _lightLocalPosition = _light.localPosition;
                _lightSource = _fire.lights[0];
                _baseLightRange = _lightSource.range;
            }
            if (_fire.flames.Count > 0 && _fire.flames[0] != null) _flameAnchor = _fire.flames[0].transform;
            _body = GetComponent<Rigidbody>();
            _colliders = GetComponentsInChildren<Collider>(true);
            _interactable = GetComponentInChildren<Interactable>();
            _burn = new TorchBurn(_burnDuration, _flameStep);
            _looseScale = transform.localScale;
            _interactable.onInteraction.AddListener(OnInteract);
        }

        private void Update()
        {
            if (!_held)
                transform.localScale = Vector3.MoveTowards(transform.localScale, _looseScale, _looseScaleSpeed * Time.deltaTime);

            if (_wentOut) return;

            if (_state.CanMove()) _burn.Burn(Time.deltaTime);

            float flameStepShare = _flameStep / _burnDuration;
            _flame = Mathf.MoveTowards(_flame, _burn.FlameLevel, flameStepShare / _flameStepFade * Time.deltaTime);
            ApplyBurn();
        }

        // У стен холла есть односторонние коллайдеры: физика пропускает факел сквозь их изнанку.
        private void FixedUpdate()
        {
            if (_held) return;

            Vector3 velocity = _body.linearVelocity;
            float speed = velocity.magnitude;
            if (speed < MinProbeSpeed) return;

            Vector3 direction = velocity / speed;
            float reach = speed * Time.fixedDeltaTime + _backfaceProbe;
            if (RaycastTwoSided(_body.worldCenterOfMass, direction, reach, out RaycastHit hit) && Vector3.Dot(hit.normal, direction) > 0f)
                _body.linearVelocity = Vector3.Reflect(velocity, hit.normal) * _backfaceBounce;
        }

        // Смещение света задано для настенного факела: у лежащего оно уводило свет под пол и за стены.
        private void LateUpdate()
        {
            if (_wentOut || _light == null || _flameAnchor == null) return;

            Vector3 origin = _flameAnchor.position;
            if (_held)
            {
                // Свет идёт от места подбора к руке плавно: скачок смещения уводил его в пол и давал блик.
                _light.position = Vector3.Lerp(origin + _lightTakeOffset, transform.TransformPoint(_lightLocalPosition), _handBlend);
                return;
            }

            float lift = _looseLightLift;
            if (RaycastTwoSided(origin, Vector3.up, lift + LightSurfaceMargin, out RaycastHit hit))
                lift = Mathf.Max(0f, hit.distance - LightSurfaceMargin);

            _light.position = origin + Vector3.up * lift;
        }

        /// <summary>Луч, который видит и изнаночную сторону мешей. Триггеры игнорирует.</summary>
        public static bool RaycastTwoSided(Vector3 origin, Vector3 direction, float distance, out RaycastHit hit)
        {
            bool previous = Physics.queriesHitBackfaces;
            Physics.queriesHitBackfaces = true;
            bool found = Physics.Raycast(origin, direction, out hit, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            Physics.queriesHitBackfaces = previous;
            return found;
        }

        /// <summary>Переводит факел в режим «в руке»: физика и подбор выключены.</summary>
        public void TakeInHand()
        {
            _held = true;
            // Интерполяция кинематического тела перезаписывает позицию родителя и даёт дрожание в руке.
            _body.interpolation = RigidbodyInterpolation.None;
            _body.isKinematic = true;
            SetCollidersEnabled(false);
            _handBlend = 0f;
            if (_light != null && _flameAnchor != null) _lightTakeOffset = _light.position - _flameAnchor.position;
            if (_flameWind != null) _flameWind.SetCarrier(_player.Controller);
        }

        /// <summary>Доля перехода света в режим «в руке»: 0 в начале подбора, 1 когда факел уже в руке.</summary>
        public void SetHandBlend(float value) => _handBlend = Mathf.Clamp01(value);

        /// <summary>Отпускает факел в мир: он получает скорость и теряет время горения.</summary>
        public void Release(Vector3 position, Vector3 velocity, Vector3 spin)
        {
            _held = false;
            _handBlend = 0f;
            if (_flameWind != null) _flameWind.SetCarrier(null);
            transform.SetParent(null);
            transform.position = position;

            SetCollidersEnabled(true);
            // Сброс при выключении коллайдера: игнор ставим только после его включения.
            foreach (var part in _colliders) Physics.IgnoreCollision(_player.Controller, part);
            _body.isKinematic = false;
            _body.interpolation = RigidbodyInterpolation.Interpolate;
            _body.linearVelocity = velocity;
            _body.angularDamping = _flightAngularDamping;
            _body.angularVelocity = spin;

            _burn.Burn(_releasePenalty);
            _flame = _burn.FlameLevel;
            if (!_wentOut) ApplyBurn();
            _interactable.enabled = !_wentOut;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_held) _body.angularDamping = _landedAngularDamping;
        }

        private void SetCollidersEnabled(bool value)
        {
            foreach (var part in _colliders) part.enabled = value;
        }

        private void ApplyBurn()
        {
            if (_burn.IsBurnedOut)
            {
                _wentOut = true;
                _fire.SetLit(false);
                _interactable.enabled = false;
                return;
            }

            _fire.SetBurnLevel(_burn.Level * Mathf.Lerp(1f, _handLightBoost, _handBlend), _flame, _burn.Level);
            if (_lightSource != null) _lightSource.range = _baseLightRange * Mathf.Lerp(1f, _handLightRange, _handBlend);
        }

        private void OnInteract()
        {
            if (_wentOut || !_player.CanTake) return;
            _player.Take(this, Vector3.up);
        }
    }
}
