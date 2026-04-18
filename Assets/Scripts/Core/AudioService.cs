using UnityEngine;

namespace Core
{
    public class AudioService : MonoBehaviour
    {
        [SerializeField] private AudioSource sfxSource;
        public static AudioClip pickup_Sound;
        public AudioSource musicSource;

        public void PlaySound(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, volume);
        }

        /*public void PlayPickUP(float volume = 1f)
        {
            if(pickup_Sound == null) return;
            sfxSource.PlayOneShot(pickup_Sound, volume);
            Debug.Log("VGVYVGYVYGVYGVYVG");
        }*/

        public void PlaySoundAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }
    }
}