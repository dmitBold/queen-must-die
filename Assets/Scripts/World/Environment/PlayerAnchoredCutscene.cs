using UI;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using Zenject;

namespace NightCycle
{
    /// <summary>
    /// Катсцена, которая разыгрывается там, где стоит игрок. Play переносит корень к ногам игрока
    /// (с его поворотом по горизонтали), ставит камеру катсцены ровно во взгляд игрока, включает режим катсцены
    /// и запускает таймлайн. Всё содержимое катсцены — дочерние объекты корня, поэтому оно едет вместе с ним.
    /// </summary>
    public class PlayerAnchoredCutscene : MonoBehaviour
    {
        /// <summary>Таймлайн катсцены. Play On Awake должен быть выключен.</summary>
        [SerializeField] private PlayableDirector _director;
        /// <summary>Камера катсцены, дочерняя корню. В момент старта совпадает со взглядом игрока.</summary>
        [SerializeField] private CinemachineCamera _camera;

        private UIManager _uiManager;
        private FirstPersonController _player;
        private CharacterController _body;

        [Inject]
        private void Construct(UIManager uiManager, FirstPersonController player)
        {
            _uiManager = uiManager;
            _player = player;
            _body = player.GetComponent<CharacterController>();
        }

        /// <summary>Запускает катсцену от текущего места игрока. Повторный вызов во время проигрывания ничего не делает.</summary>
        public void Play()
        {
            if (_director.state == PlayState.Playing) return;

            _uiManager.BeginCutscene();

            Transform body = _player.transform;
            Vector3 feet = body.TransformPoint(_body.center) - body.up * (_body.height * 0.5f);
            transform.SetPositionAndRotation(feet, Quaternion.Euler(0f, body.eulerAngles.y, 0f));

            Transform view = _player.View;
            _camera.transform.SetPositionAndRotation(view.position, view.rotation);
            if (view.TryGetComponent(out CinemachineCamera playerCamera))
                _camera.Lens = playerCamera.Lens;

            _director.Play();
        }
    }
}
