using System;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QMDA.Audit
{
    public static partial class PerfLogger
    {
        /// <summary>Edit Mode snapshot from the Game view stats (UnityStats). Appends one row to perf_log.csv.</summary>
        public static string Snapshot(string label)
        {
            AuditPaths.Ensure();
            string path = AuditPaths.PerfCsv;
            if (!File.Exists(path)) File.WriteAllText(path, PerfSampler.Header + "\n");
            var ci = CultureInfo.InvariantCulture;
            string row = string.Join(",", new[]
            {
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", ci), Sanitize(label), SceneManager.GetActiveScene().name,
                QualitySettings.names[QualitySettings.GetQualityLevel()], "snapshot(edit)", "1",
                "", "", "", (UnityStats.frameTime * 1000f).ToString("F2", ci), "", (UnityStats.renderTime * 1000f).ToString("F2", ci), "",
                UnityStats.drawCalls.ToString(ci), UnityStats.batches.ToString(ci), UnityStats.setPassCalls.ToString(ci),
                UnityStats.triangles.ToString(ci), UnityStats.vertices.ToString(ci), UnityStats.shadowCasters.ToString(ci)
            });
            File.AppendAllText(path, row + "\n");
            return row;
        }

        [MenuItem("Tools/Audit/Perf/Run baseline in Play Mode")]
        static void MenuRun() { Debug.Log("[Audit] " + Start("baseline")); }

        [MenuItem("Tools/Audit/Perf/Edit Mode snapshot")]
        static void MenuSnap() { Debug.Log("[Audit] " + Snapshot("snapshot")); }
    }
}
