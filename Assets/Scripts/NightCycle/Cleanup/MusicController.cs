using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace Core
{
    public class MusicController : MonoBehaviour
    {
        [Header("Архитектура FMOD")]
        [SerializeField] private bool useParameterApproach = true;

        [Header("Управление через параметр")]
        [SerializeField] private EventReference musicEventParameterDriven;
        [SerializeField] private EventReference AmbienceEventParameterDriven;
        [SerializeField] private string trackParameterName = "TrackIndex";

        [Header("Способ 2: Отдельные ивенты")]
        [SerializeField] private EventReference[] musicTracks;

        [Header("Настройки старта")]
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private int startIndex = 0;

        //[Header("Зависимости")]
        //[SerializeField] private AudioService audioService;

        private EventInstance currentMusicInstance;
        private EventInstance currentAmbienceInstance;
        private int currentTrackIndex = -1;

        private void Start()
        {
            if (playOnStart)
            {
                PlayTrack(startIndex);
            }
        }

        // метод вызывается через UnityEvents (передавая int)
        public void PlayTrack(int index)
        {
            if (currentTrackIndex == index) return;

            if (useParameterApproach)
            {
                PlayTrackViaParameter(index);
            }
            else
            {
                PlayTrackViaSeparateEvents(index);
            }

            currentTrackIndex = index;
        }

        private void PlayTrackViaParameter(int index)
        {
            // Если музыка еще не играет запускаем базовый контейнер
            if (!currentMusicInstance.isValid())
            {
                currentMusicInstance = RuntimeManager.CreateInstance(musicEventParameterDriven);
                RuntimeManager.AttachInstanceToGameObject(currentMusicInstance, transform);
                currentMusicInstance.start();
                currentMusicInstance.release();
            }

            // переключение через параметр
            //  currentMusicInstance.setParameterByName(trackParameterName, index);
            RuntimeManager.StudioSystem.setParameterByName(trackParameterName, index);
        }

        private void PlayTrackViaSeparateEvents(int index)
        {
            if (musicTracks == null || index < 0 || index >= musicTracks.Length)
            {
                Debug.LogWarning($"[MusicController] Индекс трека {index} вне диапазона!");
                return;
            }

            // останавливаем предыдущий трек
            StopMusic(true);

            // Запускаем новый отдельный ивент
            //currentMusicInstance = RuntimeManager.CreateInstance(musicTracks[index]);           
            //RuntimeManager.AttachInstanceToGameObject(currentMusicInstance, transform);
           // currentMusicInstance.start();
           // currentMusicInstance.release();          
        }

        public void StopMusic(bool fadeOut = true)
        {
            if (currentMusicInstance.isValid())
            {
                currentMusicInstance.stop(fadeOut ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
                currentMusicInstance.clearHandle();
                currentAmbienceInstance.stop(fadeOut ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
                currentAmbienceInstance.clearHandle();
            }
            currentTrackIndex = -1;
        }

        public void StartMusic()
        {
            if (currentTrackIndex != -1) return; // Музыка уже играет
            PlayTrack(startIndex);
        }

        private void OnDestroy()
        {
            // глушим музыку при уничтожении объекта или смене сцены
            StopMusic(false);
        }
    }
}