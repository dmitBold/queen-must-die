using UnityEngine;

public class BuildLoggerDisabler
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        // Отключаем логи только в билде
#if !UNITY_EDITOR
            Debug.unityLogger.logEnabled = false;
#endif
    }
}
