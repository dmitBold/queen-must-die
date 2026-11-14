using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace QMDA.Audit
{
    /// <summary>Paths used by the audit tools. Reports live OUTSIDE Assets/ so Unity never imports them.</summary>
    public static class AuditPaths
    {
        public static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;
        public static string ReportsDir => Path.Combine(ProjectRoot, "AuditReports");
        public static string ViewpointsFile => Path.Combine(ReportsDir, "viewpoints.json");
        public static string PerfCsv => Path.Combine(ReportsDir, "perf_log.csv");
        public static string PerfCompare => Path.Combine(ReportsDir, "perf_compare.md");

        public static string Ensure()
        {
            Directory.CreateDirectory(ReportsDir);
            return ReportsDir;
        }

        public static string Stamp() => DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);

        public static string Safe(string s)
        {
            if (string.IsNullOrEmpty(s)) return "unnamed";
            foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            return s.Replace(' ', '_');
        }
    }

    [Serializable] public class AuditKv { public string key; public string value; }

    [Serializable]
    public class AuditSection
    {
        public string name;
        public List<AuditKv> items = new List<AuditKv>();
        public void Add(string key, object value)
        {
            items.Add(new AuditKv { key = key, value = Convert.ToString(value, CultureInfo.InvariantCulture) });
        }
    }

    [Serializable] public class AuditRow { public string key; public long count; public string extra; }

    [Serializable]
    public class AuditTable
    {
        public string name;
        public List<AuditRow> rows = new List<AuditRow>();

        public void AddTop(Dictionary<string, long> counts, int n, Func<string, string> extra = null)
        {
            var list = new List<KeyValuePair<string, long>>(counts);
            list.Sort((a, b) => b.Value.CompareTo(a.Value));
            for (int i = 0; i < n && i < list.Count; i++)
                rows.Add(new AuditRow { key = list[i].Key, count = list[i].Value, extra = extra != null ? extra(list[i].Key) : "" });
        }
    }

    [Serializable]
    public class AuditReport
    {
        public string label;
        public string scene;
        public string timeLocal;
        public string unityVersion;
        public List<AuditSection> sections = new List<AuditSection>();
        public List<AuditTable> tables = new List<AuditTable>();

        public AuditSection Sec(string name)
        {
            var s = new AuditSection { name = name };
            sections.Add(s);
            return s;
        }

        public AuditTable Tab(string name)
        {
            var t = new AuditTable { name = name };
            tables.Add(t);
            return t;
        }

        public string ToMarkdown()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Scene audit: " + scene + " [" + label + "]");
            sb.AppendLine();
            sb.AppendLine("- time: " + timeLocal);
            sb.AppendLine("- unity: " + unityVersion);
            sb.AppendLine();
            foreach (var s in sections)
            {
                sb.AppendLine("## " + s.name);
                sb.AppendLine();
                sb.AppendLine("| key | value |");
                sb.AppendLine("|---|---|");
                foreach (var kv in s.items) sb.AppendLine("| " + kv.key + " | " + Esc(kv.value) + " |");
                sb.AppendLine();
            }
            foreach (var t in tables)
            {
                sb.AppendLine("## " + t.name);
                sb.AppendLine();
                sb.AppendLine("| key | count | extra |");
                sb.AppendLine("|---|---:|---|");
                foreach (var r in t.rows) sb.AppendLine("| " + Esc(r.key) + " | " + r.count + " | " + Esc(r.extra) + " |");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public string ToCsv()
        {
            var sb = new StringBuilder("section,key,count_or_value,extra\n");
            foreach (var s in sections)
                foreach (var kv in s.items) sb.Append(Q(s.name)).Append(',').Append(Q(kv.key)).Append(',').Append(Q(kv.value)).Append(",\n");
            foreach (var t in tables)
                foreach (var r in t.rows) sb.Append(Q(t.name)).Append(',').Append(Q(r.key)).Append(',').Append(r.count).Append(',').Append(Q(r.extra)).Append('\n');
            return sb.ToString();
        }

        static string Esc(string s) => string.IsNullOrEmpty(s) ? "" : s.Replace("|", "\\|").Replace("\n", " ");
        static string Q(string s) => "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";
    }
}
