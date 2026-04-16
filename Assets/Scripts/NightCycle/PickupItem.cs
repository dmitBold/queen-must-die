using Inventory;
using UnityEngine;
using Zenject;

namespace NightCycle
{
    //TODO развернуть зависимость
    public class PickupItem : MonoBehaviour
    {
        [SerializeField] ItemData item;
        [SerializeField] bool destroyOnPickup = true;
        //[SerializeField] GameObject obj;
        //[SerializeField] InventoryUI inventoryUI;

        private InventoryManager _inventoryManager;

        [Inject]
        public void Constructor(InventoryManager inventoryManager)
        {
            _inventoryManager = inventoryManager;
        }

        public void Pickup()
        {

            _inventoryManager.AddItem(item);
            //inventoryUI.Open();
            if (destroyOnPickup)
            {
                gameObject.SetActive(false);
                //obj.SetActive(false);
                destroyOnPickup = false;
            }
        }
    }
}
