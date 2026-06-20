using DI;
using UnityEngine;
using UnityEngine.Animations;
using Zenject;
using Zenject.SpaceFighter;

public class EyeConstraint : MonoBehaviour
{
    private IPlayerProvider _playerProvider;
    private Player _player;
    [SerializeField] AimConstraint _lookAtConstraint;

    [Inject]
    public void Construct(
            [Inject] IPlayerProvider playerProvider,
            [Inject] Player player)
            
    {
        _playerProvider = playerProvider;
        _player = player;
    }

    private void Start()
    {
        // 1. Проверяем, сработал ли Zenject-инжект
        if (_playerProvider == null)
        {
            Debug.LogError("EyeConstraint: _playerProvider не был внедрен через Zenject!", this);
            return;
        }

        //var player = _playerProvider.CurrentPlayer;

        // 2. Проверяем, существует ли игрок в этот момент времени
        if (_player == null /*|| player.Rigidbody == null*/)
        {
            Debug.LogWarning("EyeConstraint: Игрок или его Rigidbody еще не созданы в Start. Попробуйте использовать Coroutine или более поздний вызов.", this);
            return;
        }

        if (_lookAtConstraint == null)
        {
            _lookAtConstraint = GetComponent<AimConstraint>();
        }

        ConstraintSource source = new ConstraintSource
        {
            sourceTransform = _player.Rigidbody.transform,
            weight = 1f
        };

        _lookAtConstraint.AddSource(source);
        _lookAtConstraint.constraintActive = true;
    }

}
