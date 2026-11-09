using UnityEngine;

namespace NightCycle
{
    /// <summary>
    /// Отклоняет пламя факела против движения, как встречный ветер: резкий поворот камеры сильно, обычная ходьба слабо.
    /// </summary>
    [RequireComponent(typeof(FireIgnition))]
    public class TorchFlameWind : MonoBehaviour
    {
        /// <summary>Сила ветра на каждый м/с движения огня от поворота камеры, броска и падения.</summary>
        [SerializeField] private float _swingGain = 0.3f;

        /// <summary>Сила ветра на каждый м/с ходьбы игрока с факелом.</summary>
        [SerializeField] private float _walkGain = 0.06f;

        /// <summary>Предел силы ветра: больше пламя отрывается от факела.</summary>
        [SerializeField] private float _maxWind = 0.8f;

        /// <summary>Как быстро пламя догоняет ветер.</summary>
        [SerializeField] private float _stiffness = 70f;

        /// <summary>Затухание колебаний: чем меньше, тем дольше пламя качается после рывка.</summary>
        [SerializeField] private float _damping = 10f;

        /// <summary>Доля случайных порывов от текущей силы ветра.</summary>
        [Range(0f, 1f)]
        [SerializeField] private float _flutter = 0.3f;

        private const float FlutterSpeed = 9f;
        private const float SpringStep = 0.01f;
        private const float MaxSpringTime = 0.1f;
        private static readonly Quaternion UprightRotation = Quaternion.Euler(-90f, 0f, 0f);

        private FireIgnition _fire;
        private CharacterController _carrier;
        private Transform _flameAnchor;
        private Vector3 _lastPosition;
        private Vector3 _wind;
        private Vector3 _windVelocity;
        private float _flutterSeed;

        private void Awake()
        {
            _fire = GetComponent<FireIgnition>();
            _flutterSeed = Random.value * 100f;
            if (_fire.flames.Count == 0 || _fire.flames[0] == null) return;

            _flameAnchor = _fire.flames[0].transform;
            _lastPosition = _flameAnchor.position;

            foreach (var flame in _fire.flames)
            {
                if (flame == null) continue;
                var velocity = flame.velocityOverLifetime;
                velocity.enabled = true;
                velocity.space = ParticleSystemSimulationSpace.World;
            }
        }

        /// <summary>Задаёт контроллер игрока, который несёт факел; null — факел свободен.</summary>
        public void SetCarrier(CharacterController carrier) => _carrier = carrier;

        private void LateUpdate()
        {
            KeepFlamesUpright();
            if (_flameAnchor == null || Time.deltaTime <= 0f) return;

            Vector3 flameVelocity = (_flameAnchor.position - _lastPosition) / Time.deltaTime;
            _lastPosition = _flameAnchor.position;

            Vector3 carrierVelocity = _carrier != null ? _carrier.velocity : Vector3.zero;
            Vector3 target = Vector3.ClampMagnitude(-(flameVelocity - carrierVelocity) * _swingGain - carrierVelocity * _walkGain, _maxWind);

            StepSpring(target, Mathf.Min(Time.deltaTime, MaxSpringTime));

            float flutter = 1f + _flutter * (Mathf.PerlinNoise(_flutterSeed, Time.time * FlutterSpeed) * 2f - 1f);
            ApplyWind(_wind * flutter);
        }

        // Пружина вместо простого сглаживания: после рывка пламя немного проскакивает и качается обратно.
        // Мелкие шаги нужны для устойчивости: на целом кадре при низком FPS пружина расходится.
        private void StepSpring(Vector3 target, float time)
        {
            for (; time > 0f; time -= SpringStep)
            {
                float step = Mathf.Min(time, SpringStep);
                _windVelocity += ((target - _wind) * _stiffness - _windVelocity * _damping) * step;
                _wind += _windVelocity * step;
            }
        }

        // Частицы живут в локальном пространстве эмиттера: без этого пламя крутится вместе с факелом.
        private void KeepFlamesUpright()
        {
            foreach (var flame in _fire.flames)
                if (flame != null) flame.transform.rotation = UprightRotation;
        }

        private void ApplyWind(Vector3 wind)
        {
            foreach (var flame in _fire.flames)
            {
                if (flame == null) continue;
                var velocity = flame.velocityOverLifetime;
                velocity.x = wind.x;
                velocity.y = wind.y;
                velocity.z = wind.z;
            }
        }
    }
}
