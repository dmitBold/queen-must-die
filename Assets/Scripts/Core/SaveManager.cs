using Core;
using DI;
using Inventory;
using NightCycle;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Zenject;
using Zenject.SpaceFighter;

public class SaveManager : MonoBehaviour
{
    //[Inject] private DiContainer _container;
    [Inject] private IPlayerProvider _playerProvider;
    [Inject] private ScenesManager _scenesManager;
    [Inject] private SaveSystem _saveSystem;
    [Inject] private InventoryManager _inventoryManager;
    //[Inject] private PlayerFlashlight _flashlight;

    [System.Serializable]
    public struct SaveData
    {
        public float posX;
        public float posY;
        public float posZ;
        public string currentSceneName;
        public List<SavedItem> savedInventory;
        public List<string> triggeredWorldEvents;
        public float rotX;
        public float rotY;
        public float rotZ;
        public float rotW;
        //public bool isLightOn;
    }

    [System.Serializable]
    public struct SavedItem
    {
        public string itemId;
        public int amount;

        public SavedItem(string id, int count)
        {
            itemId = id;
            amount = count;
        }
    }

    private void Update()
    {
        if (Keyboard.current.numpad1Key.wasPressedThisFrame)
        {
            Debug.Log("SAVE");
            SaveGame();
        }
        if (Keyboard.current.numpad0Key.wasPressedThisFrame)
        {
            Debug.Log("LOAD");
            LoadGame();
        }
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        //var player = _container.TryResolve<Player>();
        var player = _playerProvider.CurrentPlayer;
        if (player != null)
        {
            data.posX = player.Position.x;
            data.posY = player.Position.y;
            data.posZ = player.Position.z;
            data.rotX = player.Rotation.x;
            data.rotY = player.Rotation.y;
            data.rotZ = player.Rotation.z;
            data.rotW = player.Rotation.w;
            Debug.Log(data.posX + " " + data.posY + " " + data.posZ);
        }
        else
        {
            Debug.Log("PLAYER_SAVE_IS_NULL");
        }

        /*if(_flashlight != null)
        {
            data.isLightOn = _flashlight.IsActive();
        }
        else
        {
            Debug.Log("FLASHLIGHT_SAVE_IS_NULL");
        }*/

        string unitySceneName = SceneManager.GetActiveScene().name;
        GameScene currentScene = GetGameSceneEnum(unitySceneName);
        data.currentSceneName = SceneNames.GetName(currentScene);

        data.savedInventory = _inventoryManager.GetInventorySaveData();

        data.triggeredWorldEvents = new List<string>(_saveSystem.SessionTriggeredEvents);

        _saveSystem.Save(data);
    }

    public void LoadGame()
    {
        if (_saveSystem.Load())
        {
            _scenesManager.LoadSingle(_saveSystem.CurrentData.currentSceneName);
        }
        else
        {
            Debug.LogWarning("Save file not found!");
        }
    }

    private GameScene GetGameSceneEnum(string sceneName)
    {
        switch (sceneName)
        {
            case SceneNames.Night: return GameScene.Night;
            case SceneNames.Day: return GameScene.Day;
            case SceneNames.Memories1: return GameScene.Memories1;
            case SceneNames.HoM: return GameScene.HoM;
            case SceneNames.MainMenu: return GameScene.MainMenu;
            case SceneNames.Assembly: return GameScene.Assembly;
            case SceneNames.IntroDialog: return GameScene.IntroDialog;
            case SceneNames.IntroDialogue2: return GameScene.IntroDialogue2;
            case SceneNames.Throne: return GameScene.Throne;
            case SceneNames.Night1_final: return GameScene.Night1_final;
            default: return GameScene.Night;
        }
    }
}