using UnityEngine;
using System.Collections;

public class MusicTriggerHOM : MonoBehaviour
{

    public float door_delay;
    public GameObject door;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log("TRTRTRTRTRTR");
            CleanupManager.Instance.StartCleanup();
            CleanupManager.Instance.UpdateMusic();
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