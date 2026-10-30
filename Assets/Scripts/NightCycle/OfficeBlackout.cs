using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NightCycle
{
    /// <summary>
    /// Затемнение кабинета при подборе статуэтки: свет, огонь свечей/каминов и свечение их материалов гаснут мгновенно,
    /// звучит колокол, через breathDelay статуя делает вдох, через lightsOnDelay всё плавно возвращается за lightsOnDuration.
    /// </summary>
    public class OfficeBlackout : MonoBehaviour
    {
        [Header("Тайминги, сек от подбора")]
        public float breathDelay = 2f;
        public float lightsOnDelay = 3f;
        public float lightsOnDuration = 1f;

        [Header("Что гаснет")]
        public List<Light> lights = new List<Light>();
        [Tooltip("Огонь свечей, каминов, жаровен (частицы)")]
        public List<ParticleSystemRenderer> flames = new List<ParticleSystemRenderer>();
        [Tooltip("Меши со светящимся материалом (emission или unlit): воск, фитили, огонь")]
        public List<Renderer> glowRenderers = new List<Renderer>();

        [Header("Звук и события")]
        [Tooltip("Interactable, который играет колокол (его interact_sound)")]
        public Interactable bell;
        public UnityEvent onBlackout;
        public UnityEvent onBreath;
        public UnityEvent onLightsOn;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int TintColorId = Shader.PropertyToID("_TintColor");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly int EmissiveStrengthId = Shader.PropertyToID("_EmissiveStrength");   // Shader Graphs/S_BasicTextured
        private static readonly int UseEmissiveId = Shader.PropertyToID("_UseEmissive");

        private struct LightState { public Light light; public float intensity; public bool enabled; }
        private struct ColorSlot { public Renderer renderer; public int index; public int prop; public Color color; }
        private struct FloatSlot { public Renderer renderer; public int index; public int prop; public float value; }
        private readonly List<FloatSlot> _floats = new List<FloatSlot>();

        private readonly List<LightState> _lights = new List<LightState>();
        private readonly List<ColorSlot> _slots = new List<ColorSlot>();
        private readonly Dictionary<Renderer, bool> _rendererEnabled = new Dictionary<Renderer, bool>();   // flames: hidden while dark
        private readonly HashSet<Renderer> _glowSeen = new HashSet<Renderer>();   // glowing meshes: only their glow fades
        private MaterialPropertyBlock _mpb;
        private bool _running;

        public void Trigger() => Run(null);
        public void TriggerAndWake(StatueBreath statue) => Run(statue);

        private void Run(StatueBreath statue)
        {
            if (_running) return;
            _running = true;
            StartCoroutine(Sequence(statue));
        }

        private IEnumerator Sequence(StatueBreath statue)
        {
            Capture();
            SetLevel(0f);
            if (bell != null) bell.Interact();
            onBlackout?.Invoke();

            yield return new WaitForSeconds(breathDelay);
            if (statue != null) statue.Breathe();
            onBreath?.Invoke();

            yield return new WaitForSeconds(Mathf.Max(0f, lightsOnDelay - breathDelay));
            onLightsOn?.Invoke();
            float t = 0f;
            while (t < lightsOnDuration)
            {
                t += Time.deltaTime;
                SetLevel(Mathf.SmoothStep(0f, 1f, t / lightsOnDuration));
                yield return null;
            }
            Restore();
            _running = false;
        }

        private void Capture()
        {
            _mpb ??= new MaterialPropertyBlock();
            _lights.Clear(); _slots.Clear(); _floats.Clear(); _rendererEnabled.Clear(); _glowSeen.Clear();
            foreach (var l in lights)
                if (l != null) _lights.Add(new LightState { light = l, intensity = l.intensity, enabled = l.enabled });

            foreach (var r in flames) AddSlots(r, true);
            foreach (var r in glowRenderers) AddSlots(r, false);
        }

        private void AddSlots(Renderer r, bool tintEverything)
        {
            if (r == null || _rendererEnabled.ContainsKey(r) || _glowSeen.Contains(r)) return;
            if (tintEverything) _rendererEnabled[r] = r.enabled; else _glowSeen.Add(r);
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                var m = mats[i];
                if (m == null) continue;
                bool emissive = m.HasProperty(EmissionColorId) && m.IsKeywordEnabled("_EMISSION");
                bool unlit = tintEverything || m.shader.name.ToLower().Contains("unlit") || m.shader.name.ToLower().Contains("particle");
                if (m.HasProperty(EmissiveStrengthId) && (!m.HasProperty(UseEmissiveId) || m.GetFloat(UseEmissiveId) > 0.5f))
                    _floats.Add(new FloatSlot { renderer = r, index = i, prop = EmissiveStrengthId, value = m.GetFloat(EmissiveStrengthId) });
                if (emissive) _slots.Add(new ColorSlot { renderer = r, index = i, prop = EmissionColorId, color = m.GetColor(EmissionColorId) });
                if (unlit)
                    foreach (var id in new[] { BaseColorId, ColorId, TintColorId })
                        if (m.HasProperty(id)) _slots.Add(new ColorSlot { renderer = r, index = i, prop = id, color = m.GetColor(id) });
            }
        }

        private void SetLevel(float k)
        {
            bool on = k > 0.001f;
            foreach (var s in _lights)
            {
                if (s.light == null) continue;
                s.light.intensity = s.intensity * k;
                s.light.enabled = s.enabled && on;
            }
            foreach (var kv in _rendererEnabled)
                if (kv.Key != null) kv.Key.enabled = kv.Value && on;
            foreach (var s in _slots)
            {
                if (s.renderer == null) continue;
                s.renderer.GetPropertyBlock(_mpb, s.index);
                var c = s.color * k;
                if (s.prop != EmissionColorId) c.a = s.color.a * k;
                _mpb.SetColor(s.prop, c);
                s.renderer.SetPropertyBlock(_mpb, s.index);
            }
            foreach (var f in _floats)
            {
                if (f.renderer == null) continue;
                f.renderer.GetPropertyBlock(_mpb, f.index);
                _mpb.SetFloat(f.prop, f.value * k);
                f.renderer.SetPropertyBlock(_mpb, f.index);
            }
        }

        private void Restore()
        {
            foreach (var s in _lights)
                if (s.light != null) { s.light.intensity = s.intensity; s.light.enabled = s.enabled; }
            foreach (var kv in _rendererEnabled)
                if (kv.Key != null) kv.Key.enabled = kv.Value;
            foreach (var s in _slots)
                if (s.renderer != null) s.renderer.SetPropertyBlock(null, s.index);
            foreach (var f in _floats)
                if (f.renderer != null) f.renderer.SetPropertyBlock(null, f.index);
        }

        [ContextMenu("Test blackout")] private void TestBlackout() => Trigger();
    }
}
