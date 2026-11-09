using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

namespace NightCycle
{
    /// <summary>
    /// Плавно гасит все системы частиц объекта и его звук, затем выключает объект.
    /// Работает с материалами, у которых есть свойство _Color.
    /// </summary>
    public class ParticleFadeOut : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [Tooltip("Длительность угасания, сек")]
        [SerializeField] private float _duration = 2f;
        [SerializeField] private StudioEventEmitter _sound;

        /// <summary>Запускает угасание.</summary>
        public void FadeOut()
        {
            StartCoroutine(FadeRoutine());
        }

        private IEnumerator FadeRoutine()
        {
            var renderers = new List<ParticleSystemRenderer>();
            var colors = new List<Color>();
            foreach (ParticleSystemRenderer r in GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                if (r.sharedMaterial == null || !r.sharedMaterial.HasProperty(ColorId)) continue;
                renderers.Add(r);
                colors.Add(r.sharedMaterial.GetColor(ColorId));
            }

            var block = new MaterialPropertyBlock();
            for (float t = 0f; t < _duration; t += Time.deltaTime)
            {
                float level = 1f - t / _duration;
                for (int i = 0; i < renderers.Count; i++)
                {
                    block.SetColor(ColorId, colors[i] * level);
                    renderers[i].SetPropertyBlock(block);
                }
                if (_sound != null && _sound.EventInstance.isValid()) _sound.EventInstance.setVolume(level);
                yield return null;
            }

            if (_sound != null) _sound.Stop();
            gameObject.SetActive(false);
        }
    }
}
