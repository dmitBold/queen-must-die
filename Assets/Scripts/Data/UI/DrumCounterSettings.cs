using UnityEngine;

namespace UI
{
    /// <summary>
    /// Настройки для DrumCounter: скорость, плавность и размер выброса.
    /// </summary>
    [CreateAssetMenu(fileName = "DrumCounterSettings", menuName = "UI/DrumCounterSettings")]
    public class DrumCounterSettings : ScriptableObject
    {
        /// <summary>
        /// Потолок скорости прокрутки в единицах значения в секунду: 45 означает, что 0 → 99 займёт больше двух секунд.
        /// </summary>
        [Tooltip("Потолок скорости прокрутки в единицах значения в секунду (45: путь 0 → 99 занимает около 2.2 с)")]
        public float rollSpeed = 45f;

        /// <summary>
        /// Время сглаживания для SmoothDamp: меньше значит резче и быстрее выход на цель.
        /// </summary>
        [Tooltip("Плавность (время сглаживания). Меньше — резче, больше — вальяжнее")]
        public float smoothTime = 0.18f;

        /// <summary>
        /// Насколько барабан выезжает за целевое значение перед возвратом, в долях одной цифры. 0 отключает выброс.
        /// </summary>
        [Tooltip("Выброс за цель в долях цифры, например 0.3 — это треть цифры. 0 отключает выброс")]
        public float overshoot = 0.3f;
    }
}
