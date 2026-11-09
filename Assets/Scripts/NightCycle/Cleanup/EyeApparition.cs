using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace NightCycle
{
    /// <summary>
    /// Глаз, появляющийся за спиной игрока, пока в HoMScene погашен свет.
    /// Слежение за игроком — EyeConstraint и AimConstraint на этом же объекте.
    /// </summary>
    public class EyeApparition : MonoBehaviour
    {
        private const int OverlapBufferSize = 32;
        private const float AngleStep = 15f;
        private const float DistanceStep = 0.5f;
        private const float RetryInterval = 0.25f;
        private const float NavMeshSnapRadius = 0.4f;
        private const float NavMeshFeetSearchRadius = 3f;
        private const float MaxFloorHeightDifference = 0.6f;

        private static readonly int DissolveId = Shader.PropertyToID("_Dissolve");

        [SerializeField] private Renderer _renderer;

        [Header("Точка появления")]
        [Tooltip("Минимальное расстояние от игрока, м")]
        [SerializeField] private float _minDistance = 2f;
        [Tooltip("Максимальное расстояние от игрока, м")]
        [SerializeField] private float _maxDistance = 4.5f;
        [Tooltip("Насколько можно отклониться от направления строго за спину, градусы. Больше 110 — глаз может появиться в поле зрения")]
        [SerializeField] private float _maxAngleFromBack = 110f;
        [Tooltip("Высота центра глаза над полом, м")]
        [SerializeField] private float _eyeHeight = 1.5f;
        [Tooltip("Радиус глаза с запасом: в этом радиусе не должно быть геометрии, м")]
        [SerializeField] private float _clearRadius = 0.5f;
        [Tooltip("Минимальное расстояние от края NavMesh, то есть от стен, м")]
        [SerializeField] private float _wallMargin = 0.45f;
        [Tooltip("Предметы крупнее этого размера не проверяются по bounds: это стены и пол, у них есть коллайдеры, м")]
        [SerializeField] private float _maxPropSize = 3f;

        [Header("Поведение")]
        [Tooltip("Если игрок подошёл ближе, глаз перелетает за спину, м")]
        [SerializeField] private float _keepOutDistance = 1.5f;
        [Tooltip("Длительность появления, сек")]
        [SerializeField] private float _appearTime = 0.6f;
        [Tooltip("Длительность исчезновения при перелёте, сек")]
        [SerializeField] private float _relocateFadeTime = 0.35f;
        [Tooltip("Длительность финального растворения, сек")]
        [SerializeField] private float _evaporateTime = 1.8f;

        private readonly Collider[] _overlaps = new Collider[OverlapBufferSize];
        private readonly RaycastHit[] _rayHits = new RaycastHit[OverlapBufferSize];
        private readonly List<Bounds> _propBounds = new List<Bounds>();

        private FirstPersonController _firstPerson;
        private CleanupManager _cleanupManager;
        private Transform _body;
        private Transform _playerRoot;
        private MaterialPropertyBlock _block;
        private Coroutine _routine;
        private float _dissolve = 1f;
        private bool _warned;

        [Inject]
        public void Construct(FirstPersonController firstPerson, CleanupManager cleanupManager)
        {
            _firstPerson = firstPerson;
            _cleanupManager = cleanupManager;
        }

        private void Start()
        {
            _block = new MaterialPropertyBlock();
            // Тело, а не Camera.main: во вступлении и катсценах камера стоит отдельно от игрока.
            _body = _firstPerson.transform;
            _playerRoot = _body.root;
            ApplyDissolve(1f);
            _cleanupManager.BlackoutCompleted.AddListener(Appear);
            _cleanupManager.LightsRestored.AddListener(Evaporate);
        }

        private void OnDestroy()
        {
            if (_cleanupManager == null) return;
            _cleanupManager.BlackoutCompleted.RemoveListener(Appear);
            _cleanupManager.LightsRestored.RemoveListener(Evaporate);
        }

        private void Appear()
        {
            Restart(HauntRoutine());
        }

        private void Evaporate()
        {
            Restart(DissolveRoutine(1f, _evaporateTime));
        }

        private void Restart(IEnumerator routine)
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(routine);
        }

        private IEnumerator HauntRoutine()
        {
            while (true)
            {
                Vector3 spot;
                while (!TryFindSpot(out spot))
                {
                    if (!_warned)
                    {
                        _warned = true;
                        Debug.LogWarning("EyeApparition: нет подходящей точки за спиной игрока, ищем дальше.", this);
                    }
                    yield return new WaitForSeconds(RetryInterval);
                }

                transform.position = spot;
                yield return DissolveRoutine(0f, _appearTime);

                while (Vector3.ProjectOnPlane(_body.position - transform.position, Vector3.up).sqrMagnitude > _keepOutDistance * _keepOutDistance)
                    yield return null;

                yield return DissolveRoutine(1f, _relocateFadeTime);
            }
        }

        private IEnumerator DissolveRoutine(float target, float duration)
        {
            float from = _dissolve;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                ApplyDissolve(Mathf.Lerp(from, target, t / duration));
                yield return null;
            }
            ApplyDissolve(target);
        }

        private void ApplyDissolve(float value)
        {
            _dissolve = value;
            _renderer.enabled = value < 1f;
            _renderer.GetPropertyBlock(_block);
            _block.SetFloat(DissolveId, value);
            _renderer.SetPropertyBlock(_block);
        }

        /// <summary>
        /// Ищет точку за спиной: сначала строго сзади и подальше, затем всё шире и ближе.
        /// </summary>
        private bool TryFindSpot(out Vector3 spot)
        {
            spot = default;
            if (!NavMesh.SamplePosition(_body.position, out NavMeshHit feet, NavMeshFeetSearchRadius, NavMesh.AllAreas))
                return false;

            Vector3 back = -Vector3.ProjectOnPlane(_body.forward, Vector3.up).normalized;

            CollectPropBounds();
            float sign = Random.value < 0.5f ? -1f : 1f;
            for (float angle = 0f; angle <= _maxAngleFromBack; angle += AngleStep)
            {
                for (int side = 0; side < (angle > 0f ? 2 : 1); side++)
                {
                    Vector3 direction = Quaternion.AngleAxis(angle * sign * (side == 0 ? 1f : -1f), Vector3.up) * back;
                    for (float distance = _maxDistance; distance >= _minDistance; distance -= DistanceStep)
                    {
                        if (IsValidSpot(feet.position, feet.position + direction * distance, out spot)) return true;
                    }
                }
            }
            return false;
        }

        private bool IsValidSpot(Vector3 feet, Vector3 candidate, out Vector3 eyePoint)
        {
            eyePoint = default;

            // NavMesh отсекает точки вне комнаты; прямая по NavMesh — что между точками нет стены.
            if (!NavMesh.SamplePosition(candidate, out NavMeshHit ground, NavMeshSnapRadius, NavMesh.AllAreas)) return false;
            if (Mathf.Abs(ground.position.y - feet.y) > MaxFloorHeightDifference) return false;
            if (NavMesh.Raycast(feet, ground.position, out _, NavMesh.AllAreas)) return false;
            if (NavMesh.FindClosestEdge(ground.position, out NavMeshHit edge, NavMesh.AllAreas) && edge.distance < _wallMargin) return false;

            Vector3 view = feet + Vector3.up * _eyeHeight;
            eyePoint = ground.position + Vector3.up * _eyeHeight;

            int count = Physics.OverlapSphereNonAlloc(eyePoint, _clearRadius, _overlaps, ~0, QueryTriggerInteraction.Ignore);
            if (count == OverlapBufferSize) return false;
            for (int i = 0; i < count; i++)
            {
                if (_overlaps[i].transform.root != _playerRoot && !_overlaps[i].transform.IsChildOf(transform)) return false;
            }

            Vector3 toEye = eyePoint - view;
            int hits = Physics.RaycastNonAlloc(view, toEye.normalized, _rayHits, toEye.magnitude, ~0, QueryTriggerInteraction.Ignore);
            if (hits == OverlapBufferSize) return false;
            for (int i = 0; i < hits; i++)
            {
                if (_rayHits[i].transform.root != _playerRoot) return false;
            }

            float clearSqr = _clearRadius * _clearRadius;
            foreach (Bounds bounds in _propBounds)
            {
                if (bounds.SqrDistance(eyePoint) < clearSqr) return false;
            }
            return true;
        }

        // У части реквизита нет коллайдеров, физика его не увидит. Собираем заново при каждом
        // поиске: убранные игроком предметы к этому моменту уже выключены.
        private void CollectPropBounds()
        {
            _propBounds.Clear();
            foreach (MeshRenderer mesh in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if (mesh == _renderer || !mesh.enabled || mesh.transform.root == _playerRoot) continue;
                Bounds bounds = mesh.bounds;
                if (Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) <= _maxPropSize) _propBounds.Add(bounds);
            }
        }
    }
}
