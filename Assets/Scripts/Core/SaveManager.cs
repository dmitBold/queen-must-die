using NightCycle;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;
using Zenject.SpaceFighter;
using UnityEngine.SceneManagement;
using Core;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Inventory;

public class SaveManager : MonoBehaviour
{

    //[Inject] Player player;
    [Inject] private DiContainer _container;
    [Inject] private ScenesManager _scenesManager;
    [Inject] private SaveSystem _saveSystem;
    [Inject] private InventoryManager _inventoryManager;

    [System.Serializable]

    public struct SaveData
    {
        public float posX;
        public float posY;
        public float posZ;
        public string currentSceneName;
        public List<SavedItem> savedInventory;
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

    public void Save(ref SaveData data)
    {
        SaveScene(ref data);
        SavePlayer(ref data);
        SaveItems(ref data);
    }

    public void Load(SaveData data)
    {
        LoadScene(data);
        LoadPlpayer(data);
        LoadItems(data);
    }

    public void SavePlayer(ref SaveData data)
    {
        var player = _container.TryResolve<Player>();

        if (player != null)
        {
            data.posX = player.Position.x;
            data.posY = player.Position.y;
            data.posZ = player.Position.z;
        }
    }

    public void LoadPlpayer(SaveData data)
    {
        var player = _container.TryResolve<Player>();

        if (player != null)
        {
            player.Position = new Vector3(data.posX, data.posY, data.posZ);
        }
    }

    public void SaveScene(ref SaveData data)
    {
        string unitySceneName = SceneManager.GetActiveScene().name;

        GameScene currentScene = GetGameSceneEnum(unitySceneName);

        data.currentSceneName = SceneNames.GetName(currentScene);
    }

    private GameScene GetGameSceneEnum(string sceneName)
    {
        switch (sceneName)
        {
            case SceneNames.Night: return GameScene.Night;
            case SceneNames.Day: return GameScene.Day;
            case SceneNames.HoM: return GameScene.HoM;
            case SceneNames.MainMenu: return GameScene.MainMenu;
            case SceneNames.Assembly: return GameScene.Assembly;
            case SceneNames.IntroDialog: return GameScene.IntroDialog;
            case SceneNames.IntroDialogue2: return GameScene.IntroDialogue2;
            case SceneNames.Throne: return GameScene.Throne;
            default: return GameScene.Night;
        }
    }

    public void LoadScene(SaveData data)
    {
        _scenesManager.LoadSingle(data.currentSceneName);
    }

    private void Update()
    {
        if (Keyboard.current.numpad1Key.wasPressedThisFrame)
        {
            Debug.Log("SAVE");
            _saveSystem.Save();
        }
        if (Keyboard.current.numpad0Key.wasPressedThisFrame)
        {
            Debug.Log("LOAD");
            _saveSystem.Load();
        }
    }

    public void SaveItems(ref SaveData data)
    {
        data.savedInventory = _inventoryManager.GetInventorySaveData();
    }

    public void LoadItems(SaveData data)
    {
        _inventoryManager.LoadInventoryData(data.savedInventory);
    }

}
