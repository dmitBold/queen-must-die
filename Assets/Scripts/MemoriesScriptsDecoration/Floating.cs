using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Header("Настройки парения")]
    public float floatHeight = 0.5f;   // Амплитуда
    public float floatSpeed = 1f;      // Скорость
    public float rotationSpeed = 0.5f; // Скорость вращения

    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
    }

    void Update()
    {
        // Плавное движение вверх-вниз
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);

        // Плавное вращение
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}