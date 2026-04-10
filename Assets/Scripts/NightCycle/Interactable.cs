using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    Outline outline;
    public string message;
    public UnityEvent onInteraction;
    //test
    [SerializeField] AssemblyController assemblyController;
    public enum InteractionType { Instant, Hold }
    public InteractionType interactionType = InteractionType.Instant;
    public float holdDuration = 1.0f;
    //test

    //TEST
    //public AudioClip interact_sound;
    //bool play_once = true;
    //TEST


    void Start()
    {
        outline = GetComponent<Outline>();
        outline.enabled = false;
    }

    public void DisableOutline()
    {
        outline.enabled = false;
    }

    public void EnableOutline()
    {
        //outline.Rebuild();
        outline.enabled = true;
    }

    public void Interact()
    {
        onInteraction.Invoke();
    }

    /*public void PlayIneractionSound()
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
    }*/

    public void Interact_Assembly()
    {
        if (!assemblyController.isActive)
        {
            assemblyController.EnterAssembly();
            HUDController.instance.EnableInteractionText("E чтобы выйти");
        }
        else
        {
            //outline = null;
            //outline = GetComponent<Outline>();
            /*outline.enabled = false;
            outline.OutlineColor = outline.OutlineColor;
            outline.enabled = true;*/
            outline.Rebuild();
            outline.enabled = false;
            outline.enabled = true;
            assemblyController.ExitAssembly();
            
        }
    }

}
