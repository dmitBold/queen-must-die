using Inventory;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NoteSlot : MonoBehaviour,
       /* IPointerEnterHandler,
        IPointerExitHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,*/
        IPointerClickHandler
{
    [SerializeField] TextMeshProUGUI label;
    DayNoteUI NoteUI;
    public DayNoteData note;

    public void Set(DayNoteData data, DayNoteUI ui)
    {
        note = data;
        NoteUI = ui;
        if (label == null)
        {
            Debug.Log("AAAAAAAAAAAAAAAAAAAAA2323");
        }
        label.text = data.NoteName;
        gameObject.SetActive(true);
    }

    public void Clear()
    {
        note = null;
        gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        NoteUI.OnNoteClicked(note);
    }
}
