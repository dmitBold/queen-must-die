using System.Collections;
using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class MusicTriggerHOM : MonoBehaviour
    {

        public float door_delay;
        public GameObject door;

        private CleanupManager _cleanupManager;

        [Inject]
        public void Constructor(CleanupManager cleanupManager)
        {
            _cleanupManager = cleanupManager;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                //Debug.Log("TRTRTRTRTRTR");
                _cleanupManager.StartCleanup();
                _cleanupManager.UpdateMusic();
                StartCoroutine(WaitAndExecute(door_delay));
                //gameObject.SetActive(false);
            }
        }

        IEnumerator WaitAndExecute(float duration)
        {
            yield return new WaitForSeconds(duration);
            door.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}