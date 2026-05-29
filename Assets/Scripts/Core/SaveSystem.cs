using UnityEngine;
using System.IO;

public class SaveSystem
{
    private readonly SaveManager _saveManager;
    private static SaveManager.SaveData _savedData = new SaveManager.SaveData();

    public SaveSystem(SaveManager saveManager)
    {
        _saveManager = saveManager;
    }

    public string SaveFileName()
    {
        string saveFile = Application.persistentDataPath + "/save.txt";
        return saveFile;
    }

    public void Save()
    {
        _saveManager.Save(ref _savedData);
        File.WriteAllText(SaveFileName(), JsonUtility.ToJson(_savedData, true));
    }

    public void Load()
    {
        string saveContent = File.ReadAllText(SaveFileName());

        _savedData = JsonUtility.FromJson<SaveManager.SaveData>(saveContent);
        _saveManager.Load(_savedData);
    }

}
