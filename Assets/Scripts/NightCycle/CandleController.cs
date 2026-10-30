using System.Collections;
using UnityEngine;

namespace NightCycle
{
    /// <summary>
    /// Свеча: точечный свет, огонь (частицы) и свечение меша (emission).
    /// Все три можно включать мгновенно (SetCandleState) или плавно (FadeTo / LightUp / BlowOut).
    /// </summary>
    public class CandleController : MonoBehaviour
    {
        public Light candleLight;
        public GameObject fireEffect;
        public MeshRenderer mesh;
        [Tooltip("Длительность плавного зажигания/гашения по умолчанию, сек")]
        public float fadeDuration = 1f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private Material candleMaterial;
        private bool _hasEmission;
        private Color _emission;
        private float _lightIntensity = 1f;
        private ParticleSystemRenderer[] _fxRenderers = new ParticleSystemRenderer[0];
        private Color[] _fxColors = new Color[0];
        private MaterialPropertyBlock _mpb;
        private float _level = 1f;
        private Coroutine _fade;
        private bool _initialized;

        /// <summary>Текущая яркость свечи: 0 — погашена, 1 — горит.</summary>
        public float Level => _level;

        void Awake() => Init();

        private void Init()
        {
            if (_initialized) return;
            _initialized = true;
            _mpb = new MaterialPropertyBlock();
            if (mesh != null)
            {
                candleMaterial = mesh.material;
                _hasEmission = candleMaterial.HasProperty(EmissionColorId);
                if (_hasEmission) _emission = candleMaterial.GetColor(EmissionColorId);
            }
            if (candleLight != null) _lightIntensity = candleLight.intensity;
            if (fireEffect != null)
            {
                _fxRenderers = fireEffect.GetComponentsInChildren<ParticleSystemRenderer>(true);
                _fxColors = new Color[_fxRenderers.Length];
                for (int i = 0; i < _fxRenderers.Length; i++)
                {
                    var m = _fxRenderers[i].sharedMaterial;
                    _fxColors[i] = m == null ? Color.white
                        : m.HasProperty(BaseColorId) ? m.GetColor(BaseColorId)
                        : m.HasProperty(ColorId) ? m.GetColor(ColorId) : Color.white;
                }
            }
            bool lit = (candleLight == null || candleLight.enabled) && (fireEffect == null || fireEffect.activeSelf);
            _level = lit ? 1f : 0f;
        }

        public void SetCandleState(bool isLit)
        {
            StopFade();
            SetLevel(isLit ? 1f : 0f);
        }

        /// <summary>Плавно меняет яркость свечи до level (0..1) за duration секунд (по умолчанию fadeDuration).</summary>
        public void FadeTo(float level, float duration = -1f)
        {
            Init();
            StopFade();
            if (duration < 0f) duration = fadeDuration;
            if (duration <= 0f || !isActiveAndEnabled)
            {
                SetLevel(level);
                return;
            }
            _fade = StartCoroutine(FadeRoutine(Mathf.Clamp01(level), duration));
        }

        public void LightUp(float duration = -1f) => FadeTo(1f, duration);
        public void BlowOut(float duration = -1f) => FadeTo(0f, duration);

        private IEnumerator FadeRoutine(float target, float duration)
        {
            // огонь появляется сразу (с нулевой яркостью), чтобы частицы успели набраться к концу разгорания
            if (target > 0f) SetLevel(Mathf.Max(_level, 0.002f));
            float from = _level, t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                SetLevel(Mathf.Lerp(from, target, Mathf.SmoothStep(0f, 1f, t / duration)));
                yield return null;
            }
            SetLevel(target);
            _fade = null;
        }

        private void StopFade()
        {
            if (_fade != null)
            {
                StopCoroutine(_fade);
                _fade = null;
            }
        }

        /// <summary>Мгновенно ставит яркость света, огня и свечения меша: 0 — погашена, 1 — горит.</summary>
        public void SetLevel(float level)
        {
            Init();
            _level = Mathf.Clamp01(level);
            bool on = _level > 0.001f;

            if (candleLight != null)
            {
                candleLight.intensity = _lightIntensity * _level;
                candleLight.enabled = on;
            }

            if (fireEffect != null && fireEffect.activeSelf != on) fireEffect.SetActive(on);
            for (int i = 0; i < _fxRenderers.Length; i++)
            {
                if (_fxRenderers[i] == null) continue;
                var c = _fxColors[i] * _level;
                _fxRenderers[i].GetPropertyBlock(_mpb);
                _mpb.SetColor(BaseColorId, c);
                _mpb.SetColor(ColorId, c);
                _fxRenderers[i].SetPropertyBlock(_mpb);
            }

            if (candleMaterial != null)
            {
                if (on) candleMaterial.EnableKeyword("_EMISSION");
                else candleMaterial.DisableKeyword("_EMISSION");
                if (_hasEmission) candleMaterial.SetColor(EmissionColorId, _emission * _level);
            }
        }

        [ContextMenu("Turn Off")] void TestOff() => SetCandleState(false);
        [ContextMenu("Turn On")] void TestOn() => SetCandleState(true);
        [ContextMenu("Blow Out (fade)")] void TestBlowOut() => BlowOut();
        [ContextMenu("Light Up (fade)")] void TestLightUp() => LightUp();
    }
}
