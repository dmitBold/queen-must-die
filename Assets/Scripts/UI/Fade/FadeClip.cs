using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class FadeClip : PlayableAsset, ITimelineClipAsset
{
    public FadeBehaviour template = new FadeBehaviour();

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        return ScriptPlayable<FadeBehaviour>.Create(graph, template);
    }
}