using System.Collections;
using UnityEngine;

namespace NightCycle
{
    /// <summary>
    /// Объём, в котором огни с FireDarkness гаснут и разгораются по команде.
    /// Darken и Restore вызываются UnityEvent-ами тех же событий, что гасят свет помещения.
    /// Держать на слое Ignore Raycast, чтобы объём не перехватывал луч взаимодействия.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class DarknessZone : MonoBehaviour
    {
        private Coroutine _fade;

        /// <summary>Яркость огней в зоне: 1 — обычная, 0 — темно.</summary>
        public float Level { get; private set; } = 1f;

        /// <summary>Гасит огни в зоне за указанное время (0 — мгновенно).</summary>
        public void Darken(float duration) => FadeTo(0f, duration);

        /// <summary>Возвращает огням в зоне прежнюю яркость за указанное время.</summary>
        public void Restore(float duration) => FadeTo(1f, duration);

        private void FadeTo(float target, float duration)
        {
            if (_fade != null)
            {
                StopCoroutine(_fade);
                _fade = null;
            }

            if (duration <= 0f || !isActiveAndEnabled)
            {
                Level = target;
                return;
            }

            _fade = StartCoroutine(FadeRoutine(target, duration));
        }

        private IEnumerator FadeRoutine(float target, float duration)
        {
            float start = Level;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                Level = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, elapsed / duration));
                yield return null;
            }

            Level = target;
            _fade = null;
        }
    }
}
