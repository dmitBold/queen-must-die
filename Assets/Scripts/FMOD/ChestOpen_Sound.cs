using UnityEngine;

public class ChestOpen_Sound : MonoBehaviour
{
    public FMODUnity.EventReference ChestEvent;

    public void PlayChestSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot(ChestEvent, transform.position);
    }
}