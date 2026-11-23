using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace NightCycle
{
    /// <summary>
    /// Проявляет выключенный объект со всеми дочерними мешами обратным растворением QMD/BalustradeDissolve.
    /// На время проявления подменяет материалы, по окончании возвращает исходные.
    /// Disappear делает обратное: растворяет меши и выключает их рендеры, объект остаётся включённым.
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

        /// <summary>
        /// Растворяет дочерние меши, у которых есть материал из списка, и выключает их рендеры.
        /// Остальные рендеры (например, частицы) не трогает, чтобы их можно было гасить отдельно.
        /// </summary>
        public void Disappear()
        {
            StartCoroutine(DisappearRoutine());
        }

        private IEnumerator DisappearRoutine()
        {
            var renderers = new List<MeshRenderer>();
            var originals = new List<Material[]>();
            foreach (MeshRenderer r in GetComponentsInChildren<MeshRenderer>())
            {
                Material[] source = r.sharedMaterials;
                Material[] dissolve = Array.ConvertAll(source, GetDissolveMaterial);
                if (dissolve.SequenceEqual(source)) continue;
                renderers.Add(r);
                originals.Add(source);
                r.sharedMaterials = dissolve;
            }

            var block = new MaterialPropertyBlock();
            for (float t = 0f; t < _duration; t += Time.deltaTime)
            {
                block.SetFloat(DissolveId, t / _duration);
                foreach (MeshRenderer r in renderers) r.SetPropertyBlock(block);
                yield return null;
            }

            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].enabled = false;
                renderers[i].SetPropertyBlock(null);
                renderers[i].sharedMaterials = originals[i];
            }
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
