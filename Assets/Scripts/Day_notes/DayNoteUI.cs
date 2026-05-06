using Cards;
using Core;
using Dialogue;
using Inventory;
using NightCycle;
using System;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static Inventory.InventoryUI;
using static System.Net.Mime.MediaTypeNames;

public class DayNoteUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    private GameObject panel;
    [SerializeField] private Transform NoteSlotParent;
    [SerializeField] private NoteSlot NoteSlotPrefab;
    [SerializeField]
    private GameObject Textpanel;
    [SerializeField] private UnityEngine.UI.Image TextImage;
    [SerializeField] private TextMeshProUGUI NoteText;

    [Header("State")] 
    public bool isOpen;
    public bool isTextOpen;

    [SerializeField] DayNoteDialogue dialogueManager;

    DayNoteData currentNote;
    public DayNoteManager NoteManager;

    private void Start()
    {
        NoteManager.OnNotesChanged += Refresh;
    }

    private void OnDestroy()
    {
        NoteManager.OnNotesChanged -= Refresh;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        if (!CanOpen())
            return;

        if (isOpen)
            Close();
        else
            Open();
    }

    public void ToggleText(DayNoteData note)
    {
        if(note == currentNote)
        {
            if (isTextOpen)
                CloseText();
            else
            {
                OpenText();
                SetNote(note);
            }
        }
        else
        {
            OpenText();
            SetNote(note);
            currentNote = note;
        }
        /*if (isTextOpen)
            CloseText();
        else
        {
            OpenText();
            SetNote(note);
        }*/
    }

    private bool CanOpen()
    {
        return true;
    }

    private void Open()
    {
        isOpen = true;
        panel.SetActive(true);
    }

    private void Close()
    {
        isOpen = false;
        panel.SetActive(false);
    }

    private void OpenText()
    {
        isTextOpen = true;
        Textpanel.SetActive(true);
    }

    private void CloseText()
    {
        isTextOpen = false;
        Textpanel.SetActive(false);
    }

    public void OnNoteClicked(DayNoteData note)
    {
        Debug.Log("AAAA");
        ToggleText(note);
        //SetNote(note);
    }

    public void SetNote(DayNoteData note)
    {
        TextImage.sprite = note.icon;

        if (note.NotePages[0] == null)
        {
            Debug.Log("G&G&G&G&G&G&GG&77777777777");
        }

        dialogueManager.Show(note.NotePages);
    }

    private void Refresh()
    {
        foreach (Transform child in NoteSlotParent)
        {
            Destroy(child.gameObject);
        }

        var notes = NoteManager.GetAllNotes();

        foreach (var note in notes)
        {
            NoteSlot slot = Instantiate(NoteSlotPrefab, NoteSlotParent);

            slot.Set(note, this);
        }
    }
}
