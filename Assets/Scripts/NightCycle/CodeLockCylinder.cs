using UnityEngine;
using System.Collections;

public class CodeLockCylinder : MonoBehaviour
{
    public int CurrentValue { get; private set; } = 1;

    [SerializeField] float rotationDuration = 0.3f;
    private bool isRotating = false;

    // Событие, которое вызывается при повороте
    public System.Action OnValueChanged;

    private const float stepAngle = 120;

    public void Rotate()
    {
        if (isRotating) return;

        CurrentValue = (CurrentValue + 1) % 3;
        StartCoroutine(RotateRoutine());
    }

    private IEnumerator RotateRoutine()
    {
        isRotating = true;

        Quaternion startRotation = transform.localRotation;

        Quaternion endRotation = startRotation * Quaternion.Euler(0f, stepAngle, 0f);
        float elapsed = 0;
        while (elapsed < rotationDuration)
        {
            transform.localRotation = Quaternion.Slerp(startRotation, endRotation, elapsed / rotationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = endRotation;
        isRotating = false;

        OnValueChanged?.Invoke();
    }
}