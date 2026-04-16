using Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StatUI : MonoBehaviour
    {
        [SerializeField] Image background;
        [SerializeField] Image fill;
        //[SerializeField] Image icon;

        [SerializeField] StatVisualData visualData;
        [SerializeField] WorldState worldState;

        [SerializeField] float smoothSpeed = 5f;

        float targetFill;

        void Start()
        {
            worldState.OnStatChanged += OnStatChanged;
            //test
            worldState.OnStatLocked += OnStatLocked;
            //test
            ApplyVisual();
            UpdateTarget();
            fill.fillAmount = targetFill;
        }

        void Update()
        {
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

            fill.color = visualData.fillColor;
            background.color = visualData.backgroundColor;

        }

        void UpdateTarget()
        {
            int value = worldState.GetStatValue(visualData.stat);
            targetFill = value / 100f;
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
            fill.color = visualData.lockColor;

            //
            // animator?.SetTrigger("Lock");
        }
        //test

    }
}
