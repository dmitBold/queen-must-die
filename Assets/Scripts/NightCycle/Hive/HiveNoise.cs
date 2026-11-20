using UnityEngine;

namespace NightCycle
{
    /// <summary>
    /// Счётчик шума одного улья в «секундах шума»: копится от действий игрока и равномерно спадает.
    /// </summary>
    public class HiveNoise
    {
        private readonly float _decayPerSecond;

        /// <summary>Текущий шум, не меньше нуля.</summary>
        public float Value { get; private set; }

        /// <param name="secondsPerUnit">За сколько реальных секунд спадает одна секунда шума.</param>
        public HiveNoise(float secondsPerUnit)
        {
            _decayPerSecond = secondsPerUnit > 0f ? 1f / secondsPerUnit : 0f;
        }

        /// <summary>Добавляет шум.</summary>
        public void Add(float amount)
        {
            if (amount > 0f) Value += amount;
        }

        /// <summary>Спад шума за прошедшее время. Идёт всегда, даже пока игрок шумит.</summary>
        public void Decay(float deltaTime)
        {
            Value = Mathf.Max(0f, Value - _decayPerSecond * deltaTime);
        }
    }
}
