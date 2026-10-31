// CinemachineBrainProvider.cs Ч где угодно в проекте
using Unity.Cinemachine;
using UnityEngine;

public class CinemachineBrainProvider
{
    public CinemachineBrain Current
    {
        get
        {
            var cam = Camera.main;
            if (cam != null && cam.TryGetComponent<CinemachineBrain>(out var b))
                return b;

#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<CinemachineBrain>();
#else
            return Object.FindObjectOfType<CinemachineBrain>();
#endif
        }
    }
}