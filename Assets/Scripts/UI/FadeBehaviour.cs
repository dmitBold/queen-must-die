using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class FadeBehaviour : PlayableBehaviour
{
    public float startAlpha = 0f;
    public float targetAlpha = 1f;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        FadeController fader = playerData as FadeController;
        if (fader == null) return;

        float progress = (float)(playable.GetTime() / playable.GetDuration());
        float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, progress);

        fader.SetAlphaDirectly(currentAlpha);
    }
}