using UnityEngine;
using NightCycle;

namespace NightCycle.Puzzles
{
    public class SwapItem : MonoBehaviour
    {
        public int correctIndex; // Индекс на котором предмет должен стоять
        public int currentIndex; // Где он стоит сейчас

        private Outline outline;
        private Vector3 originalLocalPos;

        private void Awake()
        {
            outline = GetComponent<Outline>();
            if (outline != null) outline.enabled = false;
            originalLocalPos = transform.localPosition;
        }

        public void SetOutline(bool state)
        {
            if (outline != null) outline.enabled = state;
        }

        public void UpdateLocalPosition(Vector3 newPos)
        {
            transform.localPosition = newPos;
            originalLocalPos = newPos;
        }
    }
}