using UnityEngine;
using UnityEngine.Timeline;

namespace Visitors
{
    [CreateAssetMenu(fileName = "VisitorData", menuName = "Scriptable Objects/VisitorData")]
    public class VisitorData : ScriptableObject
    {
        public GameObject prefab;
        public AudioClip arrivalSound;
        public RuntimeAnimatorController animator;
        public TimelineAsset arrivalTime;
        public TimelineAsset leavelTime;

        [Header("Placement Settings")]
        public Vector3 localOffset;    // �������� ������������ �����
        public Vector3 localRotation;  // ������� (� ��������)
        public float scaleMultiplier = 1f;
    }
}
