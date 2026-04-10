using UnityEngine;

public class EyeFollow : MonoBehaviour
{
    [SerializeField] RectTransform eyeWhite;
    [SerializeField] float maxOffset = 8f;
    [SerializeField] float smoothTime = 0.08f;

    RectTransform pupil;
    Canvas canvas;
    Vector2 velocity;

    void Awake()
    {
        pupil = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            eyeWhite,
            Input.mousePosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out mousePos
        );

        Vector2 targetPos = Vector2.ClampMagnitude(mousePos, maxOffset * 0.9f);

        pupil.localPosition = Vector2.SmoothDamp(
            pupil.localPosition,
            targetPos,
            ref velocity,
            smoothTime
        );
    }
}
