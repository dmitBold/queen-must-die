using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace QMDA.Audit
{
    /// <summary>
    /// Viewpoint seeding with NO rendering and NO colliders (many walls have none, and a render-based search
    /// crashed the Editor with OOM). Candidates are NavMesh points near every enabled light of the room
    /// (+/-3 m offsets), i.e. places where the player can really stand. Eye = navmesh point + 1.6 m.
    /// Score = mean NavMesh.Raycast free distance over 16 directions (cap 20 m) + 0.25 * best direction.
    /// Candidates closer than 0.8 m to a navmesh edge in any direction are rejected (camera would clip a wall).
    /// Look direction = most open direction. Always review the result with ViewpointPreview.Render.
    /// </summary>
    public static class ViewpointNavSeeder
    {
        const int Dirs = 16;
        const float Cap = 20f;
        static readonly string[] Rooms = { "Office", "Laboratory", "Hall", "Bedroom", "Reliquary" };

        [MenuItem("Tools/Audit/Viewpoints/Seed from NavMesh")]
        static void Menu() { Debug.Log("[Audit] " + Seed()); }

        /// <summary>Seeds every room, or only the comma separated names in 'only'.</summary>
        public static string Seed(string only = "")
        {
            var pal = GameObject.Find("Palace");
            if (pal == null) return "Palace not found";
            var filter = string.IsNullOrEmpty(only) ? null : new HashSet<string>(only.Split(','));
            var log = new StringBuilder();
            foreach (var n in Rooms)
            {
                if (filter != null && !filter.Contains(n)) continue;
                var t = pal.transform.Find(n);
                if (t == null || !t.gameObject.activeInHierarchy) { log.AppendLine(n + ": skipped (missing/inactive)"); continue; }
                log.AppendLine(n + ": " + SeedRoom(t));
            }
            return log.ToString();
        }

        static string SeedRoom(Transform room)
        {
            var offs = new[] { Vector3.zero, new Vector3(3, 0, 0), new Vector3(-3, 0, 0), new Vector3(0, 0, 3), new Vector3(0, 0, -3) };
            var seen = new HashSet<Vector3Int>();
            float bestScore = -1f, bestYaw = 0f; Vector3 bestPos = Vector3.zero;
            int tested = 0, noNav = 0, tooClose = 0;

            foreach (var l in room.GetComponentsInChildren<Light>(false))
            {
                if (!l.enabled) continue;
                foreach (var o in offs)
                {
                    if (!NavMesh.SamplePosition(l.transform.position + o, out var nh, 6f, NavMesh.AllAreas)) { noNav++; continue; }
                    var p = nh.position;
                    if (!seen.Add(Vector3Int.RoundToInt(p / 2f))) continue;
                    tested++;

                    var free = new float[Dirs];
                    float sum = 0f, min = float.MaxValue;
                    for (int i = 0; i < Dirs; i++)
                    {
                        var d = Quaternion.Euler(0, i * (360f / Dirs), 0) * Vector3.forward;
                        free[i] = NavMesh.Raycast(p, p + d * Cap, out var hit, NavMesh.AllAreas) ? hit.distance : Cap;
                        sum += free[i];
                        if (free[i] < min) min = free[i];
                    }
                    if (min < 0.8f) { tooClose++; continue; }

                    int bi = 0; float bv = -1f;
                    for (int i = 0; i < Dirs; i++)
                    {
                        float v = free[i] + 0.5f * (free[(i + 1) % Dirs] + free[(i + Dirs - 1) % Dirs]);
                        if (v > bv) { bv = v; bi = i; }
                    }
                    float score = sum / Dirs + 0.25f * free[bi];
                    if (score > bestScore) { bestScore = score; bestPos = p + Vector3.up * 1.6f; bestYaw = bi * (360f / Dirs); }
                }
            }
            string stats = " tested=" + tested + " noNavmesh=" + noNav + " tooCloseToEdge=" + tooClose;
            if (bestScore < 0f) return "no valid candidate -" + stats + " (set the point manually in Scene View)";
            return Viewpoints.Add(room.name, bestPos, new Vector3(5f, bestYaw, 0f), 60f) + " score=" + bestScore.ToString("F1") + stats;
        }
    }
}
