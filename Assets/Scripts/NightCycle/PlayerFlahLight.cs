using System.Collections;
using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class PlayerFlashlight : MonoBehaviour
    {
        private FlashlightUI flashlightUI;

        [Inject]
        public void Construct(FlashlightUI _flashlightUI)
        {
            flashlightUI = _flashlightUI;
        }

        [Header("Reveal Shader Settings")]
        [SerializeField] private float revealDistance = 15f;
        [SerializeField] private float revealAngle = 25f;
        [SerializeField] private float revealSmoothSpeed = 4f;
        [SerializeField] private float coneBackOffset = 1.5f;

        [Header("Flashlight Base")]
        public Light flashlight;
        [SerializeField] private Animator anim;
        [SerializeField] private string trig_on = "enable";
        [SerializeField] private string trig_off = "disable";
        public bool light_active = false;

        [Header("Essence Settings")]
        [SerializeField] private float maxEssence = 99f;
        public float currentEssence = 5f;
        [SerializeField] private float essenceDrainPerSecond = 0.5f;
        [SerializeField] private float uiFadeDuration = 0.5f; // Настройка скорости анимации текста

        public bool blockEssense = false;
        public bool blockUI = false;

        // Архитектурно правильное решение: отделяем реальное состояние панели от того, что хочет игрок
        private bool shouldUIBeOpen = false;

        private float currentShaderDistance = 0f;
        private int lastDisplayedEssence = -1;

        // Ссылка на активную корутину, чтобы отменять её при поднятии 2-й эссенции подряд
        private Coroutine activeUIFade;

        public float RevealDistance => revealDistance;
        public float RevealAngle => revealAngle;

        private void Awake()
        {
            light_active = false;
            shouldUIBeOpen = light_active; // Синхронизируем стартовое состояние
            UpdateUI();
            currentShaderDistance = light_active ? revealDistance : 0f;
        }

        private void Update()
        {
            if (!flashlight.enabled) return;

            HandleInput();
            ProcessEssence();
            UpdateRevealShaderSmoothly();
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (light_active)
                    TurnOffCrown();
                else if (currentEssence > 0)
                    TurnOnCrown();
            }
        }

        private void ProcessEssence()
        {
            if (light_active && !blockEssense)
            {
                currentEssence -= essenceDrainPerSecond * Time.deltaTime;
                currentEssence = Mathf.Clamp(currentEssence, 0f, maxEssence);

                UpdateUI();

                if (currentEssence <= 0)
                {
                    TurnOffCrown();
                }
            }
        }

        private void UpdateUI()
        {
            //Если идет анимация, жестко блокируем любые другие обновления текста
            if (blockUI) return;

            int displayValue = Mathf.CeilToInt(currentEssence);

            if (displayValue != lastDisplayedEssence)
            {
                flashlightUI.SetText(displayValue.ToString());
                lastDisplayedEssence = displayValue;
            }
        }

        public void AddEssenceWUI(float amount)
        {
            // Если игрок быстро подобрал вторую эссенцию, пока текст еще мигает:
            if (activeUIFade != null)
            {
                StopCoroutine(activeUIFade);
                flashlightUI.ResetAlpha(); // Резко возвращаем прозрачность в норму, чтобы не застрять полупрозрачными
            }

            float oldEssence = currentEssence;
            currentEssence += amount;
            currentEssence = Mathf.Clamp(currentEssence, 0f, maxEssence);

            activeUIFade = StartCoroutine(UpdateUIwithFade(oldEssence, currentEssence));
        }

        private IEnumerator UpdateUIwithFade(float val1, float val2)
        {
            blockUI = true;
            blockEssense = true;

            if (!flashlightUI.isOpen)
            {
                flashlightUI.Open();
            }

            int displayValue1 = Mathf.CeilToInt(val1);
            int displayValue2 = Mathf.CeilToInt(val2);

            flashlightUI.SetText(displayValue1.ToString());

            // Используем yield return StartCoroutine для строгого ожидания
            yield return StartCoroutine(flashlightUI.FadeTo(0f, uiFadeDuration));

            flashlightUI.SetText(displayValue2.ToString());

            // Синхронизируем кеш, чтобы после фейда текст не "прыгнул" обратно
            lastDisplayedEssence = displayValue2;

            yield return StartCoroutine(flashlightUI.FadeTo(1f, uiFadeDuration));

            blockUI = false;
            blockEssense = false;
            activeUIFade = null; // Очищаем ссылку, анимация закончена

            // Применяем отложенное желание игрока
            if (!shouldUIBeOpen)
            {
                flashlightUI.Close();
            }
            else
            {
                UpdateUI(); // На всякий случай актуализируем значение, если оно успело измениться
            }
        }

        private void UpdateRevealShaderSmoothly()
        {
            float targetDistance = light_active ? revealDistance : 0f;
            currentShaderDistance = Mathf.Lerp(currentShaderDistance, targetDistance, Time.deltaTime * revealSmoothSpeed);

            Vector3 virtualOrigin = flashlight.transform.position - (flashlight.transform.forward * coneBackOffset);
            Shader.SetGlobalVector("_CrownPos", virtualOrigin);
            Shader.SetGlobalVector("_CrownDir", flashlight.transform.forward.normalized);
            Shader.SetGlobalFloat("_CrownDistance", currentShaderDistance);

            float angleCos = Mathf.Cos(revealAngle * Mathf.Deg2Rad);
            Shader.SetGlobalFloat("_CrownAngle", angleCos);
        }

        public void TurnOnCrown()
        {
            shouldUIBeOpen = true; // Запоминаем, что игрок ХОЧЕТ видеть интерфейс

            if (!blockUI)
            {
                flashlightUI.Open();
            }

            anim.ResetTrigger(trig_off);
            anim.SetTrigger(trig_on);
            light_active = true;
        }

        public void TurnOffCrown()
        {
            shouldUIBeOpen = false; // Запоминаем, что игрок ХОЧЕТ скрыть интерфейс

            if (!blockUI)
            {
                flashlightUI.Close();
            }

            anim.ResetTrigger(trig_on);
            anim.SetTrigger(trig_off);
            light_active = false;
        }

        public void AddEssence(float amount)
        {
            currentEssence += amount;
            currentEssence = Mathf.Clamp(currentEssence, 0f, maxEssence);
            UpdateUI();
        }

        public void TurnOn() => this.gameObject.SetActive(true);
        public void TurnOFF() => this.gameObject.SetActive(false);
        public bool IsActive() => this.gameObject.activeSelf;
        public void TurnOnLight() => flashlight.gameObject.SetActive(true);
        public void TurnOFFLight() => flashlight.gameObject.SetActive(false);
        public bool IsActiveLight() => flashlight.gameObject.activeSelf;

        public void StatueChase()
        {
            StatueController[] Statues = Object.FindObjectsByType<StatueController>(FindObjectsSortMode.None);
            foreach (StatueController statue in Statues)
            {
                statue.advance_pose();
            }
        }

        public Vector3 GetVirtualOrigin()
        {
            return flashlight.transform.position - (flashlight.transform.forward * coneBackOffset);
        }
    }
}