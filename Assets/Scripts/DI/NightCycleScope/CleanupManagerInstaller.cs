using UnityEngine;
using Zenject;
using NightCycle;

namespace DI
{
    public class CleanupManagerInstaller : MonoInstaller
    {
        [SerializeField] private CleanupManager cleanupManager;

        public override void InstallBindings()
        {
            Container.BindInstance(cleanupManager).AsSingle();
        }
    }
}