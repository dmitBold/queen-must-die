using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace QMDA.Audit
{
    /// <summary>
    /// Optimization step 1: Cast Shadows = Off for small decor (candles, torches, books, paper, anything under 0.6 m).
    /// Only enabled MeshRenderers on active objects are touched. SkinnedMeshRenderers and architecture are left alone.
    /// No Static flags, no URP asset, no lodBias. The list of changed renderers is written to
    /// AuditReports/step1_shadow_off_list.csv (scene path + size + name), so the change can be reviewed and reverted.
    ///
    /// CLI:  unity command script eval --code "return QMDA.Audit.ShadowStep1.Apply(true);"   (dry run)
    ///       unity command script eval --code "return QMDA.Audit.ShadowStep1.Apply(false);"  (apply + save scene)
    /// </summary>
    public static class ShadowStep1
    {
        const float SmallSize = 0.6f;
        const float NamedMaxSize = 4f;
        static readonly Regex NamedDecor = new Regex("Candle|Torch|Book|Paper", RegexOptions.IgnoreCase);

        [MenuItem("Tools/Audit/Step1/Dry run (cast shadows off for small decor)")]
        static void MenuDry() { Debug.Log("[Audit] " + Apply(true)); }

        static bool Matches(Renderer r)
        {
            var b = r.bounds.size;
            float m = Mathf.Max(b.x, Mathf.Max(b.y, b.z));
            if (m < SmallSize) return true;
            return m < NamedMaxSize && NamedDecor.IsMatch(r.name);
        }

        static string PathOf(Transform t)
        {
            var sb = new StringBuilder(t.name);
            for (var p = t.parent; p != null; p = p.parent) sb.Insert(0, p.name + "/");
            return sb.ToString();
        }

        public static string Apply(bool dryRun)
        {
            if (EditorApplication.isPlaying) return "exit Play Mode first";
            var all = Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var csv = new StringBuilder("path,name,max_size_m,was_static\n");
            int changed = 0, activeCasting = 0;
            var byTop = new SortedDictionary<string, int>();
            foreach (var r in all)
            {
                if (!r.enabled || !r.gameObject.activeInHierarchy) continue;
                if (r.shadowCastingMode == ShadowCastingMode.Off) continue;
                activeCasting++;
                if (!Matches(r)) continue;
                var b = r.bounds.size;
                float m = Mathf.Max(b.x, Mathf.Max(b.y, b.z));
                csv.Append('"').Append(PathOf(r.transform).Replace("\"", "\"\"")).Append("\",\"")
                   .Append(r.name.Replace("\"", "\"\"")).Append("\",")
                   .Append(m.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)).Append(',')
                   .Append(r.gameObject.isStatic).Append('\n');
                string top = r.transform.root.name;
                var chain = r.transform; while (chain.parent != null && chain.parent.parent != null) chain = chain.parent;
                top = chain.name;
                byTop.TryGetValue(top, out int c); byTop[top] = c + 1;
                changed++;
                if (dryRun) continue;
                Undo.RecordObject(r, "Step1 cast shadows off");
                r.shadowCastingMode = ShadowCastingMode.Off;
                EditorUtility.SetDirty(r);
                PrefabUtility.RecordPrefabInstancePropertyModifications(r);
            }
            AuditPaths.Ensure();
            string list = Path.Combine(AuditPaths.ReportsDir, dryRun ? "step1_shadow_off_list_dryrun.csv" : "step1_shadow_off_list.csv");
            File.WriteAllText(list, csv.ToString());
            var sum = new StringBuilder();
            foreach (var kv in byTop) sum.Append(kv.Key).Append('=').Append(kv.Value).Append(' ');
            string res = (dryRun ? "DRY RUN: would change " : "changed ") + changed + " of " + activeCasting + " active MeshRenderers casting shadows. By top group: " + sum + " list=" + list;
            if (!dryRun && changed > 0)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(scene);
                res += " | saved=" + EditorSceneManager.SaveScene(scene);
            }
            return res;
        }
    }
}
