using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalAudioConfig", menuName = "Configs/AudioConfig")]
public class GlobalAudioConfig : ScriptableObject
{
    /// <summary>
    /// Звук подбора предмета, общий для всех PickupItem.
    /// </summary>
    public EventReference pickupSound;
}
