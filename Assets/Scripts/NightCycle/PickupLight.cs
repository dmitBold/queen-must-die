using UnityEngine;

namespace NightCycle
{
    public class PickupLight : MonoBehaviour
    {
        [SerializeField] bool destroyOnPickup = true;
        [SerializeField] PlayerFlashlight flashlight;
        public void Pickup()
        {
            flashlight.Enable();

            if (destroyOnPickup)
            {
                gameObject.SetActive(false);
                destroyOnPickup = false;
            }
        }
    }
}
