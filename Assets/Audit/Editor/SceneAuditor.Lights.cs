using System.Collections.Generic;
using UnityEngine;

namespace QMDA.Audit
{
    public static partial class SceneAuditor
    {
        static void LightsSection(AuditReport rep)
        {
            var lights = Object.FindObjectsByType<Light>(Inc, NoSort);
            var byMode = new Dictionary<string, long>();
            var rtShadowByRoom = new Dictionary<string, long>();
            var rtByRoom = new Dictionary<string, long>();
            var bakedByRoom = new Dictionary<string, long>();
            int active = 0, rtActive = 0, rtShadowActive = 0, rtHard = 0, rtSoft = 0, bakedActive = 0, mixedActive = 0, dirs = 0;
            int bakedNoEffect = 0, bakedWithEffect = 0, bakedZeroIntensity = 0, bakedWithRenderersGI = 0;

            int lightmapCount = LightmapSettings.lightmaps != null ? LightmapSettings.lightmaps.Length : 0;
            var giRenderers = new List<Renderer>();
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(Inc, NoSort))
                if (r.lightmapIndex >= 0 && r.lightmapIndex < 65534) giRenderers.Add(r);

            foreach (var l in lights)
            {
                bool act = l.enabled && l.gameObject.activeInHierarchy;
                string key = l.type + " / " + l.lightmapBakeType + " / shadows=" + l.shadows + (act ? " / ACTIVE" : " / inactive");
                Inc1(byMode, key);
                if (l.type == LightType.Directional) dirs++;
                string room = HPath(l.transform, 2);
                if (act)
                {
                    active++;
                    if (l.lightmapBakeType == LightmapBakeType.Realtime)
                    {
                        rtActive++;
                        Inc1(rtByRoom, room);
                        if (l.shadows != LightShadows.None)
                        {
                            rtShadowActive++;
                            Inc1(rtShadowByRoom, room);
                            if (l.shadows == LightShadows.Hard) rtHard++; else rtSoft++;
                        }
                    }
                    else if (l.lightmapBakeType == LightmapBakeType.Mixed) mixedActive++;
                    else if (l.lightmapBakeType == LightmapBakeType.Baked)
                    {
                        bakedActive++;
                        Inc1(bakedByRoom, room);
                        // A Baked light only affects the image through lightmaps / probes.
                        bool noLightmaps = lightmapCount == 0;
                        if (l.intensity <= 0.0001f || l.color.maxColorComponent <= 0.0001f) bakedZeroIntensity++;
                        if (noLightmaps) bakedNoEffect++; else bakedWithEffect++;
                    }
                }
            }
            bakedWithRenderersGI = giRenderers.Count;

            var s = rep.Sec("Lights");
            s.Add("Light total", lights.Length);
            s.Add("Light active", active);
            s.Add("Directional (any)", dirs);
            s.Add("Realtime active", rtActive);
            s.Add("Realtime active with shadows", rtShadowActive);
            s.Add("  hard / soft", rtHard + " / " + rtSoft);
            s.Add("Mixed active", mixedActive);
            s.Add("Baked active", bakedActive);
            s.Add("LightmapSettings.lightmaps.Length", lightmapCount);
            s.Add("LightProbes count", LightmapSettings.lightProbes != null ? LightmapSettings.lightProbes.count : 0);
            s.Add("MeshRenderers that have a lightmap index", bakedWithRenderersGI);
            s.Add("Baked active lights with NO lightmaps in scene (effectively zero light)", bakedNoEffect);
            s.Add("Baked active lights when lightmaps exist (may contribute)", bakedWithEffect);
            s.Add("Baked active lights with zero intensity/colour", bakedZeroIntensity);
            s.Add("Realtime light budget note", "URP additional lights per object limit = see Settings section");

            rep.Tab("Lights by type / bake mode / shadows / active").AddTop(byMode, 30);
            rep.Tab("Active realtime lights by room (depth 2)").AddTop(rtByRoom, 20, k => "withShadows=" + (rtShadowByRoom.TryGetValue(k, out var v) ? v : 0));
            rep.Tab("Active BAKED lights by room (depth 2)").AddTop(bakedByRoom, 20);
        }
    }
}
