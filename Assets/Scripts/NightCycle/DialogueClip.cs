using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class DialogueClip : PlayableAsset
{
    [Header("Настройки диалога")]
    [TextArea(3, 5)]
    public string[] dialoguePages;

    [Tooltip("Остановить ли таймлайн, пока идет диалог?")]
    public bool pauseTimeline = true;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<DialogueBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();

        behaviour.dialoguePages = dialoguePages;
        behaviour.pauseTimeline = pauseTimeline;

        return playable;
    }
}