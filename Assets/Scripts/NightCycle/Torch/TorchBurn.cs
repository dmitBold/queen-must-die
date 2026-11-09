using UnityEngine;

namespace NightCycle
{
    /// <summary>
    /// Правила выгорания факела: сколько времени осталось и какой уровень огня этому соответствует.
    /// </summary>
    public sealed class TorchBurn
    {
        private readonly float _duration;
        private readonly float _flameStep;

        public TorchBurn(float duration, float flameStep)
        {
            _duration = duration;
            _flameStep = flameStep;
            Remaining = duration;
        }

        /// <summary>Сколько секунд горения осталось.</summary>
        public float Remaining { get; private set; }

        public bool IsBurnedOut => Remaining <= 0f;

        /// <summary>Доля яркости и громкости 0..1. Меняется раз в секунду.</summary>
        public float Level => Mathf.Ceil(Remaining) / _duration;

        /// <summary>Доля размера пламени 0..1. Меняется раз в шаг пламени.</summary>
        public float FlameLevel => Mathf.Ceil(Remaining / _flameStep) / Mathf.Ceil(_duration / _flameStep);

        /// <summary>Отнимает время горения: и обычный ход времени, и штраф за бросок.</summary>
        public void Burn(float seconds) => Remaining = Mathf.Max(0f, Remaining - seconds);
    }
}
