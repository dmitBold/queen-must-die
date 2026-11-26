using UnityEngine;

namespace NightCycle
{
    /// <summary>
    /// Нагрузка хрупкой доски в «секундах бега»: копится от бега и приземлений, спадает, только когда Бет не на доске.
    /// </summary>
    public class PlankStress
    {
        private readonly float _breakThreshold;
        private readonly float _decayPerSecond;

        /// <summary>Текущая нагрузка, от нуля до порога слома.</summary>
        public float Value { get; private set; }

        /// <summary>Доля нагрузки от порога слома, 0–1.</summary>
        public float Ratio => _breakThreshold > 0f ? Value / _breakThreshold : 1f;

        /// <summary>Нагрузка дошла до порога, доска должна сломаться.</summary>
        public bool IsOverloaded => Value >= _breakThreshold;

        /// <param name="breakThreshold">Нагрузка, на которой доска ломается.</param>
        /// <param name="decayPerSecond">Сколько нагрузки спадает за секунду, пока Бет не на доске.</param>
        public PlankStress(float breakThreshold, float decayPerSecond)
        {
            _breakThreshold = breakThreshold;
            _decayPerSecond = Mathf.Max(0f, decayPerSecond);
        }

        /// <summary>Добавляет нагрузку, не выше порога слома.</summary>
        public void Add(float amount)
        {
            if (amount > 0f) Value = Mathf.Min(_breakThreshold, Value + amount);
        }

        /// <summary>Спад нагрузки за прошедшее время. Вызывать, только пока Бет не на доске.</summary>
        public void Decay(float deltaTime)
        {
            Value = Mathf.Max(0f, Value - _decayPerSecond * deltaTime);
        }
    }
}
