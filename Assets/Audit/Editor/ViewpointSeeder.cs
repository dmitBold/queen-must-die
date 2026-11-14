using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace QMDA.Audit
{
    /// <summary>
    /// Proposes starting viewpoints automatically, one per main Palace room.
    /// Candidates: points under every enabled light of the room (plus 3 m offsets), eye = floor + 1.6 m.
    /// A candidate is valid only if: not inside a collider, 2.2 m head clearance, and a ceiling within 25 m
    /// (rejects roofs / outside slabs). Score = mean free sight distance over 16 directions (capped at 15 m).
    /// Look direction = the most open direction. The result is a STARTING POINT: check the previews
    /// (ViewpointPreview.Render) and re-save any bad point manually.
    /// </summary>
    public static class ViewpointSeeder
    {
        static readonly string[] Rooms = { "Office", "Laboratory", "Hall", "Bedroom", "Reliquary" };
        const int Dirs = 16;
        const float Cap = 15f;

        [MenuItem("Tools/Audit/Viewpoints/Seed from rooms")]
        static void Menu() { Debug.Log("[Audit] " + Seed()); }

        /// <summary>Seeds every room, or only the given names (comma separated) when 'only' is not empty.</summary>
        public static string Seed(string only = "")
        {
            var log = new StringBuilder();
            var pal = GameObject.Find("Palace");
            if (pal == null) return "Palace not found";
            var filter = string.IsNullOrEmpty(only) ? null : new HashSet<string>(only.Split(','));
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
            float bestScore = -1f; Vector3 bestPos = Vector3.zero; float bestYaw = 0f; int tested = 0;

            foreach (var l in room.GetComponentsInChildren<Light>(false))
            {
                if (!l.enabled) continue;
                foreach (var o in offs)
                {
                    var from = l.transform.position + o;
                    if (!Physics.Raycast(from, Vector3.down, out var h, 25f, ~0, QueryTriggerInteraction.Ignore)) continue;
                    var eye = h.point + Vector3.up * 1.6f;
                    var key = Vector3Int.RoundToInt(eye / 1.5f);
                    if (!seen.Add(key)) continue;
                    tested++;
                    if (Physics.CheckSphere(eye, 0.35f, ~0, QueryTriggerInteraction.Ignore)) continue;
                    if (Physics.Raycast(eye, Vector3.up, 2.2f, ~0, QueryTriggerInteraction.Ignore)) continue;
                    if (!Physics.Raycast(eye, Vector3.up, 25f, ~0, QueryTriggerInteraction.Ignore)) continue; // no ceiling = roof/outside

                    var free = new float[Dirs];
                    float sum = 0f;
                    for (int i = 0; i < Dirs; i++)
                    {
                        var d = Quaternion.Euler(0, i * (360f / Dirs), 0) * Vector3.forward;
                        free[i] = Physics.Raycast(eye, d, out var hh, Cap, ~0, QueryTriggerInteraction.Ignore) ? hh.distance : Cap;
                        sum += free[i];
                    }
                    int bi = 0; float bv = -1f;
                    for (int i = 0; i < Dirs; i++)
                    {
                        float v = free[i] + 0.5f * (free[(i + 1) % Dirs] + free[(i + Dirs - 1) % Dirs]);
                        if (v > bv) { bv = v; bi = i; }
                    }
                    float score = sum / Dirs + 0.25f * free[bi];
                    if (score > bestScore) { bestScore = score; bestPos = eye; bestYaw = bi * (360f / Dirs); }
                }
            }
            if (bestScore < 0f) return "no valid candidate (tested " + tested + ") - set manually";
            return Viewpoints.Add(room.name, bestPos, new Vector3(5f, bestYaw, 0f), 60f) + " score=" + bestScore.ToString("F1") + " tested=" + tested;
        }
    }
}
