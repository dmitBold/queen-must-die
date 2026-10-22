using UnityEngine;

public class WallEnter_Sound : MonoBehaviour
{
    public FMODUnity.EventReference WallEnterEvent;

    public void PlayWallEnterSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot(WallEnterEvent, transform.position);
    }
}