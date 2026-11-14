using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QMDA.Audit
{
    public static partial class SceneAuditor
    {
        /// <summary>Per-branch breakdown: root objects, and children of every big root (>=500 descendants).</summary>
        static void Rooms(AuditReport rep, Scene scene)
        {
            var cache = new Dictionary<Mesh, long>();
            var rootTab = rep.Tab("Hierarchy: root objects (extra = active | descendants | renderers | tris(all LOD) | lights | LODGroups)");
            var childTab = rep.Tab("Hierarchy: children of big roots (extra = active | descendants | renderers | tris | lights | LODGroups)");

            foreach (var root in scene.GetRootGameObjects())
            {
                int desc = root.GetComponentsInChildren<Transform>(true).Length;
                rootTab.rows.Add(Row(root.transform, root.name, desc, cache));
                if (desc < 500) continue;
                foreach (Transform c in root.transform)
                {
                    int d = c.GetComponentsInChildren<Transform>(true).Length;
                    if (d < 20) continue;
                    childTab.rows.Add(Row(c, root.name + "/" + c.name, d, cache));
                }
            }
            rootTab.rows.Sort((a, b) => b.count.CompareTo(a.count));
            childTab.rows.Sort((a, b) => b.count.CompareTo(a.count));
        }

        static AuditRow Row(Transform t, string name, int desc, Dictionary<Mesh, long> cache)
        {
            long tris = 0;
            var rs = t.GetComponentsInChildren<Renderer>(true);
            foreach (var r in rs) tris += Tris(MeshOf(r), cache);
            int lights = t.GetComponentsInChildren<Light>(true).Length;
            int lods = t.GetComponentsInChildren<LODGroup>(true).Length;
            return new AuditRow
            {
                key = name,
                count = desc,
                extra = (t.gameObject.activeSelf ? "ON" : "OFF") + " | " + desc + " | " + rs.Length + " | " + tris + " | " + lights + " | " + lods
            };
        }
    }
}
