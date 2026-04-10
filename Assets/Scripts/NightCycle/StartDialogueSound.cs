using UnityEngine;
using System.Collections;

public class StartDialogueSound : MonoBehaviour
{

    public AudioClip StartSound;

    [Header("Настройки диалога")]
    [TextArea(3, 5)]
    public string[] dialoguePages;

    public float delay;

    void Start()
    {
        SoundManager.Instance.PlaySound(StartSound);
        StartCoroutine(WaitAndExecute(delay));
        //NightDialogueManager.Instance.StartDialogue(null, dialoguePages);

        //PlayerStateController.Instance.SetMode(PlayerStateController.PlayerMode.Focused);
    }

    IEnumerator WaitAndExecute(float duration)
    {
        yield return new WaitForSeconds(duration);
        OnSoundEnded();
    }

    void OnSoundEnded()
    {
        NightDialogueManager.Instance.StartDialogue(null, dialoguePages);

        PlayerStateController.Instance.SetMode(PlayerStateController.PlayerMode.Focused);
    }

    void Update()
    {
        
    }
}
