using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace NightCycle
{
    /// <summary>
    /// Проявляет выключенный объект со всеми дочерними мешами обратным растворением QMD/BalustradeDissolve.
    /// На время проявления подменяет материалы, по окончании возвращает исходные.
    /// </summary>
    public class DissolveAppear : MonoBehaviour
    {
        [Serializable]
        private struct MaterialPair
        {
            public Material Source;
            public Material Dissolve;
        }

        private static readonly int DissolveId = Shader.PropertyToID("_Dissolve");

        [Tooltip("Для каждого материала объекта — его вариант на шейдере QMD/BalustradeDissolve")]
        [SerializeField] private MaterialPair[] _materials;
        [Tooltip("Длительность проявления, сек")]
        [SerializeField] private float _duration = 4f;
        [SerializeField] private UnityEvent _onAppeared;

        /// <summary>Включает объект и запускает проявление.</summary>
        public void Appear()
        {
            gameObject.SetActive(true);
            StartCoroutine(AppearRoutine());
        }

        private IEnumerator AppearRoutine()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            var originals = new Material[renderers.Length][];
            for (int i = 0; i < renderers.Length; i++)
            {
                originals[i] = renderers[i].sharedMaterials;
                renderers[i].sharedMaterials = Array.ConvertAll(originals[i], GetDissolveMaterial);
            }

            var block = new MaterialPropertyBlock();
            for (float t = 0f; t < _duration; t += Time.deltaTime)
            {
                block.SetFloat(DissolveId, 1f - t / _duration);
                foreach (Renderer r in renderers) r.SetPropertyBlock(block);
                yield return null;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].SetPropertyBlock(null);
                renderers[i].sharedMaterials = originals[i];
            }
            _onAppeared?.Invoke();
        }

        private Material GetDissolveMaterial(Material source)
        {
            foreach (MaterialPair pair in _materials)
            {
                if (pair.Source == source) return pair.Dissolve;
            }
            return source;
        }
    }
}
