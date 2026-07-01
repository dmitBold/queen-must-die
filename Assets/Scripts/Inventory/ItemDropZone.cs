using Cards;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Inventory
{
    public class ItemDropZone : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IDropHandler
    {
        [SerializeField] InventoryUI inventoryUI;
        [SerializeField] CardManager cardManager;

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("DROP CALLED");
            //inventoryUI.UpdateDropIndicator(false);
            ItemData item = null;// inventoryUI.GetDraggedItem();
            if (item == null)
                return;

            bool applied = cardManager.ApplyItem(item);

            if (applied)
            {
               // inventoryUI.ConsumeDraggedItem();
            }
            else
            {
                //inventoryUI.CancelDrag();
            }
        }

        //test
        public void OnPointerEnter(PointerEventData eventData)
        {
           // inventoryUI.UpdateDropIndicator(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
           // inventoryUI.UpdateDropIndicator(false);
        }

    }
}
