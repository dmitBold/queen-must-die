using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [SerializeField] ItemData item;
    [SerializeField] bool destroyOnPickup = true;
    //[SerializeField] GameObject obj;
    //[SerializeField] InventoryUI inventoryUI;

    public void Pickup()
    {

        InventoryManager.Instance.AddItem(item);
        //inventoryUI.Open();
        if (destroyOnPickup)
        {
            gameObject.SetActive(false);
            //obj.SetActive(false);
            destroyOnPickup = false;
        }
    }
}
