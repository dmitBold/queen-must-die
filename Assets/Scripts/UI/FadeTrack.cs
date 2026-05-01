using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0f, 0f, 0f)]
[TrackBindingType(typeof(FadeController))]
[TrackClipType(typeof(FadeClip))]
public class FadeTrack : TrackAsset
{
}