using System.Collections;
using UnityEngine;

namespace NightCycle
{
    /// <summary>
    /// Визуал света короны: плавно проявляет и гасит прожектор, объёмный конус и частицы.
    /// Включается и выключается только через PlayerFlashlight.
    /// </summary>
    public class CrownLightEffect : MonoBehaviour
    {
        private static readonly int OpacityId = Shader.PropertyToID("_Opacity");

        /// <summary>Прожектор короны; его интенсивность из инспектора считается полной яркостью.</summary>
        [SerializeField] private Light spotLight;

        /// <summary>Объёмный конус света (шейдер VolumetricSpotlight с параметром _Opacity).</summary>
        [SerializeField] private Renderer coneRenderer;

        /// <summary>Корневая система частиц эффекта; дочерние системы запускаются и гасятся вместе с ней.</summary>
        [SerializeField] private ParticleSystem particles;

        /// <summary>Время появления и исчезновения эффекта в секундах.</summary>
        [SerializeField] private float fadeDuration = 1f;

        private float _fullIntensity;
        private float _fullOpacity;
        private float _weight;
        private MaterialPropertyBlock _block;
        private Coroutine _fade;

        private void Awake()
        {
            _block = new MaterialPropertyBlock();
            if (spotLight != null) _fullIntensity = spotLight.intensity;
            if (coneRenderer != null) _fullOpacity = coneRenderer.sharedMaterial.GetFloat(OpacityId);

            _weight = 0f;
            Apply();
            if (particles != null) particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        /// <summary>Плавно показывает или прячет эффект с текущего состояния.</summary>
        public void SetVisible(bool visible)
        {
            if (_fade != null) StopCoroutine(_fade);

            if (particles != null)
            {
                if (visible) particles.Play(true);
                // Частицы не гасим резко: уже вылетевшие доживают свой цикл и исчезают сами.
                else particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            _fade = StartCoroutine(Fade(visible ? 1f : 0f));
        }

        private IEnumerator Fade(float target)
        {
            float speed = fadeDuration > 0f ? 1f / fadeDuration : float.MaxValue;
            while (!Mathf.Approximately(_weight, target))
            {
                _weight = Mathf.MoveTowards(_weight, target, speed * Time.deltaTime);
                Apply();
                yield return null;
            }
            _fade = null;
        }

        private void Apply()
        {
            float eased = Mathf.SmoothStep(0f, 1f, _weight);
            bool visible = _weight > 0f;

            if (spotLight != null)
            {
                spotLight.intensity = _fullIntensity * eased;
                spotLight.enabled = visible;
            }

            if (coneRenderer != null)
            {
                coneRenderer.GetPropertyBlock(_block);
                _block.SetFloat(OpacityId, _fullOpacity * eased);
                coneRenderer.SetPropertyBlock(_block);
                coneRenderer.enabled = visible;
            }
        }
    }
}
