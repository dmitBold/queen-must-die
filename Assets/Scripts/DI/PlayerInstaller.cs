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
        [SerializeField] private bool _enableInventory;

        [Inject] private SaveSystem _saveSystem;
        //TEST
        [Inject] private IPlayerProvider _playerProvider;
        [Inject] private IFlashlightProvider _flashlightProvider;
        //TEST

        public override void InstallBindings()
        {
            var playerPrefab = Container.Resolve<PlayerView>();

            Vector3 targetPosition = _spawnPoint.position;
            Quaternion targetRotation = _spawnPoint.rotation;

            if (PendingSpawn.TryConsume(out var pos, out var rot))
            {
                targetPosition = pos;
                targetRotation = rot;
            }
            else if (_saveSystem.HasLoadedData)
            {
                var data = _saveSystem.CurrentData;
                targetPosition = new Vector3(data.posX, data.posY, data.posZ);
                targetRotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);
            }

            if (_saveSystem.HasLoadedData)
            {
                var data = _saveSystem.CurrentData;
                targetPosition = new Vector3(data.posX, data.posY, data.posZ);
                targetRotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);
            }

            var instance = Container.InstantiatePrefabForComponent<PlayerView>(
                playerPrefab,
                targetPosition,
                targetRotation,
                null);

            Container.BindInstance(instance.Flashlight).AsSingle();
            Container.BindInstance(instance.FirstPersonController).AsSingle();
            Container.BindInstance(instance.PlayerInteraction).AsSingle();
            Container.BindInstance(instance.PlayerStateController).AsSingle();
            instance.SetLanternActivity(_enableLantern);

            var rb = instance.GetComponentInChildren<Rigidbody>();
            var mr = instance.GetComponentInChildren<MeshRenderer>();
            _flashlightProvider.Current = instance.Flashlight;
            if (_flashlightProvider.PendingEssence.HasValue)
            {
                instance.Flashlight.SetEssense(_flashlightProvider.PendingEssence.Value);
                _flashlightProvider.PendingEssence = null;
            }
            Container.Bind<Player>().AsSingle().WithArguments(rb, mr);
        }

        public override void Start()
        {
            base.Start();
            //Container.Resolve<InventoryUI>().SetMode(InventoryUI.InventoryMode.Default);

            var inventoryUI = Container.Resolve<InventoryUI>();
            if (!_enableInventory)
            {
                inventoryUI.DisableRootCanvas();
            }
            else
            {
                inventoryUI.SetMode(InventoryUI.InventoryMode.Default);
            }

                HUDController.instance.SetCrosshairActivity(true);

            //TEST
            var player = Container.Resolve<Player>();
            _playerProvider.CurrentPlayer = player;
            //TEST

            if (_saveSystem.HasLoadedData)
            {
                _saveSystem.ClearLoadedFlag();
            }
        }

        private void OnDestroy()
        {
            HUDController.instance.SetCrosshairActivity(false);
            Container.Resolve<InventoryUI>().SetMode(InventoryUI.InventoryMode.Disable);

            //TEST
            if (_playerProvider != null)
            {
                _playerProvider.CurrentPlayer = null;
            }

            _flashlightProvider.Current = null;
            //TEST
        }
    }
}