using UnityEngine;
using System.IO;
using System.Collections.Generic;
public class SaveSystem
{
    public SaveManager.SaveData CurrentData { get; private set; }
    public bool HasLoadedData { get; private set; }

    public HashSet<string> SessionTriggeredEvents { get; private set; } = new HashSet<string>();

    private readonly string _saveFilePath;

    public SaveSystem()
    {
        _saveFilePath = Application.persistentDataPath + "/save.json";
    }

    public void Save(SaveManager.SaveData data)
    {
        CurrentData = data;
        File.WriteAllText(_saveFilePath, JsonUtility.ToJson(CurrentData, true));
    }

    public bool Load()
    {
        if (!File.Exists(_saveFilePath)) return false;

        string saveContent = File.ReadAllText(_saveFilePath);
        CurrentData = JsonUtility.FromJson<SaveManager.SaveData>(saveContent);

        SessionTriggeredEvents = new HashSet<string>(CurrentData.triggeredWorldEvents ?? new List<string>());

        HasLoadedData = true;
        return true;
    }

    public void ClearLoadedFlag()
    {
        HasLoadedData = false;
    }

    public void RegisterEvent(string eventId)
    {
        SessionTriggeredEvents.Add(eventId);
    }

    public void RemoveEvent(string eventId)
    {
        SessionTriggeredEvents.Remove(eventId);
    }
}