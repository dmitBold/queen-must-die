using UnityEngine;

public class NoteController : MonoBehaviour
{
    public static NoteController Instance;

    [SerializeField] NoteView view;

    private bool isActive = false;
    [SerializeField] PlayerInteraction playerInteraction;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        view.Hide();
    }

    public void ShowNote(NoteInteractable note)
    {
        if (isActive) return;

        isActive = true;

        PlayerStateController.Instance.SetMode(PlayerStateController.PlayerMode.Focused);

        view.Show(note.noteImage, note.noteText);

        HUDController.instance.DisableInteractionText();
    }

    public void CloseNote()
    {
        if (!isActive) return;

        isActive = false;
        view.Hide();

        PlayerStateController.Instance.SetMode(PlayerStateController.PlayerMode.FreeMovement);
    }

    private void Update()
    {
        if (!isActive || !view.Tintroot.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (playerInteraction != null)
            {
                playerInteraction.ForceExitFocus();
            }
            return;
        }


        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetMouseButtonDown(0))
        {
            OnNextPressed();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetMouseButtonDown(1))
        {
            OnBackPressed();
        }
    }

    public void OnNextPressed()
    {
        var result = view.Skip();

        if (result == TypewriterEffect.SkipResult.DialogueFinished)
        {
            if (playerInteraction != null) playerInteraction.ForceExitFocus();
            else CloseNote();
        }
    }

    public void OnBackPressed()
    {
        view.Back();
    }
}