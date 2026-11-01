using Cards;
using Core;
using DI;
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

public class DayNoteUI : MonoBehaviour, IUIScreen
{
    [Tooltip("Идентификатор окна записок. Клавиша открытия задаётся в самом ассете.")]
    [SerializeField] private UIScreenId screenId;

    [Header("UI References")]
    [SerializeField]
    private GameObject panel;
    [SerializeField] private Transform NoteSlotParent;
    [SerializeField] private NoteSlot NoteSlotPrefab;
    [SerializeField]
    private GameObject Textpanel;
    [SerializeField] private UnityEngine.UI.Image TextImage;
    [SerializeField] private TextMeshProUGUI NoteText;
    [SerializeField] private Canvas rootCanvas;

    [Header("State")] 
    public bool isOpen;
    public bool isTextOpen;



    [SerializeField] DayNoteDialogue dialogueManager;

    DayNoteData currentNote;
    public DayNoteManager NoteManager;

    

    [Inject]
    public void Construct(UIManager uiManager)
    {
        _uiManager = uiManager;
        uiManager.Register(this);
    }

    public UIScreenId Id => screenId;

    private UIManager _uiManager;

    /// <summary>
    /// Оставлен для кнопок и событий в сценах и префабах, назначенных через UnityEvent: делегирует UIManager.
    /// </summary>
    public void Toggle()
    {
        _uiManager.Toggle(screenId);
    }

    private void Start()
    {
        NoteManager.OnNotesChanged += Refresh;
        // Записки могли быть восстановлены из сохранения до подписки
        Refresh();
        rootCanvas.enabled = false;
    }

    private void OnDestroy()
    {
        NoteManager.OnNotesChanged -= Refresh;
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

    public void Open()
    {
        isOpen = true;
        rootCanvas.enabled = true;
        panel.SetActive(true);
    }

    public void Close()
    {
        isOpen = false;
        rootCanvas.enabled = false;
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
        //Debug.Log("AAAA");
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
