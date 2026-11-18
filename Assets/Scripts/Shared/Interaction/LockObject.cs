using FMODUnity;
using UnityEngine;

namespace NightCycle
{
    public class LockObject : MonoBehaviour
    {
        public EventReference LockSound;
        public bool isLocked = true;

        public void Unlock()
        {
            isLocked = false;
            this.enabled = false;
        }
    }
}