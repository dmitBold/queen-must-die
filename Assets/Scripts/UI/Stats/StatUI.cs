using Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class StatUI : MonoBehaviour
    {
        [SerializeField] Image background;
        [SerializeField] Image fill;
        // Ќеоб€зательный барабан: если назначен, значение показывает он, а fill гаситс€. Ќе назначен Ч работает fill.
        [SerializeField] private DrumCounter counter;
        //[SerializeField] Image icon;

        [SerializeField] StatVisualData visualData;

        [SerializeField] private WorldState worldStateManual;   // ручное назначение
        [InjectOptional] private WorldState worldStateInjected; // инжект из контекста

        private WorldState worldState =>
            worldStateManual != null ? worldStateManual : worldStateInjected;

        [SerializeField] float smoothSpeed = 5f;

        float targetFill;

        void Start()
        {
            worldState.OnStatChanged += OnStatChanged;
            //test
            worldState.OnStatLocked += OnStatLocked;
            //test
            ApplyVisual();
            // ѕервое значение барабан берЄт без прокрутки, дальше он крутитс€.
            UpdateTarget(false);
            if (counter == null && fill != null)
                fill.fillAmount = targetFill;
        }

        void Update()
        {
            if (counter != null || fill == null)
                return;

            fill.fillAmount = Mathf.Lerp(
                fill.fillAmount,
                targetFill,
                Time.deltaTime * smoothSpeed
            );
        }

        void ApplyVisual()
        {
            //if (icon != null && visualData.icon != null)
            //icon.sprite = visualData.icon;

            if (fill != null)
            {
                fill.color = visualData.fillColor;

                // Ѕарабан показывает значение сам, fill р€дом дал бы двойной показ.
                if (counter != null)
                    fill.enabled = false;
            }

            background.color = visualData.backgroundColor;

        }

        void UpdateTarget(bool animate = true)
        {
            int value = worldState.GetStatValue(visualData.stat);
            targetFill = value / 100f;

            if (counter != null)
                counter.SetValue(value, animate);
        }

        public void Refresh()
        {
            UpdateTarget();
        }

        void OnStatChanged(WorldState.Stats stat)
        {
            if (stat == visualData.stat)
                UpdateTarget();
        }

        //test
        void OnStatLocked(WorldState.Stats stat)
        {
            if (stat != visualData.stat)
                return;

            ApplyLockedVisual();
        }

        void ApplyLockedVisual()
        {
            background.sprite = visualData.lockedIcon;
            if (fill != null)
                fill.color = visualData.lockColor;

            // «алоченный стат барабан показывает как 0.
            if (counter != null)
                counter.SetValue(0);

            //
            // animator?.SetTrigger("Lock");
        }
        //test

    }
}
