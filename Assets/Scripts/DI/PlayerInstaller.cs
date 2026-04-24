using System;
using Inventory;
using NightCycle;
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

            Container.BindInstance(instance.Flashlight).AsSingle();
            Container.BindInstance(instance.FirstPersonController).AsSingle();
            Container.BindInstance(instance.PlayerInteraction).AsSingle();
            Container.BindInstance(instance.PlayerStateController).AsSingle();
            instance.SetLanternActivity(_enableLantern);
        }

        public override void Start()
        {
            base.Start();
            Container.Resolve<InventoryUI>().SetMode(InventoryUI.InventoryMode.Default);
            HUDController.instance.SetCrosshairActivity(true);
        }

        private void OnDestroy()
        {
            HUDController.instance.SetCrosshairActivity(false);
            Container.Resolve<InventoryUI>().SetMode(InventoryUI.InventoryMode.Disable);
        }
    }
}