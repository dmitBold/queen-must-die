using UnityEngine;

public class WardrobebeEnter_Sound : MonoBehaviour
{
    public FMODUnity.EventReference WardrobeEvent;

    public void PlayWardrobeSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot(WardrobeEvent, transform.position);
    }
}