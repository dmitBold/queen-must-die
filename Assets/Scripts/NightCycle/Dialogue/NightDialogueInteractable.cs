using Core;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Animations.Rigging;
using Zenject;

namespace NightCycle
{
    [RequireComponent(typeof(Interactable))]
    public class NightDialogueInteractable : MonoBehaviour, IFocusable
    {
        [Header("��������� �������")]
        [TextArea(3, 5)]
        public string[] dialoguePages;
        public string[] dialogueNames;
        //TEST
        public EventReference interact_sound;
        public EventReference typeSound;
        bool play_once = true;

        public List<EventReference> dialogue_sounds;
        public int dialogue_sounds_index = 0;
        //TEST

        //test
        [Header("События после диалога")]
        public UnityEvent onDialogueCompleted;
        public bool triggerEventOnlyOnce = true;
        private bool hasTriggeredEvent = false;
        //test

        [Header("Взгляд на игрока")]
        /// <summary>Сколько секунд голова поворачивается к игроку в начале диалога и возвращается после него.</summary>
        [SerializeField] private float _headTurnDuration = 0.6f;
        /// <summary>Сглаживание слежения за игроком: чем больше, тем ленивее голова догоняет его движения.</summary>
        [SerializeField] private float _headFollowSmoothTime = 0.25f;

        private const float HEAD_TARGET_START_DISTANCE = 2f;

        private AudioService _audioService;
        // Optional: объекты с диалогом стоят и в сценах без игрока.
        [InjectOptional] private FirstPersonController _player;

        private Rig _headLookRig;
        private Transform _headLookTarget;
        private Transform _headBone;
        private bool _isLookingAtPlayer;
        private Vector3 _headTargetVelocity;

        [Inject]
        public void Constructor(AudioService audioService)
        {
            _audioService = audioService;
        }

        private void Awake()
        {
            // Слежение головой включается само, если на объекте есть риг с Multi-Aim Constraint;
            // цель берётся из первого источника первой такой констрейнты. Риги без неё (например, у мёртвых големов) не трогаем.
            MultiAimConstraint aim = GetComponentInChildren<MultiAimConstraint>();
            if (aim == null) return;

            _headLookRig = aim.GetComponentInParent<Rig>();
            if (_headLookRig == null || aim.data.sourceObjects.Count == 0 || aim.data.sourceObjects[0].transform == null)
            {
                Debug.LogWarning($"{name}: Multi-Aim Constraint без рига или цели, голова не будет следить за игроком.", this);
                _headLookRig = null;
                return;
            }

            _headLookTarget = aim.data.sourceObjects[0].transform;
            _headBone = aim.data.constrainedObject;
            _headLookRig.weight = 0f;
        }

        private void Update()
        {
            if (_headLookRig == null || _player == null) return;
            if (!_isLookingAtPlayer && _headLookRig.weight <= 0f) return;

            _headLookTarget.position = Vector3.SmoothDamp(_headLookTarget.position, _player.View.position,
                ref _headTargetVelocity, _headFollowSmoothTime);

            float step = _headTurnDuration > 0f ? Time.deltaTime / _headTurnDuration : 1f;
            _headLookRig.weight = Mathf.MoveTowards(_headLookRig.weight, _isLookingAtPlayer ? 1f : 0f, step);
        }

        public void OnEnterFocus()
        {
            // В фокусе PlayerInteraction не обновляет подсказку наведения, поэтому она висела бы поверх диалога;
            // после выхода из фокуса луч снова покажет её сам.
            if (HUDController.instance != null) HUDController.instance.DisableInteractionText();
            StartLookingAtPlayer();
            PlayIneractionSound();

            NightDialogueManager.Instance.StartDialogue(this, dialoguePages, dialogueNames, typeSound, () =>
            {
                if (triggerEventOnlyOnce && hasTriggeredEvent) return;

                onDialogueCompleted?.Invoke();
                hasTriggeredEvent = true;
            });
        }

        public void OnExitFocus()
        {
            _isLookingAtPlayer = false;
            NightDialogueManager.Instance.ForceEndDialogue();
        }

        private void StartLookingAtPlayer()
        {
            if (_headLookRig == null || _player == null) return;

            // Если голова уже отвернулась, цель стартует перед лицом, чтобы поворот шёл от текущего взгляда, а не рывком.
            if (_headLookRig.weight <= 0f)
            {
                _headLookTarget.position = _headBone.position + transform.forward * HEAD_TARGET_START_DISTANCE;
                _headTargetVelocity = Vector3.zero;
            }

            _isLookingAtPlayer = true;
        }

        public void PlayIneractionSound()
        {
            Debug.Log("sss");
            //TEST
            if (!interact_sound.IsNull)
            {
                if (play_once)
                {
                    Debug.Log("sss");
                   _audioService.PlayFMODEvent(interact_sound);
                    play_once = false;
                }
            }
            //TEST
        }
    }
}