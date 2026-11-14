using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace QMDA.Audit
{
    /// <summary>Builds AuditReports/perf_compare.md: label A -> label B, per viewpoint, from perf_log.csv.</summary>
    public static class PerfCompare
    {
        static readonly string[] Cols = { "fps_avg", "fps_p1low", "frame_ms_avg", "cpu_main_ms", "cpu_render_ms", "gpu_ms", "draw_calls", "batches", "setpass", "triangles", "shadow_casters" };

        public static string Compare(string a, string b)
        {
            string csv = AuditPaths.PerfCsv;
            if (!File.Exists(csv)) return "no perf_log.csv yet";
            var lines = File.ReadAllLines(csv);
            if (lines.Length < 2) return "perf_log.csv is empty";
            var head = lines[0].Split(',');
            var idx = new Dictionary<string, int>();
            for (int i = 0; i < head.Length; i++) idx[head[i]] = i;

            // last row for (label, viewpoint) wins
            var rowsA = new Dictionary<string, string[]>();
            var rowsB = new Dictionary<string, string[]>();
            for (int i = 1; i < lines.Length; i++)
            {
                var f = lines[i].Split(',');
                if (f.Length < head.Length) continue;
                string label = f[idx["label"]], vp = f[idx["viewpoint"]];
                if (label == a) rowsA[vp] = f;
                if (label == b) rowsB[vp] = f;
            }

            var sb = new StringBuilder();
            sb.AppendLine("# Perf compare: `" + a + "` -> `" + b + "`");
            sb.AppendLine();
            sb.AppendLine("Generated " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ". Editor Play Mode numbers, indicative only.");
            sb.AppendLine();
            sb.Append("| viewpoint |");
            foreach (var c in Cols) sb.Append(" " + c + " |");
            sb.AppendLine();
            sb.Append("|---|");
            foreach (var c in Cols) sb.Append("---:|");
            sb.AppendLine();
            foreach (var kv in rowsA)
            {
                if (!rowsB.TryGetValue(kv.Key, out var fb)) continue;
                sb.Append("| " + kv.Key + " |");
                foreach (var c in Cols) sb.Append(" " + Cell(kv.Value[idx[c]], fb[idx[c]]) + " |");
                sb.AppendLine();
            }
            AuditPaths.Ensure();
            File.WriteAllText(AuditPaths.PerfCompare, sb.ToString());
            return AuditPaths.PerfCompare;
        }

        static string Cell(string x, string y)
        {
            if (!double.TryParse(x, NumberStyles.Float, CultureInfo.InvariantCulture, out double a) ||
                !double.TryParse(y, NumberStyles.Float, CultureInfo.InvariantCulture, out double b)) return x + " -> " + y;
            string pct = a != 0 ? " (" + ((b - a) / a * 100).ToString("+0;-0;0", CultureInfo.InvariantCulture) + "%)" : "";
            return a.ToString("0.#", CultureInfo.InvariantCulture) + " -> " + b.ToString("0.#", CultureInfo.InvariantCulture) + pct;
        }
    }
}
