using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NightCycle
{
    /// <summary>
    /// Разгорание / затухание огня (факел, жаровня, камин): свет, частицы пламени, дым, свечение меша и звук.
    /// Ignite() — огонь "схватывается": вспышка искры, провал, разгорание с небольшим перехлёстом, пламя растёт с нуля.
    /// Extinguish() — пламя опадает, свет гаснет, дым ещё немного идёт.
    /// </summary>
    public class FireIgnition : MonoBehaviour
    {
        public List<Light> lights = new List<Light>();
        public List<ParticleSystem> flames = new List<ParticleSystem>();
        public List<ParticleSystem> smoke = new List<ParticleSystem>();
        [Tooltip("Меши, у которых свечение (_EmissionColor) должно разгораться вместе с огнём")]
        public List<Renderer> glowMeshes = new List<Renderer>();
        public FMODUnity.StudioEventEmitter sound;

        [Header("Разгорание")]
        public float igniteDuration = 1.2f;
        [Tooltip("Яркость света во время разгорания (0..1 времени -> множитель яркости)")]
        public AnimationCurve igniteLight = new AnimationCurve(
            new Keyframe(0f, 0f), new Keyframe(0.06f, 0.55f), new Keyframe(0.12f, 0.12f), new Keyframe(0.22f, 0.7f),
            new Keyframe(0.3f, 0.35f), new Keyframe(0.52f, 1.25f), new Keyframe(0.75f, 0.92f), new Keyframe(1f, 1f));
        [Tooltip("Размер новых языков пламени во время разгорания")]
        public AnimationCurve igniteFlameSize = new AnimationCurve(
            new Keyframe(0f, 0.25f), new Keyframe(0.15f, 0.5f), new Keyframe(0.5f, 1.2f), new Keyframe(1f, 1f));
        [Range(0f, 0.5f)] public float flicker = 0.1f;
        [Tooltip("Доля igniteDuration, после которой начинает идти дым")]
        [Range(0f, 1f)] public float smokeDelay = 0.4f;

        [Header("Затухание")]
        public float extinguishDuration = 0.6f;
        [Tooltip("Сколько ещё идёт дым после затухания, сек")]
        public float smokeAfter = 1.5f;

        public UnityEvent onIgnited;
        public UnityEvent onExtinguished;

        public bool IsLit { get; private set; } = true;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private bool _captured;
        private float[] _intensity, _size, _rate;
        private Color[] _emission;
        private MaterialPropertyBlock _mpb;
        private Coroutine _co;

        private void Awake() => Capture();

        private void Capture()
        {
            if (_captured) return;
            _captured = true;
            _mpb = new MaterialPropertyBlock();
            _intensity = new float[lights.Count];
            for (int i = 0; i < lights.Count; i++) _intensity[i] = lights[i] ? lights[i].intensity : 0f;
            _size = new float[flames.Count]; _rate = new float[flames.Count];
            for (int i = 0; i < flames.Count; i++)
            {
                if (!flames[i]) continue;
                _size[i] = flames[i].main.startSizeMultiplier;
                _rate[i] = flames[i].emission.rateOverTimeMultiplier;
            }
            _emission = new Color[glowMeshes.Count];
            for (int i = 0; i < glowMeshes.Count; i++)
            {
                var m = glowMeshes[i] ? glowMeshes[i].sharedMaterial : null;
                _emission[i] = m && m.HasProperty(EmissionColorId) ? m.GetColor(EmissionColorId) : Color.black;
            }
        }

        [ContextMenu("Ignite")] public void Ignite() { Capture(); Run(IgniteRoutine()); }
        [ContextMenu("Extinguish")] public void Extinguish() { Capture(); Run(ExtinguishRoutine()); }

        /// <summary>Мгновенно зажечь / погасить без анимации (например, при загрузке сохранения).</summary>
        public void SetLit(bool lit)
        {
            Capture();
            if (_co != null) { StopCoroutine(_co); _co = null; }
            if (lit) ApplyFull(); else ApplyOff(true);
        }

        private void Run(IEnumerator routine)
        {
            if (_co != null) StopCoroutine(_co);
            if (!isActiveAndEnabled) { routine.MoveNext(); return; }
            _co = StartCoroutine(routine);
        }

        private IEnumerator IgniteRoutine()
        {
            IsLit = true;
            foreach (var s in smoke) if (s) s.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            for (int i = 0; i < flames.Count; i++)
            {
                var f = flames[i]; if (!f) continue;
                f.Clear(true);
                var main = f.main; main.startSizeMultiplier = _size[i] * igniteFlameSize.Evaluate(0f);
                f.Play(true);
            }
            foreach (var l in lights) if (l) l.enabled = true;
            if (sound && !sound.IsPlaying()) sound.Play();

            bool smokeOn = false;
            float t = 0f, seed = Random.value * 100f;
            while (t < igniteDuration)
            {
                float u = t / igniteDuration;
                float noise = 1f + flicker * (Mathf.PerlinNoise(seed, Time.time * 12f) * 2f - 1f);
                SetLight(Mathf.Max(0f, igniteLight.Evaluate(u)) * noise);
                SetGlow(Mathf.Clamp01(igniteLight.Evaluate(u)));
                float sz = igniteFlameSize.Evaluate(u);
                for (int i = 0; i < flames.Count; i++)
                {
                    if (!flames[i]) continue;
                    var main = flames[i].main; main.startSizeMultiplier = _size[i] * sz;
                    var em = flames[i].emission; em.rateOverTimeMultiplier = _rate[i] * Mathf.Lerp(0.5f, 1f, u);
                }
                if (!smokeOn && u >= smokeDelay) { smokeOn = true; foreach (var s in smoke) if (s) s.Play(true); }
                t += Time.deltaTime;
                yield return null;
            }
            ApplyFull();
            _co = null;
            onIgnited?.Invoke();
        }

        private IEnumerator ExtinguishRoutine()
        {
            IsLit = false;
            foreach (var f in flames) if (f) f.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            float t = 0f;
            while (t < extinguishDuration)
            {
                float k = 1f - Mathf.SmoothStep(0f, 1f, t / extinguishDuration);
                SetLight(k * (1f + flicker * (Random.value * 2f - 1f)));
                SetGlow(k);
                t += Time.deltaTime;
                yield return null;
            }
            ApplyOff(false);
            if (sound) sound.Stop();
            onExtinguished?.Invoke();
            if (smokeAfter > 0f) yield return new WaitForSeconds(smokeAfter);
            foreach (var s in smoke) if (s) s.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            _co = null;
        }

        private void ApplyFull()
        {
            IsLit = true;
            for (int i = 0; i < lights.Count; i++) if (lights[i]) { lights[i].intensity = _intensity[i]; lights[i].enabled = true; }
            for (int i = 0; i < flames.Count; i++)
            {
                var f = flames[i]; if (!f) continue;
                var main = f.main; main.startSizeMultiplier = _size[i];
                var em = f.emission; em.rateOverTimeMultiplier = _rate[i];
                if (!f.isPlaying) f.Play(true);
            }
            foreach (var s in smoke) if (s && !s.isPlaying) s.Play(true);
            foreach (var r in glowMeshes) if (r) r.SetPropertyBlock(null);
            if (sound && !sound.IsPlaying()) sound.Play();
        }

        private void ApplyOff(bool instant)
        {
            IsLit = false;
            for (int i = 0; i < lights.Count; i++) if (lights[i]) { lights[i].intensity = _intensity[i]; lights[i].enabled = false; }
            foreach (var f in flames) if (f) f.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (instant) { foreach (var s in smoke) if (s) s.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); if (sound) sound.Stop(); }
            SetGlow(0f);
        }

        private void SetLight(float k)
        {
            for (int i = 0; i < lights.Count; i++) if (lights[i]) lights[i].intensity = _intensity[i] * k;
        }

        private void SetGlow(float k)
        {
            for (int i = 0; i < glowMeshes.Count; i++)
            {
                var r = glowMeshes[i]; if (!r) continue;
                r.GetPropertyBlock(_mpb);
                _mpb.SetColor(EmissionColorId, _emission[i] * k);
                r.SetPropertyBlock(_mpb);
            }
        }
    }
}
