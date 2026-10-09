using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core;
using Zenject;

namespace NightCycle
{
    public class LevelLoader : MonoBehaviour
    {
        [SerializeField] private GameScene targetScene;
        
        private ScenesManager _scenesManager;

        [Header("Куда спавнить игрока в новой сцене")]
        [SerializeField] private Transform exitSpawnPoint;

        // либо явные координаты:
        [SerializeField] private Vector3 customSpawnPosition;
        [SerializeField] private Vector3 customSpawnEuler;
        [SerializeField] private bool useCustomSpawn;

        [SerializeField] private GameObject loadingUI;
        public bool showUI = true;

        [Inject] private IFlashlightProvider _flashlightProvider;

        [Inject]
        private void Construct(ScenesManager scenesManager)
        {
            _scenesManager = scenesManager;
        }
        public Animator transition;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                LoadNext();
            }
        }

        public void LoadNext()
        {
            var fl = _flashlightProvider?.Current;
            if (fl != null)
                _flashlightProvider.PendingEssence = fl.GetEssense();

            if (useCustomSpawn)
            {
                if (exitSpawnPoint != null)
                    PendingSpawn.Set(exitSpawnPoint.position, exitSpawnPoint.rotation);
                else
                    PendingSpawn.Set(customSpawnPosition, Quaternion.Euler(customSpawnEuler));
            }

            if (showUI)
            {
                loadingUI.gameObject.SetActive(true);
            }

            _scenesManager.LoadSingle(SceneNames.GetName(targetScene));
        }
    }
}
