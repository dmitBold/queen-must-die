using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Прокрутка барабана, сборка разрядов и отрисовка. Часть partial-класса <see cref="DrumCounter"/>.
    /// </summary>
    public partial class DrumCounter
    {
        // Строк на ленте на одну больше, чем цифр: последняя дублирует ноль,
        // поэтому переход 9 -> 0 идёт непрерывно и без разрыва на кадр.
        private const int WheelRows = 11;

        // Строка, которая стоит в окне, когда колесо не смещено.
        private const float CenterRow = 5f;

        private const float SnapEpsilon = 0.005f;
        private const float VelocityEpsilon = 0.05f;

        // Одна и та же лента идёт и в замер шаблона, и в каждый разряд: иначе высота строки
        // и высота колеса разъедутся и цифры поплывут относительно окна.
        private const string WheelText = "0\n1\n2\n3\n4\n5\n6\n7\n8\n9\n0";

        private void Update()
        {
            if (!isBuilt || state == State.Idle)
            {
                return;
            }

            float dt = Time.deltaTime;

            if (state == State.Approaching)
            {
                float aim = target + Mathf.Sign(target - shown) * ResolveOvershoot();

                shown = Mathf.SmoothDamp(shown, aim, ref velocity, settings.smoothTime, settings.rollSpeed, dt);

                if (Mathf.Abs(shown - aim) < SnapEpsilon)
                {
                    state = State.Settling;
                    velocity = 0f;
                }
            }
            else
            {
                shown = Mathf.SmoothDamp(shown, target, ref velocity, settings.smoothTime, settings.rollSpeed, dt);

                if (Mathf.Abs(shown - target) < SnapEpsilon && Mathf.Abs(velocity) < VelocityEpsilon)
                {
                    shown = target;
                    velocity = 0f;
                    state = State.Idle;
                }
            }

            Render();
        }

        /// <summary>
        /// Величина выброса в единицах значения. На границах диапазона выброс срезается:
        /// иначе счётчик уезжал бы в разряд, которого у него нет, и на кадр показывал лишнюю цифру.
        /// </summary>
        private float ResolveOvershoot()
        {
            if (settings.overshoot <= 0f)
            {
                return 0f;
            }

            bool movingUp = target > shown;

            if (movingUp && target >= MaxValue)
            {
                return 0f;
            }

            if (!movingUp && target <= MinValue)
            {
                return 0f;
            }

            return settings.overshoot;
        }

        private void Build()
        {
            if (isBuilt || hasBuildError)
            {
                return;
            }

            if (settings == null || template == null)
            {
                hasBuildError = true;
                Debug.LogError($"{nameof(DrumCounter)}: не назначены настройки или шаблон оформления", this);
                return;
            }

            if (template.transform == transform)
            {
                // Клонировать самого себя нельзя: получилась бы рекурсия из счётчиков.
                hasBuildError = true;
                Debug.LogError($"{nameof(DrumCounter)}: шаблоном назначен объект самого счётчика", this);
                return;
            }

            MeasureTemplate();

            // Окно расширяется на одну позицию под минус: знак стоит внутри маски,
            // иначе RectMask2D срезал бы его как выход за границы.
            int windowSlots = maxDigits + (allowNegative ? 1 : 0);
            CreateViewport(windowSlots);

            wheels = new TextMeshProUGUI[maxDigits];
            wheelRects = new RectTransform[maxDigits];

            float firstSlot = allowNegative ? 1f : 0f;

            for (int i = 0; i < maxDigits; i++)
            {
                CreateWheel(i, firstSlot + i);
            }

            if (allowNegative)
            {
                CreateSignWheel();
            }

            // Шаблон нужен был только как образец оформления: если его не выключить,
            // он останется на экране и будет показывать собственный «0».
            template.gameObject.SetActive(false);

            isBuilt = true;
        }

        /// <summary>
        /// Создаёт дочернее окно с маской. Маска живёт на отдельном объекте, а не на самом счётчике:
        /// так размер окна задаётся компонентом и не зависит от якорей и размера корневого RectTransform.
        /// </summary>
        private void CreateViewport(int slots)
        {
            GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewportObject.transform.SetParent(transform, false);

            viewport = (RectTransform)viewportObject.transform;
            viewport.anchorMin = new Vector2(0.5f, 0.5f);
            viewport.anchorMax = new Vector2(0.5f, 0.5f);
            viewport.pivot = new Vector2(0.5f, 0.5f);
            viewport.anchoredPosition = Vector2.zero;
            viewport.sizeDelta = new Vector2(digitWidth * slots, rowStep);
        }

        private void MeasureTemplate()
        {
            // Авторазмер подменяет кегль на максимум при измерении, и шаг строки разошёлся бы с тем,
            // что реально рисуется. На время замера его выключаем и возвращаем как было.
            bool wasAutoSizing = template.enableAutoSizing;
            template.enableAutoSizing = false;

            // Шаг строки берётся из той же ленты, что будет у каждого колеса: высота колеса и высота текста
            // получаются из одного замера, поэтому строки не съезжают относительно окна.
            Vector2 tape = template.GetPreferredValues(WheelText);
            Vector2 oneDigit = template.GetPreferredValues("0");

            template.enableAutoSizing = wasAutoSizing;

            rowStep = tape.y / WheelRows;
            digitWidth = oneDigit.x;

            if (rowStep <= 0f || digitWidth <= 0f)
            {
                // Подстраховка для шрифтов, у которых предпочтительные размеры не считаются.
                rowStep = template.fontSize * 1.2f;
                digitWidth = template.fontSize * 0.6f;
            }

            baseAlpha = template.alpha;
        }

        private void CreateWheel(int index, float slot)
        {
            TextMeshProUGUI wheel = Instantiate(template, viewport);
            wheel.name = $"Wheel{maxDigits - 1 - index}";
            wheel.gameObject.SetActive(true);
            wheel.text = WheelText;
            wheel.alignment = TextAlignmentOptions.Center;
            wheel.textWrappingMode = TextWrappingModes.NoWrap;
            wheel.overflowMode = TextOverflowModes.Overflow;
            wheel.enableAutoSizing = false;

            RectTransform rect = wheel.rectTransform;
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(digitWidth, rowStep * WheelRows);
            rect.anchoredPosition = new Vector2(digitWidth * (slot + 0.5f), 0f);

            wheels[index] = wheel;
            wheelRects[index] = rect;
        }

        private void CreateSignWheel()
        {
            signWheel = Instantiate(template, viewport);
            signWheel.name = "Sign";
            signWheel.gameObject.SetActive(true);
            signWheel.text = "−";
            signWheel.alignment = TextAlignmentOptions.Center;
            signWheel.textWrappingMode = TextWrappingModes.NoWrap;
            signWheel.overflowMode = TextOverflowModes.Overflow;

            RectTransform rect = signWheel.rectTransform;
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(digitWidth, rowStep);
            rect.anchoredPosition = new Vector2(digitWidth * 0.5f, 0f);
        }

        private void Render()
        {
            if (!isBuilt)
            {
                return;
            }

            float magnitude = Mathf.Abs(shown);

            // Длина берётся из реально собранных массивов, а не из maxDigits: поле можно поменять в инспекторе
            // уже после сборки, и рассинхрон дал бы IndexOutOfRange.
            for (int i = 0; i < wheels.Length; i++)
            {
                // Разряд i отвечает за степень десятки power: 0 — единицы, 1 — десятки и так далее.
                int power = wheels.Length - 1 - i;
                float slot = DigitSlot(magnitude, power);

                float row = Mathf.Repeat(slot, DigitCount);
                Vector2 anchored = wheelRects[i].anchoredPosition;
                wheelRects[i].anchoredPosition = new Vector2(anchored.x, rowStep * (row - CenterRow));

                // Проявление разряда берётся из его же позиции на колесе, а не из clamp01(shown / 10^k):
                // простой дележ давал полупрозрачный ноль слева уже на значении 5 и читался как «05».
                // Здесь разряд проявляется ровно тогда, когда младшие разряды доезжают до девятки.
                // Единицы видны всегда, иначе на нуле счётчик был бы пустым.
                float presence = power == 0 ? 1f : Mathf.Clamp01(slot);
                wheels[i].alpha = baseAlpha * presence;
            }

            if (signWheel != null)
            {
                bool negative = shown < -0.5f;
                signWheel.alpha = negative ? baseAlpha : 0f;
            }
        }

        /// <summary>
        /// Позиция колеса разряда в цифрах: целая часть это номер цифры, дробная — положение внутри колеса.
        /// Разряд доворачивается только после того, как младшие разряды прошли свою девятку,
        /// поэтому 19 стоит как «1» и «9», а не как «1.9» из простого деления на степень десятки.
        /// </summary>
        private static float DigitSlot(float magnitude, int power)
        {
            float scale = Mathf.Pow(DigitCount, power);
            float whole = Mathf.Floor(magnitude / scale);

            // Остаток по модулю scale показывает, насколько младшие разряды заполнили свою ёмкость:
            // когда он упирается в scale - 1, старший разряд начинает подтягиваться к следующей цифре.
            float carry = Mathf.Clamp01(Mathf.Repeat(magnitude, scale) - (scale - 1f));

            return whole + carry;
        }
    }
}

