using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanupManager : MonoBehaviour
{
    public static CleanupManager Instance;

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
    //test
    void Awake()
    {
        Instance = this;
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

        SoundManager.Instance.musicSource.loop = true;

        int trackIndex = Mathf.Clamp(currentStageIndex, 0, musicTracks.Count - 1);
        AudioClip clipToPlay = musicTracks[trackIndex];

        if (SoundManager.Instance.musicSource.clip != clipToPlay)
        {
            SoundManager.Instance.musicSource.clip = clipToPlay;
            SoundManager.Instance.musicSource.Play();
        }
    }

    public void StartCleanup()
    {
        //Debug.Log("Музыка началась");
        SoundManager.Instance.PlaySound(TinderSound);
        if (isMusicStarted) return;

        isMusicStarted = true;
        Debug.Log("Музыка началась");
        //StartCoroutine(MusicLoop());
    }

    public void AdvanceStage()
    {
        currentStageIndex++;
        UpdateMusic();
        apply_stage_effect();

        if (currentStageIndex < stages.Count)
        {
            Debug.Log($"Переход на стадию: {stages[currentStageIndex].name}");
        }
        else
        {
            Debug.Log("Уборка полностью завершена!");
        }
    }

    /*IEnumerator MusicLoop()
    {
        while (true)
        {
            SoundManager.Instance.musicSource.loop = true;

            if (musicTracks.Count == 0) yield break;

            int trackIndex = Mathf.Clamp(currentStageIndex, 0, musicTracks.Count - 1);
            AudioClip clipToPlay = musicTracks[trackIndex];

            if (SoundManager.Instance.musicSource.clip != clipToPlay)
            {
                SoundManager.Instance.musicSource.clip = clipToPlay;
                SoundManager.Instance.musicSource.Play();

            }

            yield return null;
        }
    }*/

    IEnumerator CandlesRoutine()
    {
        yield return new WaitForSeconds(FirstDelay);

        foreach(CandleController candle in OrderedCandles)
        {
            yield return new WaitForSeconds(CandleTime);
            SoundManager.Instance.PlaySound(candleSound);
            candle.SetCandleState(true);
        }

        foreach (CandleController candle in ImmediateCandles)
        {
            candle.SetCandleState(true);
        }
        SoundManager.Instance.PlaySound(candleSound, 2.0f);
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
                SoundManager.Instance.PlaySound(KnockSound);
                door.GetComponent<Interactable>().enabled = true;
                break;

            default:
                //Debug.Log("other");
                break;

        }
    }

    public void StopMusic()
    {
        SoundManager.Instance.musicSource.Stop();
    }


}