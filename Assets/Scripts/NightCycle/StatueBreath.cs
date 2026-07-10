using Core;
using UnityEngine;
using Zenject;
using UnityEngine.Events;
using System.Collections;

public class StatueBreath : MonoBehaviour
{
    private AudioService _audioService;
    public AudioClip breatheSound;
    public UnityEvent BreathCompleted;
    [SerializeField] private float delay;

    [Inject]
    public void Constructor(AudioService audioService)
    {
        _audioService = audioService;
    }

    public void Breathe()
    {
        StartCoroutine(BreatheRoutine());
    }

    IEnumerator BreatheRoutine()
    {
        if (breatheSound != null)
        {
            _audioService.PlaySound(breatheSound);
        }
        yield return new WaitForSeconds(delay);
        BreathCompleted?.Invoke();
    }

}
