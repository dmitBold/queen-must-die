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

        /// <summary>
        /// Запирает объект обратно: для дверей, которые сюжет закрывает за игроком.
        /// </summary>
        public void Lock()
        {
            isLocked = true;
            this.enabled = true;
        }
    }
}