using UnityEngine;
using UnityEngine.Events;

public class FadeTrigger : MonoBehaviour
{
    public float duration = 1f;
    public UnityEvent onFadeComplete_to_black;
    public UnityEvent onFadeComplete_to_clear;

    public void TriggerFadeToBlack()
    {
        // Вызываем синглтон фейда и передаем туда наше событие
        FadeController.Instance.FadeToBlack(duration, () => onFadeComplete_to_black?.Invoke());
    }

    public void TriggerFadeToClear()
    {
        FadeController.Instance.FadeToClear(duration, () => onFadeComplete_to_clear?.Invoke());
    }
}
