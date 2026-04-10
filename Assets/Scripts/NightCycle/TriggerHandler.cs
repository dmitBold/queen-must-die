using UnityEngine;
using UnityEngine.Events;

public class TriggerHandler : MonoBehaviour
{
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private UnityEvent onTriggerEnter;
    public AudioClip triggerSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            if (triggerSound != null)
            {
                SoundManager.Instance.PlaySound(triggerSound);
            }
            onTriggerEnter?.Invoke();
            gameObject.SetActive(false);
        }
    }
}