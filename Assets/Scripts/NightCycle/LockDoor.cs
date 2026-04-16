using UnityEngine;

namespace NightCycle
{
    public class LockDoor : MonoBehaviour
    {
        public AudioClip LockSound;

        public void Unlock()
        {
            this.enabled = false;
        }
    }
}