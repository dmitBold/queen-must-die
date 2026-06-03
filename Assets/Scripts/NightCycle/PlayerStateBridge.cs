using Inventory;
using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class PlayerStateBridge : MonoBehaviour
    {
        private PlayerStateController _playerStateController;
        private InventoryUI _inventoryUI;
        private HUDController _hudController;

        [Inject]
        public void Construct(
            [Inject(Optional = true)] PlayerStateController playerStateController,
            [Inject(Optional = true)] InventoryUI inventoryUI,
            [Inject(Optional = true)] HUDController hudController)
        {
            _playerStateController = playerStateController;
            _inventoryUI = inventoryUI;
            _hudController = hudController;
        }

        public void FreezePlayer()
        {
            if (_playerStateController == null) return;
            _playerStateController.SetMode(PlayerMode.Focused);
        }

        public void UnfreezePlayer()
        {
            if (_playerStateController == null) return;
            _playerStateController.SetMode(PlayerMode.FreeMovement);
        }

        public void SetCustomMode(PlayerMode mode)
        {
            if (_playerStateController == null) return;
            _playerStateController.SetMode(mode);
        }

        public void DisableInventoryUI()
        {
            if (_inventoryUI != null) _inventoryUI.DisableRootCanvas();
        }

        public void EnableInventoryUI()
        {
            if (_inventoryUI != null) _inventoryUI.EnableRootCanvas();
        }

        public void DisableHUD()
        {
            if (_hudController != null) _hudController.SetCrosshairActivity(false);
        }

        public void EnableHUD()
        {
            if (_hudController != null) _hudController.SetCrosshairActivity(true);
        }

        public void SetupToDeath()
        {
            FreezePlayer();
            DisableInventoryUI();
            DisableHUD();
            Debug.Log("[PlayerStateBridge] Игрок подготовлен к смерти");
        }

    }
}