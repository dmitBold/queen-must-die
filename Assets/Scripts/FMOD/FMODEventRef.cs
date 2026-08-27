using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections;

public class FMODEventRef:MonoBehaviour
{
    public EventReference FmodEvent;
    private EventInstance FmodInstance;
    private Coroutine fadeCoroutine;

    private void Start()
    {
        FmodInstance = RuntimeManager.CreateInstance(FmodEvent);
        RuntimeManager.AttachInstanceToGameObject(FmodInstance, transform);
        FmodInstance.start();
        //FmodInstance.release();
    }
    //Это тест - выключение звука: плавное/мгновенное
    public void StopEvent(float fadeDuration)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        if (fadeDuration <= 0)
        {
            FmodInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            FmodInstance.release();
        }
        else
        {
            fadeCoroutine = StartCoroutine(FadeOutAndStop(fadeDuration));
        }
    }

    private IEnumerator FadeOutAndStop(float duration)
    {
        float currentTime = 0;

        FmodInstance.getVolume(out float startVolume);

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newVolume = Mathf.Lerp(startVolume, 0f, currentTime / duration);
            FmodInstance.setVolume(newVolume);
            yield return null;
        }

        FmodInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        FmodInstance.release();
        fadeCoroutine = null;
    }

    private void OnDestroy()
    {
        if (FmodInstance.isValid())
        {
            FmodInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            FmodInstance.release();
        }
    }


}
