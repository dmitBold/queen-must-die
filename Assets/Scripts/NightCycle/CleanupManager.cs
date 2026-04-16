using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;
using Zenject;


namespace NightCycle
{
    public class CleanupManager : MonoBehaviour
    {
        [SerializeField] List<GameObject> stages;
        [SerializeField] List<AudioClip> musicTracks;
        private bool isMusicStarted = false;
        private int currentStageIndex = 0;

        //test
        public Light scene_light;
        public List<CandleController> OrderedCandles;
        public List<CandleController> ImmediateCandles;
        public float CandleTime;
        public float FirstDelay;
        public AudioClip candleSound;
        public AudioClip TinderSound;
        //test

        //test
        public AudioClip KnockSound;
        public GameObject door;
        private AudioService _audioService;
        //test

        [Inject]
        public void Constructor(AudioService audioService)
        {
            _audioService = audioService;
        }

        private void Start()
        {
            //StartCoroutine(MusicLoop());
            //isMusicStarted = true;
            //UpdateMusic();
            foreach (CandleController candle in OrderedCandles)
            {
                candle.SetCandleState(false);
            }
            foreach (CandleController candle in ImmediateCandles)
            {

                candle.SetCandleState(false);
            }
        }

        public void UpdateMusic()
        {
            if (musicTracks.Count == 0) return;

            _audioService.musicSource.loop = true;

            int trackIndex = Mathf.Clamp(currentStageIndex, 0, musicTracks.Count - 1);
            AudioClip clipToPlay = musicTracks[trackIndex];

            if (_audioService.musicSource.clip != clipToPlay)
            {
                _audioService.musicSource.clip = clipToPlay;
                _audioService.musicSource.Play();
            }
        }

        public void StartCleanup()
        {
            _audioService.PlaySound(TinderSound);
            if (isMusicStarted) return;

            isMusicStarted = true;
        }

        public void AdvanceStage()
        {
            currentStageIndex++;
            UpdateMusic();
            apply_stage_effect();

            if (currentStageIndex < stages.Count)
            {
                Debug.Log($"������� �� ������: {stages[currentStageIndex].name}");
            }
            else
            {
                Debug.Log("������ ��������� ���������!");
            }
        }

        IEnumerator CandlesRoutine()
        {
            yield return new WaitForSeconds(FirstDelay);

            foreach(CandleController candle in OrderedCandles)
            {
                yield return new WaitForSeconds(CandleTime);
                _audioService.PlaySound(candleSound);
                candle.SetCandleState(true);
            }

            foreach (CandleController candle in ImmediateCandles)
            {
                candle.SetCandleState(true);
            }
            _audioService.PlaySound(candleSound, 2.0f);
        }

        public void apply_stage_effect()
        {
            switch (currentStageIndex)
            {
                /*case 1:
                scene_light.enabled = false;
                Debug.Log("1");
                break;*/
                case 2:
                    scene_light.enabled = false;
                    StartCoroutine(CandlesRoutine());
                    break;
                case 5:
                    _audioService.PlaySound(KnockSound);
                    door.GetComponent<Interactable>().enabled = true;
                    break;

                default:
                    //Debug.Log("other");
                    break;

            }
        }

        public void StopMusic()
        {
            _audioService.musicSource.Stop();
        }


    }
}