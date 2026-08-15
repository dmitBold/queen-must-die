using Core;
using UnityEngine;
using Zenject;
using Inventory; 

namespace DI
{
    public class ServiceInstaller : MonoInstaller
    {
        [SerializeField] private AudioService audioService;
        [SerializeField] private SaveManager saveManager;

        public override void InstallBindings()
        {
            Container.BindInstance(audioService).AsSingle();
            Container.Bind<InventoryManager>().AsSingle();
            Container.Bind<QuestManager>().AsSingle();
            Container
                .Bind<ScenesManager>()
                .AsSingle()
                .WithArguments(this as MonoBehaviour);
            //TEST
            Container.Bind<SaveManager>().FromInstance(saveManager).AsSingle();
            Container.Bind<SaveSystem>().AsSingle();
            Container.Bind<IPlayerProvider>().To<PlayerProvider>().AsSingle();
            //TEST
        }
    }
}