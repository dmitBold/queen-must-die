using Core;
using UnityEngine;
using UnityEngine.Events;   
using Zenject;

namespace NightCycle
{
    public class TriggerHandler : MonoBehaviour
    {
        [SerializeField] private string targetTag = "Player";
        [SerializeField] private UnityEvent onTriggerEnter;
        public AudioClip triggerSound;
        public bool disable;
        
        
        private AudioService _audioService;

        [Inject]
        public void Constructor(AudioService audioService)
        {
            _audioService = audioService;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(targetTag))
            {
                if (triggerSound != null)
                {
                    _audioService.PlaySound(triggerSound);
                }
                onTriggerEnter?.Invoke();
                if (disable)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}