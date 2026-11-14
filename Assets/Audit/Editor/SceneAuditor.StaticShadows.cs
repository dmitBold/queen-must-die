using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace QMDA.Audit
{
    public static partial class SceneAuditor
    {
        static void StaticAndShadows(AuditReport rep)
        {
            var mrs = Object.FindObjectsByType<MeshRenderer>(Inc, NoSort);
            int stAny = 0, stBatch = 0, stOccluder = 0, stOccludee = 0, stGI = 0, stReflect = 0, stNav = 0;
            int shOn = 0, shOff = 0, shTwo = 0, shOnly = 0, small = 0, smallAct = 0;
            var totalByKind = new Dictionary<string, long>();
            var staticByKind = new Dictionary<string, long>();
            var shadowByKind = new Dictionary<string, long>();
            var smallByKind = new Dictionary<string, long>();
            var layers = new Dictionary<string, long>();

            foreach (var m in mrs)
            {
                var go = m.gameObject;
                var f = GameObjectUtility.GetStaticEditorFlags(go);
                string kind = Kind(go.name);
                Inc1(totalByKind, kind);
                if (f != 0) { stAny++; Inc1(staticByKind, kind); }
                if ((f & StaticEditorFlags.BatchingStatic) != 0) stBatch++;
                if ((f & StaticEditorFlags.OccluderStatic) != 0) stOccluder++;
                if ((f & StaticEditorFlags.OccludeeStatic) != 0) stOccludee++;
                if ((f & StaticEditorFlags.ContributeGI) != 0) stGI++;
                if ((f & StaticEditorFlags.ReflectionProbeStatic) != 0) stReflect++;
                Inc1(layers, LayerMask.LayerToName(go.layer) + " (" + go.layer + ")");

                var mode = m.shadowCastingMode;
                if (mode == ShadowCastingMode.Off) shOff++;
                else
                {
                    if (mode == ShadowCastingMode.On) shOn++;
                    else if (mode == ShadowCastingMode.TwoSided) shTwo++;
                    else shOnly++;
                    Inc1(shadowByKind, kind);
                    var e = m.bounds.extents;
                    if (Mathf.Max(e.x, Mathf.Max(e.y, e.z)) < 0.25f)
                    {
                        small++;
                        Inc1(smallByKind, kind);
                        if (IsActive(m)) smallAct++;
                    }
                }
            }

            var st = rep.Sec("Static flags (MeshRenderer only)");
            st.Add("Any static flag", stAny + " of " + mrs.Length);
            st.Add("Static share, %", mrs.Length == 0 ? "0" : (100.0 * stAny / mrs.Length).ToString("F1"));
            st.Add("Batching Static", stBatch);
            st.Add("Occluder Static", stOccluder);
            st.Add("Occludee Static", stOccludee);
            st.Add("Contribute GI", stGI);
            st.Add("Reflection Probe Static", stReflect);

            var sh = rep.Sec("Shadow casting (MeshRenderer)");
            sh.Add("On", shOn);
            sh.Add("Two Sided", shTwo);
            sh.Add("Shadows Only", shOnly);
            sh.Add("Off", shOff);
            sh.Add("Small casters (max bounds extent < 0.25 m, cast shadows)", small);
            sh.Add("  of them active now", smallAct);

            rep.Tab("Renderer kind (name heuristic): total, extra = static count").AddTop(totalByKind, 20,
                k => "static=" + (staticByKind.TryGetValue(k, out var v) ? v : 0) + " casting=" + (shadowByKind.TryGetValue(k, out var c) ? c : 0));
            rep.Tab("Small shadow casters by kind").AddTop(smallByKind, 20);
            rep.Tab("MeshRenderers by layer").AddTop(layers, 20);
        }
    }
}
