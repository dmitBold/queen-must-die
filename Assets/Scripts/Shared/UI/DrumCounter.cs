using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Счётчик с эффектом прокрутки барабана: показывает целое число, докручивая разряды до нового значения.
    /// Геймплейное значение меняется сразу, компонент отвечает только за то, что видит игрок.
    /// Оформление берётся с шаблонного текста: шрифт, размер, цвет и материал копируются в разряды.
    /// Расчёт колеса и отрисовка лежат в DrumCounter.Rolling.cs.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public partial class DrumCounter : MonoBehaviour
    {
        private const int DigitCount = 10;

        private enum State
        {
            Idle,
            Approaching,
            Settling
        }

        [Tooltip("Скорость, плавность и выброс. Один ассет на все счётчики, чтобы они крутились одинаково.")]
        [SerializeField] private DrumCounterSettings settings;

        [Tooltip("Шаблон оформления: с него копируются шрифт, размер, цвет и материал. После сборки объект выключается.")]
        [SerializeField] private TextMeshProUGUI template;

        [Tooltip("Сколько разрядов может показать счётчик. Ведущие нули становятся прозрачными, место под них сохраняется.")]
        [SerializeField] private int maxDigits = 2;

        [Tooltip("Показывать знак минус для отрицательных чисел.")]
        [SerializeField] private bool allowNegative = false;

        private RectTransform viewport;
        private TextMeshProUGUI[] wheels;
        private RectTransform[] wheelRects;
        private TextMeshProUGUI signWheel;

        private float rowStep;
        private float digitWidth;
        private float baseAlpha = 1f;

        private float shown;
        private float velocity;
        private int target;

        private State state = State.Idle;
        private bool isBuilt;
        private bool hasValue;
        private bool hasOverflowWarning;
        private bool hasBuildError;

        private float MaxValue => Mathf.Pow(DigitCount, maxDigits) - 1f;

        private float MinValue => allowNegative ? -MaxValue : 0f;

        private void OnValidate()
        {
            maxDigits = Mathf.Clamp(maxDigits, 1, 6);
        }

        private void OnEnable()
        {
            if (!isBuilt)
            {
                Build();
            }

            Render();
        }

        private void OnDisable()
        {
            // Пока счётчика не видно, барабан не догоняет значение: иначе при повторном показе он прыгнет из прошлого кадра.
            shown = target;
            velocity = 0f;
            state = State.Idle;

            Render();
        }

        /// <summary>
        /// Задаёт отображаемое значение. Анимация включается только при animate = true и не на первом присваивании:
        /// при первом показе счётчик сразу встаёт на нужное число, без прокрутки из нуля.
        /// Значение вне ёмкости счётчика обрезается по границам разрядов.
        /// </summary>
        public void SetValue(int value, bool animate = true)
        {
            value = ClampToCapacity(value);

            bool isFirstAssignment = !hasValue;

            hasValue = true;
            target = value;

            if (!isActiveAndEnabled)
            {
                shown = value;
                velocity = 0f;
                state = State.Idle;
                return;
            }

            if (!isBuilt)
            {
                Build();
            }

            if (!isBuilt)
            {
                return;
            }

            if (isFirstAssignment || !animate)
            {
                shown = value;
                velocity = 0f;
                state = State.Idle;
                Render();
                return;
            }

            if (state == State.Idle && Mathf.Approximately(shown, value))
            {
                return;
            }

            // Скорость не обнуляется: серия «прибавили, отняли, прибавили» продолжает движение с уже набранной скоростью,
            // поэтому барабан не дёргается при каждой смене цели.
            state = State.Approaching;
        }

        private int ClampToCapacity(int value)
        {
            int capacity = (int)MaxValue;
            int floor = (int)MinValue;

            if (value > capacity || value < floor)
            {
                WarnOverflow();
            }

            return Mathf.Clamp(value, floor, capacity);
        }

        private void WarnOverflow()
        {
            if (hasOverflowWarning)
            {
                return;
            }

            hasOverflowWarning = true;
            Debug.LogWarning($"{nameof(DrumCounter)}: значение не влезает в {maxDigits} разряда, показ обрезан. Увеличь maxDigits.", this);
        }
    }
}
