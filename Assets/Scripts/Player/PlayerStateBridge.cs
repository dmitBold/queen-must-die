using DI;
using Inventory;
using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class PlayerStateBridge : MonoBehaviour
    {
        private PlayerStateController _playerStateController;
        private InventoryUI _inventoryUI;
        private HUDController _hudController;
        private PlayerInteraction _playerInteraction;
        private IPlayerProvider _playerProvider;

        [Inject]
        public void Construct(
            [Inject(Optional = true)] PlayerStateController playerStateController,
            [Inject(Optional = true)] InventoryUI inventoryUI,
            [Inject(Optional = true)] HUDController hudController,
            [Inject(Optional = true)] PlayerInteraction playerInteraction,
            [Inject] IPlayerProvider playerProvider)
        {
            _playerProvider = playerProvider;
            _playerStateController = playerStateController;
            _inventoryUI = inventoryUI;
            _hudController = hudController;
            _playerInteraction = playerInteraction;
        }

        public void FreezePlayer()
        {
            if (_playerStateController == null) return;
            _playerStateController.SetMode(PlayerMode.Focused);
        }

        public void UnfreezePlayer()
        {
            if (_playerStateController == null) return;
            _playerStateController.SetMode(PlayerMode.FreeMovement);
        }

        public void SetCustomMode(PlayerMode mode)
        {
            if (_playerStateController == null) return;
            _playerStateController.SetMode(mode);
        }

        public void SetFocusedMode()
        {
            SetCustomMode(PlayerMode.Focused);
        }

        public void SetFreeMode()
        {
            SetCustomMode(PlayerMode.FreeMovement);
        }

        public void DisableInventoryUI()
        {
            if (_inventoryUI != null) _inventoryUI.DisableRootCanvas();
        }

        public void EnableInventoryUI()
        {
            if (_inventoryUI != null) _inventoryUI.EnableRootCanvas();
        }

        public void DisableHUD()
        {
            if (_hudController != null) _hudController.SetCrosshairActivity(false);
        }

        public void DiasableHUDtext()
        {
            if (_hudController != null) _hudController.DisableInteractionText();
        }

        /*public void EnableHUDtext()
        {
            if (_hudController != null) _hudController.EnableInteractionText();
        }*/

        public void EnableHUD()
        {
            if (_hudController != null) _hudController.SetCrosshairActivity(true);
        }

        public void SetupToDeath()
        {
            FreezePlayer();
            DisableInventoryUI();
            DisableHUD();
            Debug.Log("[PlayerStateBridge] Игрок подготовлен к смерти");
        }

        public void SetupToCutscene()
        {
            FreezePlayer();
            //DisableInventoryUI();
            DisableHUD();
            DiasableHUDtext();
            DisableInteraction();
        }

        public void SetupFromCutscene()
        {
            UnfreezePlayer();
            //EnableInventoryUI();
            EnableHUD();
            EnableInteraction();
        }

        /*public void MovePlayer(Transform point)
        {
            Debug.Log("MOVE");
            var player = _playerProvider.CurrentPlayer;
            player.Position = new Vector3(point.position.x, point.position.y, point.position.z);
        }*/

        public void MovePlayer(Transform point)
        {
            Debug.Log("MOVE");
            var player = _playerProvider.CurrentPlayer;
            var rb = player.Rigidbody;
            CharacterController controller = null;

            if (rb != null)
            {
                controller = rb.gameObject.GetComponentInParent<CharacterController>();

                if (controller != null)
                {
                    Debug.Log("aaa");
                    controller.enabled = false;
                }

                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // ИЗМЕНЕНИЕ ЗДЕСЬ: Если нашли родительский контроллер, двигаем ЕГО трансформ
            if (controller != null)
            {
                controller.transform.position = point.position;

                // Дополнительно разворачиваем игрока в сторону точки, если нужно
                controller.transform.rotation = point.rotation;
            }
            else
            {
                // Если контроллера вдруг нет, двигаем как раньше через Rigidbody
                player.Position = point.position;
            }

            // Синхронизируем координаты в движке
            Physics.SyncTransforms();

            if (controller != null)
            {
                controller.enabled = true;
            }
        }

        public void MovePlayerDelay(Transform point, float Delay)
        {
            Debug.Log("MOVE");
            var player = _playerProvider.CurrentPlayer;
            var rb = player.Rigidbody;
            CharacterController controller = null;

            if (rb != null)
            {
                controller = rb.gameObject.GetComponentInParent<CharacterController>();

                if (controller != null)
                {
                    Debug.Log("aaa");
                    controller.enabled = false;
                }

                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // ИЗМЕНЕНИЕ ЗДЕСЬ: Если нашли родительский контроллер, двигаем ЕГО трансформ
            if (controller != null)
            {
                controller.transform.position = point.position;

                // Дополнительно разворачиваем игрока в сторону точки, если нужно
                controller.transform.rotation = point.rotation;
            }
            else
            {
                // Если контроллера вдруг нет, двигаем как раньше через Rigidbody
                player.Position = point.position;
            }

            // Синхронизируем координаты в движке
            Physics.SyncTransforms();

            if (controller != null)
            {
                controller.enabled = true;
            }
        }

        public void EnableInteraction()
        {
            _playerInteraction.can_interact = true;
        }

        public void DisableInteraction()
        {
            _playerInteraction.can_interact = false;
        }

    }
}