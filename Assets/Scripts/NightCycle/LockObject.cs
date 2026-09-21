using UnityEngine;

namespace NightCycle
{
    public class LockObject : MonoBehaviour
    {
        public AudioClip LockSound;
        public bool isLocked = true;

        public void Unlock()
        {
            isLocked = false;
            this.enabled = false;
        }
    }
}