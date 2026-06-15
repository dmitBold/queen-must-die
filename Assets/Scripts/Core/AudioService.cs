using UnityEngine;
using FMODUnity;

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

        public AudioSource PlaySoundAtPoint_loop(AudioClip clip, Vector3 position, float volume = 1f, bool loop = false, float minDistance = 1f, float maxDistance = 20f)
        {
            if (clip == null) return null;

            GameObject go = new GameObject("[Audio] " + clip.name);
            go.transform.position = position;

            AudioSource source = go.AddComponent<AudioSource>();

            // Основные настройки
            source.clip = clip;
            source.volume = volume;
            source.loop = loop;

            source.spatialBlend = 1f;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
            source.rolloffMode = AudioRolloffMode.Linear;

            source.Play();

            if (!loop)
            {
                Destroy(go, clip.length);
            }

            return source;
        }

        //для FMOD

        // Воспроизведение через EventReference (самый удобный способ для инспектора)
        public void PlayFMODEvent(EventReference eventReference, Vector3 position = default)
        {
            if (eventReference.IsNull) return;
            RuntimeManager.PlayOneShot(eventReference, position);
        }

        // Воспроизведение через строковый путь (например: "event:/FS_Wood")
        public void PlayFMODEvent(string eventPath, Vector3 position = default)
        {
            if (string.IsNullOrEmpty(eventPath)) return;
            RuntimeManager.PlayOneShot(eventPath, position);
        }
    }
}