using System.Collections.Generic;
using UnityEngine;

namespace NightCycle
{
    /// <summary>
    /// Рой над ульем: летает, облетает препятствия и огонь, волнуется частицами. Решения принимает Hive.
    /// </summary>
    public class HiveSwarm : MonoBehaviour
    {
        private const int CastBufferSize = 8;
        private const float MinStep = 0.0001f;
        private const float WobbleFrequency = 0.6f;

        [SerializeField] private ParticleSystem _particles;

        /// <summary>Скорость роя в погоне, м/с.</summary>
        [Header("Полёт")]
        [SerializeField] private float _chaseSpeed = 7f;
        /// <summary>Скорость возврата к улью и покачивания над ним, м/с.</summary>
        [SerializeField] private float _returnSpeed = 3f;
        /// <summary>Размах покачивания над ульем в покое и при наибольшей тревоге, м.</summary>
        [SerializeField] private float _calmWobble = 0.15f;
        [SerializeField] private float _alarmWobble = 0.6f;
        /// <summary>Радиус роя для облёта препятствий, м.</summary>
        [SerializeField] private float _bodyRadius = 0.3f;
        /// <summary>Во что рой не влетает. Слой Ignore Raycast (факелы, триггеры) по умолчанию не входит.</summary>
        [SerializeField] private LayerMask _obstacleMask = Physics.DefaultRaycastLayers;

        /// <summary>Сила шума частиц и скорость их симуляции: в покое и в атаке.</summary>
        [Header("Частицы")]
        [SerializeField] private float _calmNoiseStrength = 0.3f;
        [SerializeField] private float _angryNoiseStrength = 1.5f;
        [SerializeField] private float _angrySimulationSpeed = 2.5f;

        private readonly RaycastHit[] _hits = new RaycastHit[CastBufferSize];
        private Transform _ignoreRoot;
        private float _wobbleSeed;
        private int _detourSide;

        /// <summary>Где сейчас рой.</summary>
        public Vector3 Position => transform.position;

        private void Awake()
        {
            _wobbleSeed = Random.value * 100f;
        }

        /// <summary>Коллайдеры под этим корнем (игрок) рой не облетает.</summary>
        public void SetIgnoredRoot(Transform root) => _ignoreRoot = root;

        /// <summary>Тревога роя от 0 (спокоен) до 1 (атакует): сила дрожания частиц.</summary>
        public void SetAgitation(float agitation)
        {
            if (_particles == null) return;
            agitation = Mathf.Clamp01(agitation);
            var noise = _particles.noise;
            noise.strengthMultiplier = Mathf.Lerp(_calmNoiseStrength, _angryNoiseStrength, agitation);
            var main = _particles.main;
            main.simulationSpeed = Mathf.Lerp(1f, _angrySimulationSpeed, agitation);
        }

        /// <summary>Держится над точкой улья и покачивается тем сильнее, чем выше тревога.</summary>
        public void Hover(Vector3 home, float agitation)
        {
            float wobble = Mathf.Lerp(_calmWobble, _alarmWobble, Mathf.Clamp01(agitation));
            float t = Time.time * WobbleFrequency * Mathf.Lerp(1f, 3f, agitation);
            var offset = new Vector3(
                Mathf.PerlinNoise(_wobbleSeed, t) - 0.5f,
                Mathf.PerlinNoise(_wobbleSeed + 10f, t) - 0.5f,
                Mathf.PerlinNoise(_wobbleSeed + 20f, t) - 0.5f) * (2f * wobble);
            transform.position = Vector3.MoveTowards(transform.position, home + offset, _returnSpeed * Time.deltaTime);
        }

        /// <summary>Летит к точке улья. Возвращает true, когда долетел.</summary>
        public bool ReturnHome(Vector3 home, Vector3 zoneCenter, float zoneRadius)
        {
            Fly(home, _returnSpeed, zoneCenter, zoneRadius, null, 0f);
            return (transform.position - home).sqrMagnitude <= _calmWobble * _calmWobble;
        }

        /// <summary>
        /// Летит к цели, не покидая зону улья и не залетая в радиус огня.
        /// </summary>
        public void Chase(Vector3 target, Vector3 zoneCenter, float zoneRadius, List<Vector3> fires, float fireRadius)
        {
            Fly(target, _chaseSpeed, zoneCenter, zoneRadius, fires, fireRadius);
        }

        private void Fly(Vector3 target, float speed, Vector3 zoneCenter, float zoneRadius, List<Vector3> fires, float fireRadius)
        {
            Vector3 position = transform.position;
            Vector3 toTarget = target - position;
            float step = Mathf.Min(speed * Time.deltaTime, toTarget.magnitude);
            if (step < MinStep) return;

            Vector3 next = position + AvoidObstacles(position, toTarget.normalized, step) * step;

            // Сфера огня выталкивает рой на свою поверхность: так он скользит вокруг пламени, а не сквозь него.
            if (fires != null)
            {
                foreach (Vector3 fire in fires)
                {
                    Vector3 fromFire = next - fire;
                    if (fromFire.sqrMagnitude < fireRadius * fireRadius)
                        next = fire + (fromFire.sqrMagnitude > MinStep ? fromFire.normalized : Vector3.up) * fireRadius;
                }
            }

            Vector3 fromCenter = next - zoneCenter;
            if (fromCenter.sqrMagnitude > zoneRadius * zoneRadius)
                next = zoneCenter + fromCenter.normalized * zoneRadius;

            transform.position = next;
        }

        // Упёршись в стену, рой идёт вдоль неё по горизонтали и держит выбранную сторону, пока путь не освободится:
        // скольжение по плоскости стены уводило его к полу, и он дрожал на месте. В глубоком углу может застрять.
        private Vector3 AvoidObstacles(Vector3 position, Vector3 direction, float step)
        {
            float reach = step + _bodyRadius;
            if (!TryCast(position, direction, reach, out RaycastHit hit))
            {
                _detourSide = 0;
                return direction;
            }

            Vector3 along = Vector3.Cross(Vector3.up, hit.normal);
            // Пол или потолок: вдоль них просто скользим.
            if (along.sqrMagnitude < 0.01f)
            {
                Vector3 slide = Vector3.ProjectOnPlane(direction, hit.normal).normalized;
                return TryCast(position, slide, reach, out _) ? Vector3.zero : slide;
            }

            along.Normalize();
            if (_detourSide == 0) _detourSide = Vector3.Dot(along, direction) >= 0f ? 1 : -1;
            Vector3 side = along * _detourSide;
            if (!TryCast(position, side, reach, out _)) return side;

            _detourSide = -_detourSide;
            return TryCast(position, -side, reach, out _) ? Vector3.zero : -side;
        }

        private bool TryCast(Vector3 origin, Vector3 direction, float distance, out RaycastHit nearest)
        {
            nearest = default;
            int count = Physics.SphereCastNonAlloc(origin, _bodyRadius, direction, _hits, distance, _obstacleMask, QueryTriggerInteraction.Ignore);
            float best = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = _hits[i];
                // Нулевая дистанция — рой уже касается коллайдера: такой удар не задаёт направления.
                if (hit.distance <= 0f) continue;
                if (_ignoreRoot != null && hit.transform.IsChildOf(_ignoreRoot)) continue;
                if (hit.distance < best)
                {
                    best = hit.distance;
                    nearest = hit;
                }
            }
            return best < float.MaxValue;
        }
    }
}
