using UnityEngine;

public class ColliderNameLogger : MonoBehaviour
{
    // 1. Для Character Controller (когда игрок врезается в стены/пол при движении)
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log($"[CharacterController] Коллизия с: {hit.collider.name}");
    }

    // 2. Для обычного Rigidbody (если у тебя на игроке или препятствиях стоят галочки Is Trigger)
    private void OnTriggerStay(Collider other)
    {
        Debug.Log($"[Trigger] Внутри триггера: {other.name}");
    }

    // 3. Для обычного Rigidbody (если сталкиваются два НЕ триггерных объекта с физикой)
    private void OnCollisionStay(Collision collision)
    {
        Debug.Log($"[Rigidbody] Физическое столкновение с: {collision.collider.name}");
    }
}
