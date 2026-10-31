using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using NightCycle;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Core
{
    /// <summary>
    /// Skips every currently playing Timeline cutscene when Space is pressed.
    ///
    /// The skip is instant: within a single frame the timeline is evaluated in steps
    /// up to its end (no intermediate frames are rendered), so signals, activation
    /// tracks, animation events and the director's own end (stopped event, wrap mode)
    /// happen as during normal playback.
    /// Timeline dialogues are completed without being shown.
    /// Sound is suppressed: audio is muted for the duration of the skip and every
    /// sound started during the skip is stopped afterwards.
    ///
    /// Created automatically on game start, no scene setup required.
    /// </summary>
    public class CutsceneSkipper : MonoBehaviour
    {
        public static bool IsSkipping { get; private set; }

        private const KeyCode SkipKey = KeyCode.Space;

        // Size of one evaluation step. Signals are never missed (they fire for the whole
        // stepped range); only clips shorter than a step could be stepped over.
        private const float StepSeconds = 0.2f;

        // How close to the end the skip goes; the last bit is played by the director itself.
        private const double EndMarginSeconds = 0.005;

        // Safety limit so a broken timeline cannot keep the game muted forever.
        // Covers a whole chain of cutscenes.
        private const float MaxSkipRealSeconds = 15f;

        private const string MasterBusPath = "bus:/";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateInstance()
        {
            if (FindFirstObjectByType<CutsceneSkipper>() != null) return;

            var go = new GameObject("[CutsceneSkipper]");
            DontDestroyOnLoad(go);
            go.AddComponent<CutsceneSkipper>();
        }

        private void Update()
        {
            if (IsSkipping || !Input.GetKeyDown(SkipKey)) return;

            List<PlayableDirector> directors = FindPlayingTimelines();
            if (directors.Count > 0)
                StartCoroutine(SkipRoutine(directors));
        }

        private static List<PlayableDirector> FindPlayingTimelines()
        {
            var result = new List<PlayableDirector>();
            var all = FindObjectsByType<PlayableDirector>(FindObjectsSortMode.None);
            foreach (var director in all)
            {
                if (IsRunning(director) && director.playableAsset is TimelineAsset)
                    result.Add(director);
            }
            return result;
        }

        private static bool IsRunning(PlayableDirector director)
        {
            return director != null
                && director.isActiveAndEnabled
                && director.state == PlayState.Playing
                && director.playableGraph.IsValid();
        }

        private IEnumerator SkipRoutine(List<PlayableDirector> directors)
        {
            IsSkipping = true;
            var audio = new AudioSuppression();
            bool restartForChain = false;

            // finally guarantees that sound and the skip flag are restored
            // even if something inside the cutscene throws.
            try
            {
                audio.Begin();
                Debug.Log("[CutsceneSkipper] Skipping cutscene: " + DescribeDirectors(directors));

                // A dialogue that is currently paused on screen is completed the same way
                // a player click-through would complete it.
                var dialogueManager = NightDialogueManager.Instance;
                if (dialogueManager != null)
                {
                    foreach (var director in directors)
                        dialogueManager.CompleteTimelineDialogue(director);
                }

                float startTime = Time.realtimeSinceStartup;

                while (true)
                {
                    // Cutscenes are often chained: a signal at the end of one timeline starts
                    // the next one. Timelines started during the skip are skipped as well.
                    foreach (var started in FindPlayingTimelines())
                    {
                        if (!directors.Contains(started))
                            directors.Add(started);
                    }

                    bool anyRunning = false;
                    foreach (var director in directors)
                    {
                        if (!IsRunning(director)) continue;
                        anyRunning = true;

                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                        double from = director.time;
                        FastForward(director);
                        if (director.time - from > EndMarginSeconds)
                            Debug.Log($"[CutsceneSkipper] {director.name}: {from:F1}s -> {director.time:F1}s in {stopwatch.ElapsedMilliseconds} ms");
                    }

                    if (!anyRunning) break;

                    if (Time.realtimeSinceStartup - startTime > MaxSkipRealSeconds)
                    {
                        Debug.LogWarning("[CutsceneSkipper] Skip timed out, the rest of the cutscene plays normally.");
                        break;
                    }

                    // Let the director's own update run: it plays the last frame and
                    // handles the end of the timeline (stopped event, end signals).
                    yield return null;
                }

                // One extra frame so sounds started by end-of-cutscene reactions are caught.
                yield return null;

                // A timeline started on that last frame is part of the same chain.
                foreach (var started in FindPlayingTimelines())
                {
                    if (!directors.Contains(started))
                    {
                        restartForChain = true;
                        break;
                    }
                }

                Debug.Log("[CutsceneSkipper] Skip finished.");
            }
            finally
            {
                IsSkipping = false;
                audio.End();
            }

            if (restartForChain)
                StartCoroutine(SkipRoutine(FindPlayingTimelines()));
        }

        private static string DescribeDirectors(List<PlayableDirector> directors)
        {
            var names = new List<string>();
            foreach (var director in directors)
                names.Add($"{director.name} ({director.time:F1}/{director.duration:F1}s)");
            return string.Join(", ", names);
        }

        private static void FastForward(PlayableDirector director)
        {
            var graph = director.playableGraph;
            var root = graph.GetRootPlayable(0);

            // A timeline dialogue may have paused the timeline (speed 0).
            root.SetSpeed(1);

            // The whole timeline is processed in this frame.
            while (IsRunning(director))
            {
                double remaining = director.duration - director.time;

                // Stop right before the end: the director's own update on the next frame
                // finishes the timeline, so its end behaves exactly as without skipping.
                if (remaining <= EndMarginSeconds) return;

                float step = (float)System.Math.Min(StepSeconds, remaining - EndMarginSeconds);
                double before = director.time;

                try
                {
                    graph.Evaluate(step);
                }
                catch (System.Exception e)
                {
                    // During normal playback Unity logs exceptions from scripts reacting to
                    // the timeline and keeps going; manual evaluation must do the same.
                    Debug.LogException(e);
                }

                if (!graph.IsValid()) return;

                // Reactions during the step can pause the timeline again.
                if (root.GetSpeed() != 1)
                    root.SetSpeed(1);

                // Safety: never spin if time stopped advancing for any reason.
                if (director.time <= before)
                {
                    Debug.LogWarning($"[CutsceneSkipper] {director.name} stopped advancing at {director.time:F2}s, rest plays normally.");
                    return;
                }
            }
        }

        /// <summary>
        /// Mutes audio while skipping and stops every sound that started during the skip.
        /// </summary>
        private class AudioSuppression
        {
            private readonly HashSet<System.IntPtr> _fmodBefore = new HashSet<System.IntPtr>();
            private readonly HashSet<AudioSource> _unityBefore = new HashSet<AudioSource>();
            private float _listenerVolume;
            private FMOD.Studio.Bus _masterBus;
            private bool _busWasMuted;

            public void Begin()
            {
                foreach (var instance in GetFmodInstances())
                    _fmodBefore.Add(instance.handle);

                foreach (var source in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
                {
                    if (source.isPlaying) _unityBefore.Add(source);
                }

                _listenerVolume = AudioListener.volume;
                AudioListener.volume = 0f;

                if (RuntimeManager.IsInitialized)
                {
                    _masterBus = RuntimeManager.GetBus(MasterBusPath);
                    if (_masterBus.isValid())
                    {
                        _masterBus.getMute(out _busWasMuted);
                        _masterBus.setMute(true);
                    }
                }
            }

            public void End()
            {
                foreach (var instance in GetFmodInstances())
                {
                    if (_fmodBefore.Contains(instance.handle)) continue;
                    instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    instance.release();
                }

                foreach (var source in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
                {
                    if (source.isPlaying && !_unityBefore.Contains(source))
                        source.Stop();
                }

                AudioListener.volume = _listenerVolume;

                if (_masterBus.isValid())
                    _masterBus.setMute(_busWasMuted);
            }

            private static List<FMOD.Studio.EventInstance> GetFmodInstances()
            {
                var result = new List<FMOD.Studio.EventInstance>();
                if (!RuntimeManager.IsInitialized) return result;

                if (RuntimeManager.StudioSystem.getBankList(out FMOD.Studio.Bank[] banks) != FMOD.RESULT.OK)
                    return result;

                foreach (var bank in banks)
                {
                    if (bank.getEventList(out FMOD.Studio.EventDescription[] events) != FMOD.RESULT.OK) continue;

                    foreach (var description in events)
                    {
                        if (description.getInstanceList(out FMOD.Studio.EventInstance[] instances) != FMOD.RESULT.OK) continue;
                        result.AddRange(instances);
                    }
                }
                return result;
            }
        }
    }
}
