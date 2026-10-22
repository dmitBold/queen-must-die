using Core;
using Inventory;
using Unity.Cinemachine;
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
        [SerializeField] FlashlightUI flashlightUI;
        [SerializeField] HintManager HintUI;
        [SerializeField] SettingsMenu settingsMenu;
        [SerializeField] WorldState state;
        [SerializeField] DayNoteUI notes;

        public override void InstallBindings()
        {
            Container.Bind<CinemachineBrain>()
         .FromMethod(ctx => {
             var brain = Camera.main?.GetComponent<CinemachineBrain>();
             if (brain == null) brain = GameObject.FindObjectOfType<CinemachineBrain>();
             /*if (brain == null) throw new System.InvalidOperationException(
                 "CinemachineBrain íå íàéäåí. Óáåäèòåñü, ÷òî â ñöåíå åñòü êàìåðà ñ ýòèì êîìïîíåíòîì è òåãîì MainCamera.");*/
             return brain;
         })
         .AsSingle();

            // 2. Áèíäèì ñåðâèñ ñáîðêè. Òåïåðü îí ïîëó÷èò ScenesManager èç ProjectContext è êàìåðó èç SceneContext
            Container.Bind<AssemblyService>().AsSingle();


            Container.Bind<PlayerInputManager>().FromInstance(playerInputManager).AsSingle().NonLazy();
            Container.Bind<InventoryUI>().FromComponentInNewPrefab(inventoryUI).AsSingle().NonLazy();
            Container.Bind<HUDController>().FromComponentInNewPrefab(hUDController).AsSingle().NonLazy();
            Container.Bind<QuestUI>().FromComponentInNewPrefab(questUI).AsSingle().NonLazy();
            Container.Bind<HintManager>().FromComponentInNewPrefab(HintUI).AsSingle().NonLazy();
            //Container.Bind<Volume>().FromComponentInHierarchy().AsSingle();
            Container.Bind<SettingsMenu>().FromComponentInNewPrefab(settingsMenu).AsSingle().NonLazy();
            Container.Bind<WorldState>().FromComponentInNewPrefab(state).AsSingle().NonLazy();
            Container.Bind<FlashlightUI>().FromComponentInNewPrefab(flashlightUI).AsSingle().NonLazy();
            Container.Bind<DayNoteUI>().FromComponentInNewPrefab(notes).AsSingle().NonLazy();
        }
    }
}