using UnityEngine;



public class Statue_animation_fmod_audio : MonoBehaviour
    
{
    public FMODUnity.EventReference StatueFootstepEvent;
    FMOD.Studio.EventInstance Footstep;
    public FMODUnity.EventReference StatueSwordEvent;
    public FMODUnity.EventReference StatueHeadEvent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void StatueFootstep()
    {
        FMODUnity.RuntimeManager.PlayOneShot(StatueFootstepEvent, gameObject.transform.position);
        //FMODUnity.RuntimeManager.PlayOneShot(StatueFootstepEvent);
        //Footstep = FMODUnity.RuntimeManager.CreateInstance(StatueFootstepEvent);
        //Footstep.start();
    }
    void StatueSword()
    {
        FMODUnity.RuntimeManager.PlayOneShot(StatueSwordEvent, gameObject.transform.position);
    }
    void StatueHead()
    {
        FMODUnity.RuntimeManager.PlayOneShot(StatueHeadEvent, gameObject.transform.position);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
