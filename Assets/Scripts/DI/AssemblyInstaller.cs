using Core;
using NightCycle;
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
            Container
                .Bind<AssemblyService>()
                .AsSingle();
        }
    }
}
