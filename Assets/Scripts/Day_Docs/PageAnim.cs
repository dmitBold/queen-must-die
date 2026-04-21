using System;
using UnityEngine;

public class PageAnim : MonoBehaviour
{
    [SerializeField] Animator animator;

    public event Action OnArrived;
    public event Action OnLeft;

    public void PlayArrival()
    {
        Debug.Log("GY*FEGFY*EEFYEFY");
        animator.SetTrigger("arrive");
    }

    public void PlayLeave()
    {
        animator.SetTrigger("leave");
    }

    public void FinishArrivalAnim()
    {
        OnArrived?.Invoke();
    }

    public void FinishLeaveAnim()
    {
        OnLeft?.Invoke();
    }
}