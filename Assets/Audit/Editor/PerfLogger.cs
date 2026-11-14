using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace QMDA.Audit
{
    /// <summary>
    /// Play Mode perf logger. For every saved viewpoint: teleport the camera, warm up, sample N frames,
    /// append one row to AuditReports/perf_log.csv. State survives domain reload via SessionState.
    ///
    /// CLI:  unity command script eval --code "return QMDA.Audit.PerfLogger.Start(\"baseline\", 120, 300, true);"
    ///       unity command script eval --code "return QMDA.Audit.PerfLogger.Status();"
    ///       unity command script eval --code "return QMDA.Audit.PerfLogger.Snapshot(\"baseline_snapshot\");"
    ///       unity command script eval --code "return QMDA.Audit.PerfCompare.Compare(\"baseline\", \"step2\");"
    /// NOTE: Editor numbers (GPU time in particular) are only indicative. The Game view must be visible
    /// for the camera to actually render in the Editor.
    /// </summary>
    [InitializeOnLoad]
    public static partial class PerfLogger
    {
        const string K = "QMDA.Audit.Perf.";
        const int SettleFrames = 240; // after entering Play Mode, let Zenject / scene init finish

        enum Phase { Idle = 0, Settle = 1, Warmup = 2, Sampling = 3 }

        static PerfSampler _sampler;
        static int _lastFrame = -1;
        static int _counter;
        static ViewpointSet _set;
        static Camera _cam;
        static readonly List<Behaviour> Disabled = new List<Behaviour>();

        static PerfLogger()
        {
            EditorApplication.update += Update;
            EditorApplication.playModeStateChanged += OnPlayMode;
        }

        static Phase Ph { get => (Phase)SessionState.GetInt(K + "phase", 0); set => SessionState.SetInt(K + "phase", (int)value); }
        static string Label => SessionState.GetString(K + "label", "run");
        static int WarmupN => SessionState.GetInt(K + "warmup", 120);
        static int SampleN => SessionState.GetInt(K + "sample", 300);
        static int Index { get => SessionState.GetInt(K + "index", 0); set => SessionState.SetInt(K + "index", value); }
        static bool ExitAfter => SessionState.GetBool(K + "exit", true);

        static string Sanitize(string s) => (s ?? "run").Replace(",", "_").Replace("\n", " ").Trim();

        public static string Start(string label, int warmupFrames = 120, int sampleFrames = 300, bool exitPlayModeAfter = true)
        {
            var set = Viewpoints.Load();
            if (set.points.Count == 0) return "no viewpoints saved (use QMDA.Audit.Viewpoints.Add)";
            SessionState.SetString(K + "label", Sanitize(label));
            SessionState.SetInt(K + "warmup", warmupFrames);
            SessionState.SetInt(K + "sample", sampleFrames);
            SessionState.SetBool(K + "exit", exitPlayModeAfter);
            Index = 0;
            Ph = Phase.Settle;
            _counter = 0;
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.EnterPlaymode();
                return "entering Play Mode; " + set.points.Count + " viewpoints; label=" + Sanitize(label);
            }
            return "already in Play Mode, started; " + set.points.Count + " viewpoints";
        }

        public static string Status() =>
            "phase=" + Ph + " label=" + Label + " viewpointIndex=" + Index + " counter=" + _counter + " playing=" + EditorApplication.isPlaying;

        public static string Stop()
        {
            Finish("stopped by user");
            return "stopped";
        }

        static void OnPlayMode(PlayModeStateChange c)
        {
            if (c == PlayModeStateChange.ExitingPlayMode) Restore();
            if (c == PlayModeStateChange.EnteredEditMode)
            {
                Restore();
                Ph = Phase.Idle;
            }
            if (c == PlayModeStateChange.EnteredPlayMode && Ph != Phase.Idle)
            {
                _counter = 0; _lastFrame = -1; _set = null; _cam = null;
            }
        }

        static void Restore()
        {
            foreach (var b in Disabled) if (b != null) b.enabled = true;
            Disabled.Clear();
        }
    }
}
