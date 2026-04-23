using UnityEngine;
using Zenject;
using Zenject.SpaceFighter;

namespace DI
{
    public class PlayerInstaller : MonoInstaller
    {
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private bool _enableLantern;


        public override void InstallBindings()
        {
            var playerPrefab = Container.Resolve<PlayerView>();
            var instance = Container.InstantiatePrefabForComponent<PlayerView>(playerPrefab, _spawnPoint);

            Container.BindInstance(instance.FirstPersonController).AsSingle();
            Container.BindInstance(instance.PlayerInteraction).AsSingle();
            Container.BindInstance(instance.PlayerStateController).AsSingle();
            instance.SetLanternActivity(_enableLantern);
        }
    }
}