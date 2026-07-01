using Core;
using NightCycle;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

namespace DI
{
    /// <summary>
    /// Scene MonoInstaller для режима сборки.
    /// Регистрирует ScenesManager и AssemblyService.
    /// </summary>
    public class AssemblyInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            //test
            //Container.Bind<CinemachineBrain>().FromComponentInHierarchy().AsSingle();
            //test

            Container
                .Bind<AssemblyService>()
                .AsSingle();
        }
    }
}
