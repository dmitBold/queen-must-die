using Core;
using FMODUnity;
using Inventory;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace NightCycle
{
    public class AssemblySocket : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] ItemData requiredItem;
        [SerializeField] GameObject visualPart;
        public EventReference SocketFillEvent;
        public UnityEvent SocketEvent;
        public bool triggerOnnce = true;
        private AudioService _audioService;
        Outline outline;

        bool isFilled;

        public bool IsFilled => isFilled;
        public ItemData RequiredItem => requiredItem;

        [Inject]
        public void Constructor(AudioService audioService)
        {
            _audioService = audioService;
        }

        void Awake()
        {
            if (visualPart != null) {
                visualPart.SetActive(false);
            }
            outline = GetComponent<Outline>();
            //outline.enabled = false;
            SetHighlight(false);
        }

        public bool CanAccept(ItemData item)
        {
            return !isFilled && item == requiredItem;
        }

        public void Apply(ItemData item)
        {
            if (!CanAccept(item))
                return;

            isFilled = true;

            if (visualPart != null)
            {
                visualPart.SetActive(true);
            }

            SetHighlight(false);

            //test
            if (!SocketFillEvent.IsNull)
            {
                _audioService.PlayFMODEvent(SocketFillEvent, gameObject.transform.position);
            }
            //test

            if (triggerOnnce)
            {
                SocketEvent?.Invoke();
            }

        }

        public void Apply_now()
        {

            isFilled = true;

            if (visualPart != null)
            {
                visualPart.SetActive(true);
            }

            SetHighlight(false);

        }

        public void SetHighlight(bool state)
        {
            outline.enabled = state;
        }

        public void SetEvent(bool state)
        {
            triggerOnnce = state;
        }

    }
}
