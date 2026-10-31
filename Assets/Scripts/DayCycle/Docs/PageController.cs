using Core;
using System;
using UnityEngine;

namespace Cards
{
    public class PageController : MonoBehaviour
    {
        public PageAnim pageAnim;

        public event Action OnPageArrived;
        public event Action OnPageLeft;

        void Awake()
        {
            pageAnim.OnArrived += () => OnPageArrived?.Invoke();
            pageAnim.OnLeft += () => OnPageLeft?.Invoke();
        }

        public void ShowPage()
        {
            pageAnim.PlayArrival();
        }

        public void EndPage()
        {
            pageAnim.PlayLeave();
        }

        
    }
}