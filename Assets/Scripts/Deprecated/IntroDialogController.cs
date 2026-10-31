using NightCycle;
using UnityEngine;

public class IntroDialogController : MonoBehaviour
{
    [SerializeField] private NightDialogueInteractable _dialogInteractable;
    [SerializeField] private LevelLoader _levelLoader;
        

    private void Start()
    {
        _dialogInteractable.OnEnterFocus();
        NightDialogueManager.Instance.DialogEnded += _levelLoader.LoadNext;
    }
}