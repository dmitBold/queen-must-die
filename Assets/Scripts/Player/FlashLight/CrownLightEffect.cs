using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

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

        /// <summary>Время появления света и конуса в секундах. Совпадает с разгоранием частицы Light магического круга (около 0.25 с).</summary>
        [SerializeField] private float fadeInDuration = 0.25f;

        /// <summary>Время исчезновения эффекта в секундах.</summary>
        [FormerlySerializedAs("fadeDuration")]
        [SerializeField] private float fadeOutDuration = 1f;

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
                if (visible)
                {
                    particles.Play(true);
                    EmitFirstParticles();
                }
                // Частицы не гасим резко: уже вылетевшие доживают свой цикл и исчезают сами.
                else particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            _fade = StartCoroutine(Fade(visible ? 1f : 0f));
        }

        // Системы без стартового выброса (Sides) выпускают первую частицу только через 1/rate секунд,
        // и эффект догоняет свет и руны с заметным опозданием. Первую частицу выпускаем сразу.
        private void EmitFirstParticles()
        {
            foreach (var system in particles.GetComponentsInChildren<ParticleSystem>())
            {
                if (system.emission.burstCount == 0 && system.particleCount == 0) system.Emit(1);
            }
        }

        private IEnumerator Fade(float target)
        {
            float duration = target > _weight ? fadeInDuration : fadeOutDuration;
            float speed = duration > 0f ? 1f / duration : float.MaxValue;
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
            // Линейно, как разгорается частица Light магического круга: свет и частицы проявляются синхронно.
            bool visible = _weight > 0f;

            if (spotLight != null)
            {
                spotLight.intensity = _fullIntensity * _weight;
                spotLight.enabled = visible;
            }

            if (coneRenderer != null)
            {
                coneRenderer.GetPropertyBlock(_block);
                _block.SetFloat(OpacityId, _fullOpacity * _weight);
                coneRenderer.SetPropertyBlock(_block);
                coneRenderer.enabled = visible;
            }
        }
    }
}
