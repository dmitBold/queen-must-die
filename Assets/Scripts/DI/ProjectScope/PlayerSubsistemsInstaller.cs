using Core;
using Inventory;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

namespace NightCycle
{
    public class PlayerSubsistemsInstaller : MonoInstaller
    {
        [SerializeField] PlayerInputManager playerInputManager;
        [SerializeField] InventoryUI inventoryUI;
        [SerializeField] HUDController hUDController;
        [SerializeField] QuestUI questUI;
        [SerializeField] HintManager HintUI;
        [SerializeField] SettingsMenu settingsMenu;
        [SerializeField] WorldState state;

        public override void InstallBindings()
        {
            Container.Bind<PlayerInputManager>().FromInstance(playerInputManager).AsSingle().NonLazy();
            Container.Bind<InventoryUI>().FromComponentInNewPrefab(inventoryUI).AsSingle().NonLazy();
            Container.Bind<HUDController>().FromComponentInNewPrefab(hUDController).AsSingle().NonLazy();
            Container.Bind<QuestUI>().FromComponentInNewPrefab(questUI).AsSingle().NonLazy();
            Container.Bind<HintManager>().FromComponentInNewPrefab(HintUI).AsSingle().NonLazy();
            //Container.Bind<Volume>().FromComponentInHierarchy().AsSingle();
            Container.Bind<SettingsMenu>().FromComponentInNewPrefab(settingsMenu).AsSingle().NonLazy();
            Container.Bind<WorldState>().FromComponentInNewPrefab(state).AsSingle().NonLazy();
        }
    }
}