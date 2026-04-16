using UnityEngine;
using Zenject;

namespace NightCycle
{
    public class CleanupStage : MonoBehaviour
    {
        [SerializeField] int objectsToCleanup = 0;
        private int currentlyCleaned = 0;

        private CleanupManager _cleanupManager;

        [Inject]
        public void Constructor(CleanupManager cleanupManager)
        {
            _cleanupManager = cleanupManager;
        }

        public void OnObjectCleaned()
        {
            currentlyCleaned++;
            CheckCompletion();
        }

        private void CheckCompletion()
        {
            if (currentlyCleaned >= objectsToCleanup)
            {
                _cleanupManager.AdvanceStage();
            }
        }
    }
}