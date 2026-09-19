using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class PickupLight : MonoBehaviour
    {
        [SerializeField] private bool destroyOnPickup = true;

        [Inject] private PlayerFlashlight flashlight;

        public void Pickup()
        {
            // Используем новый удобный метод включения
            flashlight.TurnOn();
            flashlight.TurnOnCrown();

            if (destroyOnPickup)
            {
                gameObject.SetActive(false);
                destroyOnPickup = false;
            }
        }

        public void ChangeEssense(int value)
        {
            // Передаем значение в централизованный метод
            //flashlight.AddEssence(value);
            flashlight.AddEssenceWUI(value);
        }

    }
}