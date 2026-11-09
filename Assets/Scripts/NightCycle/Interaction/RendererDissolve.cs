using System.Collections;
using UnityEngine;

namespace NightCycle
{
    /// <summary>
    /// Растворяет меш с материалом на шейдере QMD/BalustradeDissolve.
    /// Объект и его коллайдер не выключает: это делает тот, кто вызвал растворение, по его окончании.
    /// </summary>
    public class RendererDissolve : MonoBehaviour
    {
        private static readonly int DissolveId = Shader.PropertyToID("_Dissolve");

        [SerializeField] private Renderer _renderer;
        [Tooltip("Длительность растворения, сек. Для балюстрады равна Delay у Interactable руны")]
        [SerializeField] private float _duration = 4f;

        private MaterialPropertyBlock _block;

        /// <summary>Запускает растворение от целого объекта до полного исчезновения.</summary>
        public void Dissolve()
        {
            _block ??= new MaterialPropertyBlock();
            StartCoroutine(DissolveRoutine());
        }

        private IEnumerator DissolveRoutine()
        {
            for (float t = 0f; t < _duration; t += Time.deltaTime)
            {
                ApplyDissolve(t / _duration);
                yield return null;
            }
            ApplyDissolve(1f);
        }

        private void ApplyDissolve(float value)
        {
            _renderer.GetPropertyBlock(_block);
            _block.SetFloat(DissolveId, value);
            _renderer.SetPropertyBlock(_block);
        }
    }
}
