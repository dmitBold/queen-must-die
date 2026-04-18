using UnityEngine;
using Zenject;
using Inventory;

namespace NightCycle
{
    public class PlayerSubsistemsInstaller : MonoInstaller
    {
        [SerializeField] PlayerInputManager playerInputManager;
        [SerializeField] InventoryUI inventoryUI;
        [SerializeField] HUDController hUDController;
        public override void InstallBindings()
        {
            Container.Bind<PlayerInputManager>().FromInstance(playerInputManager).AsSingle();
            Container.Bind<InventoryUI>().FromComponentInNewPrefab(inventoryUI).AsSingle();
            Container.Bind<HUDController>().FromComponentInNewPrefab(hUDController).AsSingle();
        }
    }
}