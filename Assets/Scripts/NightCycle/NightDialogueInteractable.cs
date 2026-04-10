using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class NightDialogueInteractable : MonoBehaviour, IFocusable
{
    [Header("Настройки диалога")]
    [TextArea(3, 5)]
    public string[] dialoguePages;

    //TEST
    public AudioClip interact_sound;
    bool play_once = true;
    //TEST

    public void OnEnterFocus()
    {
        PlayIneractionSound();
        NightDialogueManager.Instance.StartDialogue(this, dialoguePages);
    }

    public void OnExitFocus()
    {
        NightDialogueManager.Instance.ForceEndDialogue();
    }

    public void PlayIneractionSound()
    {
        Debug.Log("sss");
        //TEST
        if (interact_sound != null)
        {
            if (play_once)
            {
                Debug.Log("sss");
                SoundManager.Instance.PlaySound(interact_sound);
                play_once = false;
            }
        }
        //TEST
    }
}