using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// ѕроигрывание FMOD-ивентов и громкость музыки и эффектов (VCA FMOD-проекта, значени€ хран€тс€ в PlayerPrefs).
    /// </summary>
    public class AudioService : MonoBehaviour
    {
        private const string MusicVcaPath = "vca:/Music";
        private const string SfxVcaPath = "vca:/SFX";
        private const string MusicVolumeKey = "MusicVolume";
        private const string SfxVolumeKey = "SfxVolume";
        private const float DefaultVolume = 0.75f;

        /// <summary>
        /// —охранЄнна€ громкость музыки, 0Ц1.
        /// </summary>
        public float MusicVolume => PlayerPrefs.GetFloat(MusicVolumeKey, DefaultVolume);

        /// <summary>
        /// —охранЄнна€ громкость эффектов, 0Ц1.
        /// </summary>
        public float SfxVolume => PlayerPrefs.GetFloat(SfxVolumeKey, DefaultVolume);

        // VCA живут в FMOD-системе, а не в сцене, поэтому достаточно выставить их один раз при старте игры.
        private void Start()
        {
            ApplyVolume(MusicVcaPath, MusicVolume);
            ApplyVolume(SfxVcaPath, SfxVolume);
        }

        /// <summary>
        /// «адаЄт и сохран€ет громкость музыки, 0Ц1.
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, volume);
            ApplyVolume(MusicVcaPath, volume);
        }

        /// <summary>
        /// «адаЄт и сохран€ет громкость эффектов, 0Ц1.
        /// </summary>
        public void SetSfxVolume(float volume)
        {
            PlayerPrefs.SetFloat(SfxVolumeKey, volume);
            ApplyVolume(SfxVcaPath, volume);
        }

        private static void ApplyVolume(string vcaPath, float volume)
        {
            try
            {
                RuntimeManager.GetVCA(vcaPath).setVolume(volume);
            }
            catch (VCANotFoundException)
            {
                Debug.LogWarning($"AudioService: в FMOD-банках нет {vcaPath}, громкость не применена.");
            }
        }

        /// <summary>
        /// ќднократно проигрывает ивент. ѕуста€ ссылка и пропуск катсцены игнорируютс€.
        /// </summary>
        public void PlayFMODEvent(EventReference eventReference, Vector3 position = default)
        {
            if (eventReference.IsNull || CutsceneSkipper.IsSkipping) return;
            RuntimeManager.PlayOneShot(eventReference, position);
        }

        /// <summary>
        /// ќднократно проигрывает ивент по пути, например "event:/UI/Text".
        /// </summary>
        public void PlayFMODEvent(string eventPath, Vector3 position = default)
        {
            if (string.IsNullOrEmpty(eventPath) || CutsceneSkipper.IsSkipping) return;
            RuntimeManager.PlayOneShot(eventPath, position);
        }

        /// <summary>
        /// «апускает 3D-ивент в точке с заданной дальностью слышимости. ќстановить и освободить экземпл€р должен вызывающий код:
        /// FMOD-экземпл€ры переживают выгрузку сцены. ƒл€ пустой ссылки возвращает невалидный экземпл€р.
        /// </summary>
        public EventInstance PlayFMODEventAtPoint(EventReference eventReference, Vector3 position, float minDistance, float maxDistance)
        {
            if (eventReference.IsNull) return default;

            EventInstance instance = RuntimeManager.CreateInstance(eventReference);
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
            instance.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, minDistance);
            instance.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, maxDistance);
            instance.start();
            return instance;
        }
    }
}
