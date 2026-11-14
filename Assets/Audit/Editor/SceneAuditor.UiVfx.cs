using System.Collections.Generic;
using UnityEngine;

namespace QMDA.Audit
{
    public static partial class SceneAuditor
    {
        static void UiAndVfx(AuditReport rep)
        {
            var canvases = Object.FindObjectsByType<Canvas>(Inc, NoSort);
            var byMode = new Dictionary<string, long>();
            int canvasActive = 0;
            foreach (var c in canvases)
            {
                bool act = c.enabled && c.gameObject.activeInHierarchy;
                if (act) canvasActive++;
                Inc1(byMode, c.renderMode + (act ? " / ACTIVE" : " / inactive"));
            }

            var ps = Object.FindObjectsByType<ParticleSystem>(Inc, NoSort);
            int psActiveGo = 0, psPlaying = 0, psLooping = 0, psPlayOnAwake = 0;
            long maxParticles = 0;
            foreach (var p in ps)
            {
                if (!p.gameObject.activeInHierarchy) continue;
                psActiveGo++;
                if (p.isPlaying) psPlaying++;
                var main = p.main;
                if (main.loop) psLooping++;
                if (main.playOnAwake) psPlayOnAwake++;
                maxParticles += main.maxParticles;
            }

            var s = rep.Sec("UI / VFX / misc");
            s.Add("Canvas total / active", canvases.Length + " / " + canvasActive);
            s.Add("ParticleSystem total", ps.Length);
            s.Add("ParticleSystem on active GameObjects", psActiveGo);
            s.Add("  playing right now (Edit Mode: usually 0)", psPlaying);
            s.Add("  looping / playOnAwake (active ones)", psLooping + " / " + psPlayOnAwake);
            s.Add("  sum of maxParticles (active ones)", maxParticles);
            s.Add("ReflectionProbe", Object.FindObjectsByType<ReflectionProbe>(Inc, NoSort).Length);
            s.Add("LightProbeGroup", Object.FindObjectsByType<LightProbeGroup>(Inc, NoSort).Length);
            s.Add("Camera", Object.FindObjectsByType<Camera>(Inc, NoSort).Length);
            s.Add("Application.isPlaying", Application.isPlaying);
            rep.Tab("Canvas by render mode").AddTop(byMode, 10);
        }
    }
}
