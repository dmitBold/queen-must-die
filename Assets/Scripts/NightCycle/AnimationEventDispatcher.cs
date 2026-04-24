using UnityEngine;
using UnityEngine.Events;

namespace NightCycle
{
    public class AnimationEventDispatcher : MonoBehaviour
    {
        public UnityEvent onAnimationTrigger;

        public UnityEvent onAnimationTrigger_additional;

        public bool can_trigger = false;

        public void TriggerEvent()
        {
            if (can_trigger)
            {
                onAnimationTrigger?.Invoke();
            }
        }

        public void TriggerEvent_additional()
        {
            if (can_trigger)
            {
                onAnimationTrigger_additional?.Invoke();
            }
        }

        public void set_trigger_condition(bool condition)
        {
            can_trigger = condition;
        }
    }
}
