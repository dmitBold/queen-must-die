using UnityEngine;
using UnityEngine.EventSystems;

namespace Environment
{
    public class EyeButtonFX : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [SerializeField] RectTransform eyeRoot;
        [SerializeField] float pressedScale = 1.15f;
        [SerializeField] float hoverScale = 1.05f;
        [SerializeField] float smooth = 12f;

        Vector3 targetScale = Vector3.one;

        void Update()
        {
            eyeRoot.localScale = Vector3.Lerp(
                eyeRoot.localScale,
                targetScale,
                Time.deltaTime * smooth
            );
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            targetScale = Vector3.one * pressedScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            targetScale = Vector3.one * hoverScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            targetScale = Vector3.one * hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            targetScale = Vector3.one;
        }
    }
}
