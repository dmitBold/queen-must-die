using System.Collections.Generic;
using UnityEngine;

namespace NightCycle
{
    /// <summary>
    /// Ведёт коллайдер объекта уборки вслед за анимируемым телом, поэтому игроку приходится следить за ним прицелом.
    /// В инспекторе подключить к Interactable: startHOLD — BeginTracking, resetHOLD и onInteraction — EndTracking.
    /// Соседние объекты ищутся по общей CleanupStage в родителях.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class CleanupTrackTarget : MonoBehaviour
    {
        // Тело уезжает примерно на 3 м: с обычной дальностью луч обрывался бы в конце движения.
        private const float TrackingInteractionReach = 6f;

        // Полсекунды дают игроку перевести прицел с края коллайдера на тело, иначе уборка сорвётся сразу.
        private const float NarrowingSeconds = 0.5f;

        /// <summary>Кость, за которой движется коллайдер.</summary>
        [SerializeField] private Transform _trackedBone;

        /// <summary>Размер зоны вокруг кости во время уборки: чем меньше, тем точнее нужно вести прицел.</summary>
        [SerializeField] private float _trackingBoxSize = 1.4f;

        private readonly List<BoxCollider> _hiddenColliders = new List<BoxCollider>();
        private BoxCollider _collider;
        private CleanupTrackTarget[] _siblings;
        private Vector3 _restCenter;
        private Vector3 _restSize;
        private Vector3 _restBoneLocal;
        private bool _hasRestPose;
        private float _trackingTime;

        /// <summary>Идёт ли сейчас уборка этого тела.</summary>
        public bool IsTracking { get; private set; }

        /// <summary>Дальность взаимодействия на время уборки.</summary>
        public float TrackingReach => TrackingInteractionReach;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider>();
            CleanupStage stage = GetComponentInParent<CleanupStage>();
            _siblings = stage != null ? stage.GetComponentsInChildren<CleanupTrackTarget>(true) : new CleanupTrackTarget[0];
        }

        /// <summary>
        /// Начинает уборку: соседние тела перестают перехватывать прицел. Повторные вызовы безопасны,
        /// startHOLD приходит каждый кадр удержания.
        /// </summary>
        public void BeginTracking()
        {
            if (IsTracking || !_collider.enabled)
            {
                return;
            }

            IsTracking = true;
            _trackingTime = 0f;
            foreach (CleanupTrackTarget sibling in _siblings)
            {
                if (sibling != this && sibling._collider.enabled)
                {
                    sibling._collider.enabled = false;
                    _hiddenColliders.Add(sibling._collider);
                }
            }
        }

        /// <summary>Завершает уборку и возвращает соседним телам коллайдеры.</summary>
        public void EndTracking()
        {
            IsTracking = false;
            foreach (BoxCollider hiddenCollider in _hiddenColliders)
            {
                hiddenCollider.enabled = true;
            }

            _hiddenColliders.Clear();
        }

        // LateUpdate: кость уже в позе текущего кадра. Коллайдер идёт за телом и при откате.
        private void LateUpdate()
        {
            if (_trackedBone == null || !_collider.enabled)
            {
                return;
            }

            Vector3 boneLocal = transform.InverseTransformPoint(_trackedBone.position);
            if (!_hasRestPose)
            {
                _hasRestPose = true;
                _restCenter = _collider.center;
                _restSize = _collider.size;
                _restBoneLocal = boneLocal;
            }

            Vector3 followingCenter = _restCenter + boneLocal - _restBoneLocal;
            if (!IsTracking)
            {
                _collider.center = followingCenter;
                _collider.size = _restSize;
                return;
            }

            _trackingTime += Time.deltaTime;
            float narrowing = Mathf.Clamp01(_trackingTime / NarrowingSeconds);
            _collider.center = Vector3.Lerp(followingCenter, boneLocal, narrowing);
            _collider.size = Vector3.Lerp(_restSize, Vector3.one * _trackingBoxSize, narrowing);
        }
    }
}
