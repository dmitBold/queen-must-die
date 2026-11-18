using FMODUnity;
using UnityEngine;

/// <summary>
/// Громкость экземпляра соседнего StudioEventEmitter: у самого эмиттера такого поля нет.
/// Нужен там, где звук должен быть тише, чем в FMOD-ивенте (например, приглушённый огонь).
/// </summary>
[RequireComponent(typeof(StudioEventEmitter))]
public class EmitterVolume : MonoBehaviour
{
    /// <summary>
    /// Множитель громкости экземпляра, 0–1. Перемножается с громкостью ивента и VCA.
    /// </summary>
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    private StudioEventEmitter _emitter;
    private System.IntPtr _appliedHandle;

    private void Awake()
    {
        _emitter = GetComponent<StudioEventEmitter>();
    }

    // Эмиттер создаёт экземпляр при каждом включении объекта, а порядок OnEnable у компонентов не задан,
    // поэтому громкость ставится в первом кадре, где появился новый экземпляр.
    private void LateUpdate()
    {
        var instance = _emitter.EventInstance;
        if (!instance.isValid() || instance.handle == _appliedHandle) return;

        instance.setVolume(volume);
        _appliedHandle = instance.handle;
    }
}
