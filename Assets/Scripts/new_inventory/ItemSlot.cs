using System.Collections;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Inventory
{
    public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Image icon;
        [SerializeField] private float hoverDelay = 0.5f;

        private InventoryUI inventoryUI;
        private ItemData item;
        private ItemTooltip tooltip;
        private Coroutine hoverRoutine;
        private bool isPartSlot;

        public void Set(ItemData data, InventoryUI ui, bool canApply, ItemTooltip tooltipInstance, bool isPart = false)
        {
            item = data;
            inventoryUI = ui;
            tooltip = tooltipInstance;
            isPartSlot = isPart;

            icon.sprite = data.icon;
            if (label != null) label.text = data.itemName;
            icon.color = canApply ? Color.white : new Color(1, 1, 1, 0.4f);

            if (tooltip != null) tooltip.Hide();
            gameObject.SetActive(true);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (item == null) return;
            hoverRoutine = StartCoroutine(HoverDelay());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StopHover();
        }

        private IEnumerator HoverDelay()
        {
            yield return new WaitForSeconds(hoverDelay);
            if (item != null && tooltip != null)
            {
                tooltip.Show(item.description, transform.position + new Vector3(150, 0));
            }
        }

        private void StopHover()
        {
            if (hoverRoutine != null)
            {
                StopCoroutine(hoverRoutine);
                hoverRoutine = null;
            }
            if (tooltip != null) tooltip.Hide();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (item != null && inventoryUI != null)
            {
                // Передаем флаг, чтобы UI понимал, кликнули мы по детали в мини-окне или по предмету в списке
                inventoryUI.OnItemClicked(item, isPartSlot);
            }
        }

        private void OnDisable()
        {
            StopHover();
        }
    }
}