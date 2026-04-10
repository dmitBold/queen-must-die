using UnityEngine;

public class LockDoor : MonoBehaviour
{
    public AudioClip LockSound;

    public void Unlock()
    {
        this.enabled = false;
    }
}