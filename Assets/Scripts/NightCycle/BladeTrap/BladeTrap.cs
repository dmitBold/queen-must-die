using System.Collections.Generic;
using Core;
using FMODUnity;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace NightCycle
{
    /// <summary>
    /// Ловушка клинков на тропе к трону. Begin выпускает первые клинки перед Бет и запускает фронт:
    /// он с ускорением идёт вдоль локальной +Z от начала зоны к её концу, а клинки лезут из пола, стен и потолка без пересечений.
    /// Касание выезжающего клинка убивает: затемнение и загрузка последнего сохранения (чекпоинта).
    /// Висит на объекте, внутри которого лежат первые клинки: попадания луча в своих детей ловушка считает клинками.
    /// </summary>
    public class BladeTrap : MonoBehaviour
    {
        private const string ShowTrigger = "show";
        // Имя стейта выезда в BLADE.controller.
        private static readonly int RiseState = Animator.StringToHash("New State");
        private const float BladeGap = 0.1f;
        private const int MaxOverlaps = 16;

        /// <summary>Центр зоны клинков в локальных осях объекта.</summary>
        [Header("Зона")]
        [SerializeField] private Vector3 _areaCenter;
        /// <summary>Размер зоны в локальных осях. Фронт идёт от -Z к +Z; дальний край по Z — граница перед троном, дальше клинки не появляются.</summary>
        [SerializeField] private Vector3 _areaSize = new Vector3(26f, 16f, 80f);

        /// <summary>Префаб одиночного клинка: выезжает вдоль своей оси Y из точки корня.</summary>
        [Header("Клинки")]
        [SerializeField] private GameObject _bladePrefab;
        /// <summary>Клинки сцены, которые выезжают перед Бет сразу при запуске ловушки.</summary>
        [SerializeField] private Animator[] _openingBlades;
        /// <summary>Видимая длина и толщина клинка префаба при масштабе 1, м. Нужны для раскладки без пересечений.</summary>
        [SerializeField, Min(0.1f)] private float _bladeLength = 8.8f;
        [SerializeField, Min(0.01f)] private float _bladeRadius = 0.7f;
        /// <summary>Разброс масштаба клинков.</summary>
        [SerializeField] private Vector2 _scaleRange = new Vector2(0.5f, 1.2f);
        /// <summary>Наибольший наклон клинка от перпендикуляра к поверхности, градусы.</summary>
        [SerializeField, Range(0f, 60f)] private float _maxTilt = 25f;
        /// <summary>Предел числа клинков фронта, чтобы не просела производительность. Клинки под догнанной Бет в него не входят.</summary>
        [SerializeField, Min(1)] private int _maxBlades = 800;
        /// <summary>Звук выезда клинка. Пусто — без звука.</summary>
        [SerializeField] private EventReference _bladeSound;

        /// <summary>Скорость фронта в начале и в конце разгона, м/с.</summary>
        [Header("Фронт")]
        [SerializeField, Min(0f)] private float _startSpeed = 5.1f;
        [SerializeField, Min(0f)] private float _endSpeed = 9.35f;
        /// <summary>За сколько секунд фронт разгоняется до конечной скорости.</summary>
        [SerializeField, Min(0.01f)] private float _accelerationTime = 20f;
        /// <summary>Сколько попыток поставить клинок приходится на метр продвижения фронта. Больше — плотнее.</summary>
        [SerializeField, Min(0f)] private float _attemptsPerMeter = 12f;
        /// <summary>Глубина полосы за фронтом, где клинки ещё добирают свободные места, м.</summary>
        [SerializeField, Min(0f)] private float _fillDepth = 4f;

        /// <summary>Длительность затемнения при смерти, сек.</summary>
        [Header("Смерть")]
        [SerializeField, Min(0f)] private float _deathFadeDuration = 0.5f;

        private readonly BladeLayout _layout = new BladeLayout(BladeGap);
        private readonly List<Animator> _rising = new List<Animator>();
        private readonly HashSet<Animator> _risingSet = new HashSet<Animator>();
        private readonly HashSet<Animator> _openingSet = new HashSet<Animator>();
        private readonly Collider[] _overlaps = new Collider[MaxOverlaps];

        private UIManager _uiManager;
        private SaveManager _saveManager;
        private SceneTransitionService _transitionService;
        private AudioService _audioService;
        private CharacterController _player;

        private bool _started;
        private bool _spawning;
        private bool _dead;
        private float _startTime;
        private float _frontZ;
        private float _attemptBudget;
        private int _spawned;

        [Inject]
        private void Construct(UIManager uiManager, SaveManager saveManager, SceneTransitionService transitionService,
            AudioService audioService, FirstPersonController player)
        {
            _uiManager = uiManager;
            _saveManager = saveManager;
            _transitionService = transitionService;
            _audioService = audioService;
            _player = player.GetComponent<CharacterController>();
        }

        /// <summary>Запускает ловушку: первые клинки и фронт. Повторный вызов ничего не делает.</summary>
        public void Begin()
        {
            if (_started) return;
            _started = true;
            _spawning = true;
            _startTime = Time.time;
            _frontZ = -_areaSize.z * 0.5f;

            foreach (var blade in _openingBlades)
            {
                if (blade == null) continue;
                _openingSet.Add(blade);
                Show(blade);
            }
        }

        /// <summary>Останавливает появление новых клинков. Уже выезжающие довыезжают и остаются опасными до конца выезда.</summary>
        public void StopSpawning()
        {
            _spawning = false;
        }

        private void Update()
        {
            if (!_started || _dead) return;

            UpdateRising();

            if (_spawning)
                AdvanceFront();

            if (_rising.Count > 0 && TouchesRisingBlade())
                Die();
        }

        private void Show(Animator blade)
        {
            blade.SetTrigger(ShowTrigger);
            _rising.Add(blade);
            _risingSet.Add(blade);
            _audioService.PlayFMODEvent(_bladeSound, blade.transform.position);
        }

        private void UpdateRising()
        {
            for (int i = _rising.Count - 1; i >= 0; i--)
            {
                Animator blade = _rising[i];
                if (IsRising(blade)) continue;

                _rising.RemoveAt(i);
                _risingSet.Remove(blade);
                // Выехавший клинок дальше просто стена: аниматор больше не нужен, поза остаётся.
                blade.enabled = false;

                // Первые клинки стоят в сцене вручную, их место узнаём по готовой позе.
                if (_openingSet.Contains(blade))
                    AddOpeningBladeToLayout(blade);
            }
        }

        private static bool IsRising(Animator blade)
        {
            if (blade.IsInTransition(0)) return true;

            AnimatorStateInfo state = blade.GetCurrentAnimatorStateInfo(0);
            // Пока стейт не сменился, триггер ещё не подхвачен: клинок только начинает выезд.
            return state.shortNameHash != RiseState || state.normalizedTime < 1f;
        }

        private void AddOpeningBladeToLayout(Animator blade)
        {
            foreach (var mesh in blade.GetComponentsInChildren<MeshFilter>())
            {
                if (mesh.sharedMesh == null) continue;

                // Капсула по самой длинной оси меша.
                Bounds bounds = mesh.sharedMesh.bounds;
                Vector3 extents = bounds.extents;
                int axis = extents.x >= extents.y && extents.x >= extents.z ? 0 : extents.y >= extents.z ? 1 : 2;
                Vector3 along = Vector3.zero;
                along[axis] = extents[axis];
                Vector3 across = extents;
                across[axis] = 0f;

                Transform t = mesh.transform;
                Vector3 from = t.TransformPoint(bounds.center - along);
                Vector3 to = t.TransformPoint(bounds.center + along);
                float radius = t.TransformVector(across).magnitude;
                _layout.Add(from, to, radius);
            }
        }

        private void AdvanceFront()
        {
            float halfLength = _areaSize.z * 0.5f;
            float speed = Mathf.Lerp(_startSpeed, _endSpeed, (Time.time - _startTime) / _accelerationTime);
            float previous = _frontZ;
            _frontZ = Mathf.Min(_frontZ + speed * Time.deltaTime, halfLength);
            _attemptBudget += (_frontZ - previous) * _attemptsPerMeter;

            while (_attemptBudget >= 1f && _spawned < _maxBlades)
            {
                _attemptBudget -= 1f;
                TrySpawnInFront();
            }

            // Клинки со стен не достают до середины коридора, а с потолка — до роста Бет.
            // Поэтому догнавший фронт сам выпускает клинок из пола под ней.
            if (PlayerLocalZ() <= _frontZ)
                TrySpawnUnderPlayer();

            if (_frontZ >= halfLength || _spawned >= _maxBlades)
                _spawning = false;
        }

        private void TrySpawnInFront()
        {
            Vector3 half = _areaSize * 0.5f;
            float z = Random.Range(Mathf.Max(_frontZ - _fillDepth, -half.z), _frontZ);
            Vector3 local = _areaCenter + new Vector3(Random.Range(-half.x, half.x), Random.Range(-half.y, half.y), z);
            Vector3 origin = transform.TransformPoint(local);
            Vector3 direction = transform.TransformDirection(PickSurfaceDirection());

            if (TryPlaceBlade(origin, direction, _maxTilt))
                _spawned++;
        }

        private void TrySpawnUnderPlayer()
        {
            Transform body = _player.transform;
            Vector3 feet = body.TransformPoint(_player.center) - body.up * (_player.height * 0.5f - _player.radius);
            Vector2 offset = Random.insideUnitCircle * _player.radius;
            // Луч из капсулы Бет её не замечает: так он попадает в пол прямо под ней.
            Vector3 origin = feet + body.right * offset.x + body.forward * offset.y;
            TryPlaceBlade(origin, Vector3.down, 0f);
        }

        private float PlayerLocalZ()
        {
            return (transform.InverseTransformPoint(_player.transform.position) - _areaCenter).z;
        }

        private bool TryPlaceBlade(Vector3 origin, Vector3 direction, float maxTilt)
        {
            if (!Physics.Raycast(origin, direction, out RaycastHit hit, _areaSize.magnitude, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                return false;

            // Клинок не растёт из другого клинка и из Бет.
            if (hit.transform.IsChildOf(transform) || hit.collider is CharacterController)
                return false;

            Vector3 up = TiltedNormal(hit.normal, maxTilt);
            float scale = Random.Range(_scaleRange.x, _scaleRange.y);
            Vector3 tip = hit.point + up * (_bladeLength * scale);
            float radius = _bladeRadius * scale;

            if (!IsInsideArea(tip) || !_layout.Fits(hit.point, tip, radius))
                return false;

            _layout.Add(hit.point, tip, radius);

            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, up) * Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);
            GameObject blade = Instantiate(_bladePrefab, hit.point, rotation, transform);
            blade.transform.localScale *= scale;

            Animator animator = blade.GetComponentInChildren<Animator>();
            if (animator != null)
                Show(animator);

            return true;
        }

        // Площадь пола и потолка пропорциональна ширине зоны, стен — высоте.
        private Vector3 PickSurfaceDirection()
        {
            float width = _areaSize.x;
            float height = _areaSize.y;
            float pick = Random.value * 2f * (width + height);

            if (pick < width) return Vector3.down;
            if (pick < 2f * width) return Vector3.up;
            if (pick < 2f * width + height) return Vector3.left;
            return Vector3.right;
        }

        private static Vector3 TiltedNormal(Vector3 normal, float maxTilt)
        {
            Vector3 axis = Vector3.Cross(normal, Random.onUnitSphere);
            if (axis.sqrMagnitude < 1e-4f) return normal;
            return Quaternion.AngleAxis(Random.Range(0f, maxTilt), axis.normalized) * normal;
        }

        private bool IsInsideArea(Vector3 worldPoint)
        {
            Vector3 local = transform.InverseTransformPoint(worldPoint) - _areaCenter;
            Vector3 half = _areaSize * 0.5f;
            return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z;
        }

        private bool TouchesRisingBlade()
        {
            // Клинки двигает аниматор без Rigidbody: без синхронизации запрос видит позу прошлого кадра.
            Physics.SyncTransforms();

            Transform body = _player.transform;
            Vector3 center = body.TransformPoint(_player.center);
            float offset = Mathf.Max(0f, _player.height * 0.5f - _player.radius);
            Vector3 up = body.up * offset;
            int count = Physics.OverlapCapsuleNonAlloc(center + up, center - up, _player.radius + _player.skinWidth,
                _overlaps, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                Animator blade = _overlaps[i].GetComponentInParent<Animator>();
                if (blade != null && _risingSet.Contains(blade))
                    return true;
            }

            return false;
        }

        private void Die()
        {
            _dead = true;
            _spawning = false;
            _uiManager.BeginCutscene();

            if (FadeController.Instance != null)
                FadeController.Instance.FadeToBlack(_deathFadeDuration, LoadCheckpoint);
            else
                LoadCheckpoint();
        }

        private void LoadCheckpoint()
        {
            if (!_saveManager.LoadGame())
                _transitionService.TryBegin(SceneManager.GetActiveScene().name);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.6f);
            Gizmos.DrawWireCube(_areaCenter, _areaSize);
        }
    }
}
