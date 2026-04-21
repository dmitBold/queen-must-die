using System;
using UnityEngine;

public class FeatherAnim : MonoBehaviour
{
    [SerializeField] Animator animator;

    public event Action OnSignFinished;

    public void PlaySign()
    {
        animator.SetTrigger("sign");
    }

    public void FinishSignAnim()
    {
        OnSignFinished?.Invoke();
    }
}