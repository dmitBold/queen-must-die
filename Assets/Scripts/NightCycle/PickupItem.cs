using Core;
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
        private AudioService _audioService;
        //TEST
        [SerializeField] private GlobalAudioConfig _configReference;
        private static GlobalAudioConfig sharedConfig;
        //TEST

        [Inject]
        public void Constructor(InventoryManager inventoryManager, AudioService audioService)
        {
            _inventoryManager = inventoryManager;
            _audioService = audioService;
        }
        //TEST
        private void Awake()
        {
            if (_configReference != null)
            {
                sharedConfig = _configReference;
            }
        }
        //TEST
        public void Pickup()
        {

            _inventoryManager.AddItem(item);
            //test
            _audioService.PlaySound(sharedConfig.pickupSound);
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
