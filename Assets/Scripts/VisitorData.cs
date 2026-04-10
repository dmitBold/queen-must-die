using UnityEngine;
using UnityEngine.Timeline;

[CreateAssetMenu(fileName = "VisitorData", menuName = "Scriptable Objects/VisitorData")]
public class VisitorData : ScriptableObject
{
    public GameObject prefab;
    public AudioClip arrivalSound;
    public RuntimeAnimatorController animator;
    public TimelineAsset arrivalTime;
    public TimelineAsset leavelTime;

    [Header("Placement Settings")]
    public Vector3 localOffset;    // Смещение относительно слота
    public Vector3 localRotation;  // Поворот (в градусах)
    public float scaleMultiplier = 1f;
}
