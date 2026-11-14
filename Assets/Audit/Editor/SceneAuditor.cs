using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QMDA.Audit
{
    /// <summary>
    /// Read-only scene statistics. Uses sharedMesh / sharedMaterials only, never touches .material,
    /// never marks the scene dirty. Output goes to AuditReports/ (outside Assets/).
    /// CLI: unity command script eval --code "return QMDA.Audit.SceneAuditor.Run(\"baseline\");"
    /// </summary>
    public static partial class SceneAuditor
    {
        static readonly Regex LodSuffix = new Regex(@"_?LOD\d+$", RegexOptions.IgnoreCase);
        const FindObjectsInactive Inc = FindObjectsInactive.Include;
        const FindObjectsSortMode NoSort = FindObjectsSortMode.None;

        [MenuItem("Tools/Audit/Scene Report (baseline)")]
        public static void RunMenu() { Debug.Log("[Audit] report: " + Run("baseline")); }

        /// <summary>Builds the report and returns the path of the .md file.</summary>
        public static string Run(string label)
        {
            var scene = SceneManager.GetActiveScene();
            var rep = new AuditReport
            {
                label = label,
                scene = scene.name,
                timeLocal = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                unityVersion = Application.unityVersion
            };

            MeshStats(rep);
            StaticAndShadows(rep);
            LightsSection(rep);
            LodAndOcclusion(rep);
            Rooms(rep, scene);
            UiAndVfx(rep);
            SettingsSection(rep);

            string dir = AuditPaths.Ensure();
            string baseName = "Scene_" + AuditPaths.Safe(scene.name) + "_" + AuditPaths.Safe(label) + "_" + AuditPaths.Stamp();
            string md = Path.Combine(dir, baseName + ".md");
            File.WriteAllText(md, rep.ToMarkdown());
            File.WriteAllText(Path.Combine(dir, baseName + ".json"), JsonUtility.ToJson(rep, true));
            File.WriteAllText(Path.Combine(dir, baseName + ".csv"), rep.ToCsv());
            return md;
        }

        // ---------- helpers ----------

        static long Tris(Mesh m, Dictionary<Mesh, long> cache)
        {
            if (m == null) return 0;
            if (cache.TryGetValue(m, out long v)) return v;
            long t = 0;
            for (int i = 0; i < m.subMeshCount; i++)
                if (m.GetTopology(i) == MeshTopology.Triangles) t += m.GetIndexCount(i) / 3;
            cache[m] = t;
            return t;
        }

        static void Inc1(Dictionary<string, long> d, string k, long n = 1)
        {
            d.TryGetValue(k, out long v);
            d[k] = v + n;
        }

        static bool IsActive(Renderer r) => r != null && r.enabled && r.gameObject.activeInHierarchy;

        static Mesh MeshOf(Renderer r)
        {
            if (r is SkinnedMeshRenderer s) return s.sharedMesh;
            var mf = r.GetComponent<MeshFilter>();
            return mf != null ? mf.sharedMesh : null;
        }

        static string Kind(string n)
        {
            string s = n.ToLowerInvariant();
            if (s.Contains("book")) return "book";
            if (s.Contains("paper") || s.Contains("note") || s.Contains("scroll")) return "paper";
            if (s.Contains("candle") || s.Contains("torch")) return "candle/torch";
            if (s.Contains("wall")) return "wall";
            if (s.Contains("floor")) return "floor";
            if (s.Contains("ceil") || s.Contains("roof")) return "ceiling";
            if (s.Contains("pillar") || s.Contains("column")) return "pillar";
            if (s.Contains("door")) return "door";
            if (s.Contains("trim") || s.Contains("border")) return "trim";
            if (s.Contains("statue")) return "statue";
            if (s.Contains("prop")) return "prop";
            return "other";
        }

        static string HPath(Transform t, int depth)
        {
            // hierarchy path truncated to the first `depth` levels
            var parts = new List<string>();
            for (var c = t; c != null; c = c.parent) parts.Add(c.name);
            parts.Reverse();
            if (parts.Count > depth) parts = parts.GetRange(0, depth);
            return string.Join("/", parts);
        }
    }
}
