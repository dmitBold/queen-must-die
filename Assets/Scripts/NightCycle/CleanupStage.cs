using UnityEngine;

public class CleanupStage : MonoBehaviour
{
    [SerializeField] int objectsToCleanup = 0;
    private int currentlyCleaned = 0;

    public void OnObjectCleaned()
    {
        currentlyCleaned++;
        CheckCompletion();
    }

    private void CheckCompletion()
    {
        if (currentlyCleaned >= objectsToCleanup)
        {
            CleanupManager.Instance.AdvanceStage();
        }
    }
}