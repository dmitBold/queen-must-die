using UnityEngine;

public class AutoSaveOnStart : MonoBehaviour
{
    [SerializeField] private NightCycle.SaveLoadBridge _bridge;
    void Start() { _bridge.TriggerSave(); }
}