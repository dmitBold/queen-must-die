using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Бюджет realtime-теней: тени остаются только у N ближайших к камере огней,
    /// у остальных Light.shadows = None. Исходный режим теней запоминается и возвращается при выключении компонента.
    /// Не привязан к сцене: ни одного имени объекта, только правила.
    /// Управляются Point/Spot огни не в режиме Baked, у которых тени включены изначально.
    ///
    /// Огонь не претендует на слот, если он слабее порога по Intensity, больше порога по Range
    /// или целиком дальше QualitySettings.shadowDistance от камеры (тень всё равно не видна).
    /// Тег Ignore Tag на свете или на его родителе: такой огонь бюджет не трогает и не учитывает.
    ///
    /// Огни кэшируются и перечитываются раз в Rescan Interval, ранжирование идёт раз в Rank Interval
    /// по кэшу, FindObjectsByType в каждом кадре не вызывается.
    /// </summary>
    [DisallowMultipleComponent]
    public class ShadowLightBudget : MonoBehaviour
    {
        [Header("Бюджет")]
        [Tooltip("Сколько огней одновременно могут отбрасывать realtime-тени")]
        [SerializeField, Min(0)] private int _maxShadowedLights = 6;
        [Tooltip("От чего считать расстояние. Пусто — основная камера (Camera.main)")]
        [SerializeField] private Transform _focus;

        [Header("Правила допуска")]
        [Tooltip("Огни слабее этого порога теней не получают. 0 — правило выключено")]
        [SerializeField, Min(0f)] private float _minIntensity = 3f;
        [Tooltip("Огни с Range больше порога теней не получают. 0 — правило выключено")]
        [SerializeField, Min(0f)] private float _maxRange = 40f;
        [Tooltip("Огни, чья сфера действия целиком дальше QualitySettings.shadowDistance от камеры, теней не получают")]
        [SerializeField] private bool _cullBeyondShadowDistance = true;
        [Tooltip("Тег на огне или его родителе: бюджет такой огонь не трогает")]
        [SerializeField] private string _ignoreTag = "IgnoreShadowBudget";

        [Header("Частота обновления")]
        [Tooltip("Как часто пересчитывать, каким огням оставить тени, с")]
        [SerializeField, Min(0.05f)] private float _rankInterval = 0.2f;
        [Tooltip("Как часто заново искать огни в сцене (комнаты включаются и выключаются), с")]
        [SerializeField, Min(0.5f)] private float _rescanInterval = 2f;
        [Tooltip("Смещение камеры с прошлого пересчёта, при котором пересчёт делается сразу (телепорт), м")]
        [SerializeField, Min(0.5f)] private float _jumpDistance = 4f;
        [Tooltip("Защита от мерцания: у огня, который уже со слотом, расстояние умножается на это число")]
        [SerializeField, Range(0.3f, 1f)] private float _stickiness = 0.8f;

        private sealed class Entry
        {
            public int Id;
            public Light Light;
            public LightShadows Original;
            public bool Ignored;
            public bool Applied;
            public float Score;
        }

        private static readonly Comparison<Entry> ByScore = (a, b) => a.Score.CompareTo(b.Score);

        private readonly Dictionary<int, Entry> _byId = new Dictionary<int, Entry>();
        private readonly List<Entry> _entries = new List<Entry>();
        private readonly List<Entry> _ranked = new List<Entry>();

        private Camera _camera;
        private Vector3 _lastRankPosition;
        private float _nextRescan;
        private float _nextRank;
        private bool _reported;

        /// <summary>Сколько огней сейчас реально с тенями под управлением бюджета.</summary>
        public int ShadowedCount { get; private set; }

        /// <summary>Сколько огней вообще под управлением бюджета (без тега-исключения).</summary>
        public int ManagedCount { get; private set; }

        /// <summary>Пересчитать бюджет в ближайшем кадре (например, после включения комнаты).</summary>
        public void ForceRefresh()
        {
            _nextRescan = 0f;
            _nextRank = 0f;
        }

        private void OnEnable()
        {
            _reported = false;
            ForceRefresh();
        }

        private void OnDisable()
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                Entry e = _entries[i];
                if (e.Light != null && !e.Ignored)
                {
                    e.Light.shadows = e.Original;
                }
            }

            _entries.Clear();
            _byId.Clear();
            _ranked.Clear();
            ShadowedCount = 0;
            ManagedCount = 0;
        }

        private void Update()
        {
            float now = Time.unscaledTime;

            if (now >= _nextRescan)
            {
                Rescan();
                _nextRescan = now + _rescanInterval;
                _nextRank = 0f;
            }

            Transform focus = ResolveFocus();
            if (focus == null)
            {
                return;
            }

            Vector3 position = focus.position;
            bool jumped = (position - _lastRankPosition).sqrMagnitude > _jumpDistance * _jumpDistance;
            if (now < _nextRank && !jumped)
            {
                return;
            }

            Rank(position);
            _lastRankPosition = position;
            _nextRank = now + _rankInterval;
        }

        private Transform ResolveFocus()
        {
            if (_focus != null)
            {
                return _focus;
            }

            if (_camera == null)
            {
                _camera = Camera.main;
            }

            return _camera != null ? _camera.transform : null;
        }

        private void Rescan()
        {
            Light[] found = FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < found.Length; i++)
            {
                Light light = found[i];
                if (!IsManageable(light) || _byId.ContainsKey(light.GetInstanceID()))
                {
                    continue;
                }

                var entry = new Entry
                {
                    Id = light.GetInstanceID(),
                    Light = light,
                    Original = light.shadows,
                    Ignored = HasIgnoreTag(light),
                    Applied = true,
                };
                _byId.Add(entry.Id, entry);
                _entries.Add(entry);
            }

            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                if (_entries[i].Light == null)
                {
                    _byId.Remove(_entries[i].Id);
                    _entries.RemoveAt(i);
                }
            }
        }

        private static bool IsManageable(Light light)
        {
            return light.type != LightType.Directional
                && light.lightmapBakeType != LightmapBakeType.Baked
                && light.shadows != LightShadows.None;
        }

        private bool HasIgnoreTag(Light light)
        {
            if (string.IsNullOrEmpty(_ignoreTag))
            {
                return false;
            }

            try
            {
                for (Transform t = light.transform; t != null; t = t.parent)
                {
                    if (t.CompareTag(_ignoreTag))
                    {
                        return true;
                    }
                }
            }
            catch (UnityException)
            {
                // Тег не заведён в проекте: правило-исключение просто не работает.
                _ignoreTag = null;
            }

            return false;
        }

        private bool IsEligible(Light light, Vector3 camera, float shadowDistance)
        {
            if (_minIntensity > 0f && light.intensity < _minIntensity)
            {
                return false;
            }

            if (_maxRange > 0f && light.range > _maxRange)
            {
                return false;
            }

            if (_cullBeyondShadowDistance && shadowDistance > 0f
                && Vector3.Distance(light.transform.position, camera) - light.range > shadowDistance)
            {
                return false;
            }

            return true;
        }

        private void Rank(Vector3 camera)
        {
            float shadowDistance = QualitySettings.shadowDistance;
            _ranked.Clear();
            int managed = 0;

            for (int i = 0; i < _entries.Count; i++)
            {
                Entry e = _entries[i];
                Light light = e.Light;
                if (light == null || e.Ignored)
                {
                    continue;
                }

                managed++;
                if (!light.isActiveAndEnabled)
                {
                    continue;
                }

                if (!IsEligible(light, camera, shadowDistance))
                {
                    SetShadows(e, false);
                    continue;
                }

                float distance = Vector3.Distance(light.transform.position, camera);
                e.Score = e.Applied ? distance * _stickiness : distance;
                _ranked.Add(e);
            }

            _ranked.Sort(ByScore);

            int shadowed = 0;
            for (int i = 0; i < _ranked.Count; i++)
            {
                bool keep = i < _maxShadowedLights;
                SetShadows(_ranked[i], keep);
                if (keep)
                {
                    shadowed++;
                }
            }

            ManagedCount = managed;
            ShadowedCount = shadowed;

            if (!_reported)
            {
                _reported = true;
                Debug.Log($"[ShadowLightBudget] управляется огней: {managed}, "
                    + $"исключено тегом: {_entries.Count - managed}, с тенями сейчас: {shadowed} (бюджет {_maxShadowedLights}).");
            }
        }

        private static void SetShadows(Entry entry, bool enabled)
        {
            if (entry.Applied == enabled)
            {
                return;
            }

            entry.Applied = enabled;
            entry.Light.shadows = enabled ? entry.Original : LightShadows.None;
        }
    }
}
