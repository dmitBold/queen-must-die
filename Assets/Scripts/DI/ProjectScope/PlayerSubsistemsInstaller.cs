using UnityEngine;
using Zenject;
using Inventory;
using NightCycle;
using Unity.Cinemachine; // Нужно для CinemachineBrain

namespace NightCycle
{
    public class PlayerSubsistemsInstaller : MonoInstaller
    {
        [SerializeField] PlayerInputManager playerInputManager;
        [SerializeField] InventoryUI inventoryUI;
        [SerializeField] HUDController hUDController;

        public override void InstallBindings()
        {
            // 1. Биндим камеру из текущей сцены
            Container.Bind<CinemachineBrain>()
         .FromMethod(ctx => {
             // Ищем на MainCamera (тег MainCamera обязателен)
             var brain = Camera.main?.GetComponent<CinemachineBrain>();
             // Если не нашли — ищем по всем объектам во всех сценах
             if (brain == null) brain = GameObject.FindObjectOfType<CinemachineBrain>();
             // Если всё равно нет — явная ошибка
             if (brain == null) throw new System.InvalidOperationException(
                 "CinemachineBrain не найден. Убедитесь, что в сцене есть камера с этим компонентом и тегом MainCamera.");
             return brain;
         })
         .AsSingle();

            // 2. Биндим сервис сборки. Теперь он получит ScenesManager из ProjectContext и камеру из SceneContext
            Container.Bind<AssemblyService>().AsSingle();

            // 3. Твои старые бинды
            Container.Bind<PlayerInputManager>().FromInstance(playerInputManager).AsSingle().NonLazy();
            Container.Bind<InventoryUI>().FromComponentInNewPrefab(inventoryUI).AsSingle().NonLazy();
            Container.Bind<HUDController>().FromComponentInNewPrefab(hUDController).AsSingle().NonLazy();
        }
    }
}