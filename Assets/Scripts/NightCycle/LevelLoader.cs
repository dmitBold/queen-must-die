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

        [Header(" уда спавнить игрока в новой сцене")]
        [SerializeField] private Transform exitSpawnPoint;

        // либо €вные координаты:
        [SerializeField] private Vector3 customSpawnPosition;
        [SerializeField] private Vector3 customSpawnEuler;
        [SerializeField] private bool useCustomSpawn;

        [SerializeField] private GameObject loadingUI;
        public bool showUI = true;

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
            if (useCustomSpawn)
            {
                if (exitSpawnPoint != null)
                    PendingSpawn.Set(exitSpawnPoint.position, exitSpawnPoint.rotation);
                else
                    PendingSpawn.Set(customSpawnPosition, Quaternion.Euler(customSpawnEuler));
            }
            // если useCustomSpawn == false Ч PendingSpawn не трогаем,
            // PlayerInstaller возьмЄт свой _spawnPoint

            if (showUI)
            {
                loadingUI.gameObject.SetActive(true);
            }

            _scenesManager.LoadSingle(SceneNames.GetName(targetScene));
        }
    }
}
