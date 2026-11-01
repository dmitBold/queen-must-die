using System;
using UnityEngine;

namespace Environment
{
    public class CageController : MonoBehaviour
    {
        [SerializeField] Animator animator;

        public event Action OnArrived;
        public event Action OnLeft;

        public void PlayArrival()
        {
            animator.ResetTrigger("Leave");
            animator.SetTrigger("Arrive");
        }

        public void PlayLeave()
        {
            animator.ResetTrigger("Arrive");
            animator.SetTrigger("Leave");
        }

        public void Anim_Arrived()
        {
            animator.ResetTrigger("Arrive");
            OnArrived?.Invoke();
        }

        public void Anim_Left()
        {
            animator.ResetTrigger("Leave");
            OnLeft?.Invoke();
        }
    }
}
