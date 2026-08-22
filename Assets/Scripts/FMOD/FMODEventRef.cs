using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODEventRef:MonoBehaviour
{
    public EventReference FmodEvent;
    private EventInstance FmodInstance;

    private void Start()
    {
        FmodInstance = RuntimeManager.CreateInstance(FmodEvent);
        RuntimeManager.AttachInstanceToGameObject(FmodInstance, transform);
        FmodInstance.start();
        FmodInstance.release();
    }




}
