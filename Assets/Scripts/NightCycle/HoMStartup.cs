using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace NightCycle
{
    public class HoMStartup : MonoBehaviour
    {
      [FormerlySerializedAs("door_delay")]  public float delay;

        private CleanupManager _cleanupManager;

        [Inject]
        public void Constructor(CleanupManager cleanupManager)
        {
            _cleanupManager = cleanupManager;
        }

        void Start()
        {
            StartCoroutine(WaitAndExecute(delay));
        }

        IEnumerator WaitAndExecute(float duration)
        {
            yield return new WaitForSeconds(duration);
            _cleanupManager.StartCleanup();
            _cleanupManager.UpdateMusic();
        }
    }
}