using System.Collections;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Inventory
{
    public class ItemSlot : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,    
        IPointerClickHandler
    {
        [SerializeField] TextMeshProUGUI label;
        //[SerializeField] GameObject tooltip;
        //[SerializeField] TextMeshProUGUI tooltipText;
        [SerializeField] float hoverDelay = 0.5f;
        InventoryUI inventoryUI;

        [SerializeField] Image icon;

        ItemData item;
        Coroutine hoverRoutine;
        bool isDragging;

        //static ItemTooltip tooltip;

        ItemTooltip tooltip;

        bool is_night = false;

        public void Set(ItemData data, InventoryUI ui, bool canApply, ItemTooltip tooltipInstance)
        {
            item = data;
            inventoryUI = ui;
            icon.sprite = data.icon;
            label.text = data.itemName;
            //tooltipText.text = data.description;
            //test
            tooltip = tooltipInstance;
            //test

            tooltip.Hide();
            gameObject.SetActive(true);

            //test
            icon.color = canApply ? Color.white : new Color(1, 1, 1, 0.4f);
            //is_night = !canApply;
            //icon.
            //test

        }

        /*public static void SetTooltip(ItemTooltip instance)
    {
        tooltip = instance;
    }*/

        public void Clear()
        {
            item = null;
            tooltip.Hide();
            gameObject.SetActive(false);
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("POINTERHOVER!");
            if (item == null || isDragging)
            {
                Debug.Log("NULLORNOTDRAG@@@!");
                return;
            }

            hoverRoutine = StartCoroutine(HoverDelay());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StopHover();
        }

        IEnumerator HoverDelay()
        {
            yield return new WaitForSeconds(hoverDelay);

            if (!isDragging && item != null /*&& inventoryUI.isOpen*/)
            {
                Debug.Log("T O O L T I P S H O W @@@!");
                //test
                //if (tooltip == null)
                //  yield break;
                //test
                tooltip.Show(
                    item.description,
                    transform.position + new Vector3(150, 0)
                );
            }
        }

        void StopHover()
        {
            if (hoverRoutine != null)
            {
                StopCoroutine(hoverRoutine);
                hoverRoutine = null;
            }
            if (tooltip != null)
            {
                tooltip.Hide();
            }
        }

        public void SetDragging(bool dragging)
        {
            isDragging = dragging;
            if (dragging)
                StopHover();
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            if (item == null || !inventoryUI.IsDefaultMode()) return;
            inventoryUI.BeginDrag(item, icon.sprite);
        }

        public void OnDrag(PointerEventData eventData)
        {
            inventoryUI.UpdateDrag(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            inventoryUI.EndDrag();
        }


        public ItemData GetItem() => item;

        //test
        /*public void OnClick()
    {
        inventoryUI.OnItemClicked(item);
    }*/

        void OnDisable()
        {
            //StopAllCoroutines();
            StopHover();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            inventoryUI.OnItemClicked(item);
        }
        //test

    }
}
