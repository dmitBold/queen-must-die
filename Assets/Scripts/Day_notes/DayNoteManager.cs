using Inventory;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DayNoteManager : MonoBehaviour
{
    List<DayNoteData> notes = new();

    public event Action OnNotesChanged;
    //
    public DayNoteData test;
    public DayNoteData test1;
    //

    public void AddNote(DayNoteData note)
    {
        notes.Add(note);
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            AddNote(test);
            AddNote(test1);
        }
    }
}
