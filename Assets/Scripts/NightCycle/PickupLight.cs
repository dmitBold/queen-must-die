using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class PickupLight : MonoBehaviour
    {
        [SerializeField] bool destroyOnPickup = true;
        [Inject] PlayerFlashlight flashlight;
        public void Pickup()
        {
            flashlight.TurnOn();
            flashlight.light_active = true;

            if (destroyOnPickup)
            {
                gameObject.SetActive(false);
                destroyOnPickup = false;
            }
        }
    }
}
