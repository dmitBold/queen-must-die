using UnityEngine;

namespace NightCycle
{
    public class BuildLoggerDisabler
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad()
        {
            // ��������� ���� ������ � �����
#if !UNITY_EDITOR
            Debug.unityLogger.logEnabled = false;
#endif
        }
    }
}
