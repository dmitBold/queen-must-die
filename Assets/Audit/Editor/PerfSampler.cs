using System;
using System.Collections.Generic;
using System.Globalization;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

namespace QMDA.Audit
{
    /// <summary>Collects per-frame render/CPU numbers while a viewpoint is being sampled.</summary>
    public class PerfSampler : IDisposable
    {
        ProfilerRecorder _dc, _batches, _setPass, _tris, _verts, _shadow, _main, _render;
        readonly FrameTiming[] _ft = new FrameTiming[1];
        readonly List<float> _frameMs = new List<float>();
        double _cpuMain, _cpuRender, _gpu, _dcSum, _batSum, _spSum, _triSum, _vertSum, _shSum;
        int _n;

        public PerfSampler()
        {
            _dc = Rec(Unity.Profiling.ProfilerCategory.Render, "Draw Calls Count");
            _batches = Rec(Unity.Profiling.ProfilerCategory.Render, "Batches Count");
            _setPass = Rec(Unity.Profiling.ProfilerCategory.Render, "SetPass Calls Count");
            _tris = Rec(Unity.Profiling.ProfilerCategory.Render, "Triangles Count");
            _verts = Rec(Unity.Profiling.ProfilerCategory.Render, "Vertices Count");
            _shadow = Rec(Unity.Profiling.ProfilerCategory.Render, "Shadow Casters Count");
            _main = Rec(Unity.Profiling.ProfilerCategory.Internal, "Main Thread");
            _render = Rec(Unity.Profiling.ProfilerCategory.Internal, "Render Thread");
        }

        static ProfilerRecorder Rec(ProfilerCategory c, string n) => ProfilerRecorder.StartNew(c, n, 1);

        static double V(ProfilerRecorder r) => r.Valid ? r.LastValue : 0;

        public void Reset()
        {
            _frameMs.Clear();
            _cpuMain = _cpuRender = _gpu = _dcSum = _batSum = _spSum = _triSum = _vertSum = _shSum = 0;
            _n = 0;
        }

        /// <summary>Call once per rendered frame.</summary>
        public void Sample()
        {
            _frameMs.Add(Time.unscaledDeltaTime * 1000f);
            FrameTimingManager.CaptureFrameTimings();
            double cm = 0, cr = 0, g = 0;
            if (FrameTimingManager.GetLatestTimings(1, _ft) > 0)
            {
                cm = _ft[0].cpuMainThreadFrameTime;
                cr = _ft[0].cpuRenderThreadFrameTime;
                g = _ft[0].gpuFrameTime;
            }
            if (cm <= 0.001) cm = V(_main) / 1e6;
            if (cr <= 0.001) cr = V(_render) / 1e6;
            _cpuMain += cm; _cpuRender += cr; _gpu += g;
            _dcSum += V(_dc); _batSum += V(_batches); _spSum += V(_setPass);
            _triSum += V(_tris); _vertSum += V(_verts); _shSum += V(_shadow);
            _n++;
        }

        public int Count => _n;

        public static string Header =>
            "time,label,scene,quality,viewpoint,frames,fps_avg,fps_min,fps_p1low,frame_ms_avg,cpu_main_ms,cpu_render_ms,gpu_ms,draw_calls,batches,setpass,triangles,vertices,shadow_casters";

        public string Row(string label, string scene, string viewpoint)
        {
            var sorted = new List<float>(_frameMs);
            sorted.Sort();
            double avgMs = 0;
            foreach (var m in sorted) avgMs += m;
            avgMs = sorted.Count > 0 ? avgMs / sorted.Count : 0;
            double worstMs = sorted.Count > 0 ? sorted[sorted.Count - 1] : 0;
            // 1% low: average of the slowest 1% of frames, expressed as fps
            int k = Math.Max(1, sorted.Count / 100);
            double low = 0;
            for (int i = 0; i < k && i < sorted.Count; i++) low += sorted[sorted.Count - 1 - i];
            low = low / k;
            double d = Math.Max(1, _n);
            var ci = CultureInfo.InvariantCulture;
            string F(double v, string f = "F2") => v.ToString(f, ci);
            return string.Join(",", new[]
            {
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", ci), label, scene,
                QualitySettings.names[QualitySettings.GetQualityLevel()], viewpoint, _n.ToString(ci),
                F(avgMs > 0 ? 1000.0 / avgMs : 0, "F1"), F(worstMs > 0 ? 1000.0 / worstMs : 0, "F1"), F(low > 0 ? 1000.0 / low : 0, "F1"),
                F(avgMs), F(_cpuMain / d), F(_cpuRender / d), F(_gpu / d),
                F(_dcSum / d, "F0"), F(_batSum / d, "F0"), F(_spSum / d, "F0"), F(_triSum / d, "F0"), F(_vertSum / d, "F0"), F(_shSum / d, "F0")
            });
        }

        public void Dispose()
        {
            _dc.Dispose(); _batches.Dispose(); _setPass.Dispose(); _tris.Dispose();
            _verts.Dispose(); _shadow.Dispose(); _main.Dispose(); _render.Dispose();
        }
    }
}
