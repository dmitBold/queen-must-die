using Core;
using Inventory;
using UnityEngine;
using Zenject;
using FMODUnity;

namespace NightCycle
{
    public class PickupItem : MonoBehaviour
    {
        [SerializeField] private ItemData item;
        [SerializeField] private bool destroyOnPickup = true;

        private InventoryManager _inventoryManager;
        private AudioService _audioService;
        private GlobalAudioConfig _audioConfig;

        [Inject]
        public void Construct(InventoryManager inventoryManager, AudioService audioService, GlobalAudioConfig audioConfig)
        {
            _inventoryManager = inventoryManager;
            _audioService = audioService;
            _audioConfig = audioConfig;
        }

        public void Pickup()
        {
            _inventoryManager.AddItem(item);
            //_audioService.PlaySound(_audioConfig.pickupSound);
            if (item != null && !item.pickupSound.IsNull)
            {
                FMODUnity.RuntimeManager.PlayOneShotAttached(item.pickupSound, gameObject);
            }


            if (destroyOnPickup)
            {
                gameObject.SetActive(false);
                destroyOnPickup = false;
            }
        }
    }
}
