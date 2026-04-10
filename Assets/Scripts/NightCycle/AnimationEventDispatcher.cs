using UnityEngine;
using UnityEngine.Events;

public class AnimationEventDispatcher : MonoBehaviour
{
    public UnityEvent onAnimationTrigger;

    public void TriggerEvent()
    {
        onAnimationTrigger?.Invoke();
    }
}
