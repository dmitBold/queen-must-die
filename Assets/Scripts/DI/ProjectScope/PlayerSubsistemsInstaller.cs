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
        [SerializeField] QuestUI questUI;

        public override void InstallBindings()
        {
            Container.Bind<PlayerInputManager>().FromInstance(playerInputManager).AsSingle().NonLazy();
            Container.Bind<InventoryUI>().FromComponentInNewPrefab(inventoryUI).AsSingle().NonLazy();
            Container.Bind<HUDController>().FromComponentInNewPrefab(hUDController).AsSingle().NonLazy();
            Container.Bind<QuestUI>().FromComponentInNewPrefab(questUI).AsSingle().NonLazy();
        }
    }
}