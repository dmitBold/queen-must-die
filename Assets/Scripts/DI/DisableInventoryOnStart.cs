using UnityEngine;
using Zenject;
using Inventory;

public class DisableInventoryOnStart : MonoBehaviour
{
    [Inject] private InventoryUI _inventoryUI;

    private void Start()
    {
        if (_inventoryUI != null)
        {
            // Отключаем полностью весь UI инвентаря
            _inventoryUI.DisableRootCanvas();
            // Или используйте ваш метод:
            //_inventoryUI.SetMode(InventoryUI.InventoryMode.Disable);
        }
    }
}
