using Core;
using UnityEngine;
using Zenject;
using Inventory; 

namespace DI
{
    public class ServiceInstaller : MonoInstaller
    {
        [SerializeField] private AudioService audioService;

        public override void InstallBindings()
        {
            Container.BindInstance(audioService).AsSingle();
            Container.Bind<InventoryManager>().AsSingle();
            Container
                .Bind<ScenesManager>()
                .AsSingle()
                .WithArguments(this as MonoBehaviour);

        }
    }
}