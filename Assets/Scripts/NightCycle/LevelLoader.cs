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

        public void LoadNext(){
            _scenesManager.LoadSingle(SceneNames.GetName(targetScene));
        }
    }
}
