using System.Collections.Generic;
using Inventory;
using UnityEngine;

/// <summary>
/// База всех записок для восстановления их из сохранения по id. Устроена так же, как ItemDatabase и QuestDatabase.
/// </summary>
[CreateAssetMenu(fileName = "DayNoteDatabase", menuName = "Scriptable Objects/DayNoteDatabase")]
public class DayNoteDatabase : ScriptableObject
{
    [SerializeField] private List<DayNoteData> allNotes;

    private Dictionary<string, DayNoteData> _noteDict;

    private void Initialize()
    {
        _noteDict = new Dictionary<string, DayNoteData>();
        if (allNotes == null) return;

        foreach (var note in allNotes)
        {
            if (note != null && !string.IsNullOrEmpty(note.id))
            {
                _noteDict[note.id] = note;
            }
        }
    }

    public DayNoteData GetNoteById(string id)
    {
        if (_noteDict == null || _noteDict.Count == 0)
        {
            Initialize();
        }

        if (!string.IsNullOrEmpty(id) && _noteDict.TryGetValue(id, out var note))
        {
            return note;
        }

        Debug.LogError($"[DayNoteDatabase] Записка с ID {id} не найдена!");
        return null;
    }
}
