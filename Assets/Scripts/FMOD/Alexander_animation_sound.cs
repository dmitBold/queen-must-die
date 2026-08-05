using UnityEngine;

public class Alexander_animation_sound : MonoBehaviour
{
    public FMODUnity.EventReference AlexanderFootstepEvent;
    FMOD.Studio.EventInstance Footstep;
    void AlexanderFootstep()
    {
        FMODUnity.RuntimeManager.PlayOneShot(AlexanderFootstepEvent, gameObject.transform.position);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
