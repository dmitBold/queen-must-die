using Core;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;   
using Zenject;

namespace NightCycle
{
    public class TriggerHandler : MonoBehaviour
    {
        [SerializeField] private string targetTag = "Player";
        [SerializeField] private UnityEvent onTriggerEnter;
        public EventReference triggerSound;
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
                _audioService.PlayFMODEvent(triggerSound);
                onTriggerEnter?.Invoke();
                if (disable)
                {
                    gameObject.SetActive(false);
                }
            }
        }

        public void triggerEvent()
        {
            onTriggerEnter?.Invoke();
        }
    }
}