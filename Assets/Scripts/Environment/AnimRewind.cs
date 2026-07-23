using UnityEngine;
using System.Collections;

public class AnimRewind : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float rewindSpeed = -5f;

    [Header("Имена состояний")]
    public string cleanStateName = "Boxes";
    public string idleStateName = "idle";

    private Coroutine rewindCoroutine;

    public void PlayForward()
    {
        if (animator == null) return;

        if (rewindCoroutine != null)
        {
            StopCoroutine(rewindCoroutine);
            rewindCoroutine = null;
        }

        animator.SetFloat("AnimSpeed", 1f);
        animator.Play(cleanStateName);
    }

    public void TriggerRewind()
    {
        Debug.Log("Anim_rewind");
        if (animator == null) return;

        animator.SetFloat("AnimSpeed", rewindSpeed);

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        float timeToWait = (stateInfo.normalizedTime * stateInfo.length) / Mathf.Abs(rewindSpeed);

        rewindCoroutine = StartCoroutine(WaitAndStop(timeToWait));
    }

    private IEnumerator WaitAndStop(float delay)
    {
        yield return new WaitForSeconds(delay);

        animator.Play(idleStateName);
        animator.SetFloat("AnimSpeed", 0f);

        rewindCoroutine = null;
    }
}