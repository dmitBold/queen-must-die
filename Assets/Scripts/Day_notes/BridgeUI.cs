using Core;
using Inventory;
using UnityEngine;
using Zenject;

public class BridgeUI : MonoBehaviour
{
    private InventoryUI inventoryUI;
    private DayNoteUI noteUI;

    [Inject]
    public void Constructor(InventoryUI _inventoryUI, DayNoteUI _noteUI)
    {
        inventoryUI = _inventoryUI;
        noteUI = _noteUI;
    }

    public void LockUI()
    {
        inventoryUI.CloseUIOnly();
        noteUI.CloseUIOnly();
        inventoryUI.canOpen = false;
        noteUI.canOpen = false;
    }

    public void UnlockUI()
    {
        inventoryUI.canOpen = true;
        noteUI.canOpen = true;
    }
}
