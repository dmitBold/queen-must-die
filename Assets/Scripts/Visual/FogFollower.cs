using UnityEngine;

/// <summary>
/// Плавно ведёт объект с туманом за персонажем с небольшим отставанием.
/// Работает и на дочернем объекте персонажа: позиция считается в мировых координатах.
/// </summary>
public class FogFollower : MonoBehaviour
{
    /// <summary>Персонаж, за которым идёт туман. Обязательное поле.</summary>
    [SerializeField] private Transform _target;

    /// <summary>Скорость догоняния: чем больше, тем меньше туман отстаёт.</summary>
    [SerializeField, Min(0.01f)] private float _followSharpness = 5f;

    private Vector3 _position;
    private Quaternion _rotation;

    private void OnEnable()
    {
        if (_target == null)
        {
            Debug.LogError($"{nameof(FogFollower)}: не назначен _target на объекте {name}.", this);
            enabled = false;
            return;
        }

        _position = _target.position;
        _rotation = _target.rotation;
        transform.SetPositionAndRotation(_position, _rotation);
    }

    private void LateUpdate()
    {
        // Позиция хранится в полях: родитель уже сдвинул transform в этом кадре, и отставание потерялось бы.
        float blend = 1f - Mathf.Exp(-_followSharpness * Time.deltaTime);
        _position = Vector3.Lerp(_position, _target.position, blend);
        _rotation = Quaternion.Slerp(_rotation, _target.rotation, blend);
        transform.SetPositionAndRotation(_position, _rotation);
    }
}
