using UnityEngine;
using System.Collections;
using System;

namespace NightCycle.Puzzles
{
    public class RotatableItem : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [Tooltip("Ось поворота (например, 0, 1, 0 для Y)")]
        public Vector3 rotationAxis = Vector3.up;
        [Tooltip("На сколько градусов поворачивать за раз")]
        public float angleStep = 90f;
        [Tooltip("Целевой угол для решения паззла")]
        public float targetAngle = 180f;
        [Tooltip("Время за которое предмет поворачивается")]
        public float rotationDuration = 0.5f;

        public float CurrentAngle { get; private set; } = 0f;
        public bool IsRotating { get; private set; }

        public Action OnRotationFinished; // Событие, чтобы оповестить менеджер паззла

        private Quaternion startLocalRotation;

        private void Awake()
        {
            startLocalRotation = transform.localRotation;
        }

        public void Rotate()
        {
            if (IsRotating) return;
            StartCoroutine(RotateRoutine());
        }

        private IEnumerator RotateRoutine()
        {
            IsRotating = true;

            // Считаем новый угол
            CurrentAngle += angleStep;

            Quaternion initialRot = transform.localRotation;
            // Умножаем стартовое вращение на дополнительное, чтобы избежать gimbal lock и ошибок кватернионов
            Quaternion targetRot = startLocalRotation * Quaternion.AngleAxis(CurrentAngle, rotationAxis);

            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime / rotationDuration;
                transform.localRotation = Quaternion.Lerp(initialRot, targetRot, t);
                yield return null;
            }

            transform.localRotation = targetRot; // Жестко фиксируем в конце
            IsRotating = false;

            OnRotationFinished?.Invoke();
        }

        public bool IsAtTargetAngle()
        {
            // Используем Mathf.DeltaAngle для защиты от того, что 90 и -270 - это один и тот же угол.
            // Сравниваем с погрешностью 0.1f из-за особенностей float.
            return Mathf.Abs(Mathf.DeltaAngle(CurrentAngle, targetAngle)) < 0.1f;
        }

        // Метод для загрузки состояния из SaveSystem
        public void SetAngleInstantly(float savedAngle)
        {
            CurrentAngle = savedAngle;
            transform.localRotation = startLocalRotation * Quaternion.AngleAxis(CurrentAngle, rotationAxis);
        }
    }
}