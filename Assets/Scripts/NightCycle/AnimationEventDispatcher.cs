using UnityEngine;
using UnityEngine.Events;

namespace NightCycle
{
    public class AnimationEventDispatcher : MonoBehaviour
    {
        public UnityEvent onAnimationTrigger;

        public void TriggerEvent()
        {
            onAnimationTrigger?.Invoke();
        }
    }
}
