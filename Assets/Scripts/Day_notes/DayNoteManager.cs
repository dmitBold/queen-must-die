using Inventory;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class DayNoteManager : MonoBehaviour
{
    List<DayNoteData> notes = new();

    public event Action OnNotesChanged;

    private SaveSystem _saveSystem;

    [Inject]
    public void Construct([InjectOptional] SaveSystem saveSystem, [InjectOptional] DayNoteDatabase noteDatabase)
    {
        _saveSystem = saveSystem;
        RestoreFromSave(noteDatabase);
    }

    // Восстанавливает записки из SaveSystem.SessionNoteIds (в том числе после смены сцены или загрузки)
    private void RestoreFromSave(DayNoteDatabase noteDatabase)
    {
        if (_saveSystem == null || noteDatabase == null) return;

        foreach (string noteId in _saveSystem.SessionNoteIds)
        {
            DayNoteData note = noteDatabase.GetNoteById(noteId);
            if (note != null && !notes.Contains(note))
            {
                notes.Add(note);
            }
        }

        OnNotesChanged?.Invoke();
    }

    public void AddNote(DayNoteData note)
    {
        notes.Add(note);
        _saveSystem?.RegisterNote(note != null ? note.id : null);
        OnNotesChanged?.Invoke();
    }

    public bool HasNote(DayNoteData note)
    {
        return notes.Contains(note);
    }

    public int GetCount()
    {
        return notes.Count;
    }

    public List<DayNoteData> GetAllNotes()
    {
        List<DayNoteData> result = new();

        foreach (DayNoteData note in notes)
        {
            result.Add(note);
        }

        return result;
    }

}
