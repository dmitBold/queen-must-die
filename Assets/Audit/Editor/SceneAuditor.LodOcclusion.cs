using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace QMDA.Audit
{
    public static partial class SceneAuditor
    {
        static void LodAndOcclusion(AuditReport rep)
        {
            var groups = Object.FindObjectsByType<LODGroup>(Inc, NoSort);
            var byCount = new Dictionary<string, long>();
            var fade = new Dictionary<string, long>();
            var lastThr = new Dictionary<string, long>();
            var firstThr = new Dictionary<string, long>();
            int active = 0, lastAbove2 = 0, culledAtEnd = 0, noRenderersInLast = 0;

            foreach (var g in groups)
            {
                if (g.gameObject.activeInHierarchy) active++;
                var l = g.GetLODs();
                Inc1(byCount, l.Length + " levels");
                Inc1(fade, g.fadeMode + " / animate=" + g.animateCrossFading);
                if (l.Length == 0) continue;
                float last = l[l.Length - 1].screenRelativeTransitionHeight;
                if (last > 0.02f) lastAbove2++;
                if (last <= 0.0001f) culledAtEnd++;
                Inc1(lastThr, Bucket(last));
                Inc1(firstThr, Bucket(l[0].screenRelativeTransitionHeight));
                var lastR = l[l.Length - 1].renderers;
                if (lastR == null || lastR.Length == 0) noRenderersInLast++;
            }

            var s = rep.Sec("LOD groups");
            s.Add("LODGroup total", groups.Length);
            s.Add("LODGroup active in hierarchy", active);
            s.Add("Last LOD threshold > 2% screen height (object not culled until large)", lastAbove2);
            s.Add("Last LOD threshold ~0 (never culled by LOD)", culledAtEnd);
            s.Add("Last LOD has no renderers (= culled state)", noRenderersInLast);
            s.Add("QualitySettings.lodBias", QualitySettings.lodBias);
            s.Add("QualitySettings.maximumLODLevel", QualitySettings.maximumLODLevel);
            rep.Tab("LODGroup by level count").AddTop(byCount, 10);
            rep.Tab("LODGroup fade mode").AddTop(fade, 10);
            rep.Tab("LOD0 -> LOD1 threshold (screen height)").AddTop(firstThr, 10);
            rep.Tab("Last LOD threshold (screen height)").AddTop(lastThr, 10);

            var o = rep.Sec("Occlusion culling");
            var cams = Object.FindObjectsByType<Camera>(Inc, NoSort);
            foreach (var c in cams)
                o.Add("Camera '" + c.name + "'", "useOcclusionCulling=" + c.useOcclusionCulling + " far=" + c.farClipPlane + " near=" + c.nearClipPlane + " enabled=" + c.enabled);
            o.Add("Baked Umbra data size (bytes)", StaticOcclusionCulling.umbraDataSize);
            o.Add("OcclusionArea", Object.FindObjectsByType<OcclusionArea>(Inc, NoSort).Length);
            o.Add("OcclusionPortal", Object.FindObjectsByType<OcclusionPortal>(Inc, NoSort).Length);
        }

        static string Bucket(float v)
        {
            if (v <= 0.0001f) return "0";
            if (v < 0.005f) return "<0.5%";
            if (v < 0.01f) return "0.5-1%";
            if (v < 0.02f) return "1-2%";
            if (v < 0.05f) return "2-5%";
            if (v < 0.15f) return "5-15%";
            if (v < 0.4f) return "15-40%";
            return ">=40%";
        }
    }
}
