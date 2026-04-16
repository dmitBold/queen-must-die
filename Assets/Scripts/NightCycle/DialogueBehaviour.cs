using UnityEngine.Playables;

namespace NightCycle
{
    public class DialogueBehaviour : PlayableBehaviour
    {
        public string[] dialoguePages;
        public bool pauseTimeline;
        private bool hasTriggered = false;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (!hasTriggered && info.weight > 0)
            {
                PlayableDirector director = playable.GetGraph().GetResolver() as PlayableDirector;

                NightDialogueManager.Instance.StartTimelineDialogue(dialoguePages, director, pauseTimeline);

                hasTriggered = true;
            }
        }
    }
}