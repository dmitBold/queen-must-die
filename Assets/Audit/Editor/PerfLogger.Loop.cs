using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QMDA.Audit
{
    public static partial class PerfLogger
    {
        static void Update()
        {
            if (Ph == Phase.Idle || !EditorApplication.isPlaying || EditorApplication.isPaused) return;
            if (EditorApplication.isCompiling) return;
            int frame = Time.frameCount;
            if (frame == _lastFrame) return;
            _lastFrame = frame;

            if (_set == null) _set = Viewpoints.Load();
            if (_cam == null)
            {
                _cam = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
                if (_cam == null) return;
            }

            switch (Ph)
            {
                case Phase.Settle:
                    if (++_counter >= SettleFrames) BeginViewpoint();
                    break;
                case Phase.Warmup:
                    if (++_counter >= WarmupN) { _sampler.Reset(); _counter = 0; Ph = Phase.Sampling; }
                    break;
                case Phase.Sampling:
                    _sampler.Sample();
                    if (++_counter >= SampleN) EndViewpoint();
                    break;
            }
        }

        static void BeginViewpoint()
        {
            if (Index >= _set.points.Count) { Finish("done"); return; }
            var vp = _set.points[Index];
            if (_sampler == null) _sampler = new PerfSampler();

            // Cinemachine / player rigs would fight us over the camera transform: switch them off while measuring.
            if (Disabled.Count == 0)
            {
                foreach (var b in _cam.GetComponents<Behaviour>())
                {
                    if (b != null && b.enabled && b.GetType().Name.Contains("Cinemachine")) { b.enabled = false; Disabled.Add(b); }
                }
            }
            _cam.transform.SetPositionAndRotation(vp.position, Quaternion.Euler(vp.euler));
            if (vp.fov > 1f) _cam.fieldOfView = vp.fov;
            _counter = 0;
            Ph = Phase.Warmup;
        }

        static void EndViewpoint()
        {
            var vp = _set.points[Index];
            AuditPaths.Ensure();
            string path = AuditPaths.PerfCsv;
            if (!File.Exists(path)) File.WriteAllText(path, PerfSampler.Header + "\n");
            File.AppendAllText(path, _sampler.Row(Label, SceneManager.GetActiveScene().name, vp.name) + "\n");
            Index++;
            _counter = 0;
            if (Index >= _set.points.Count) Finish("done");
            else BeginViewpoint();
        }

        static void Finish(string why)
        {
            Ph = Phase.Idle;
            if (_sampler != null) { _sampler.Dispose(); _sampler = null; }
            Restore();
            Debug.Log("[Audit] PerfLogger finished (" + why + "): " + AuditPaths.PerfCsv);
            if (ExitAfter && EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
        }
    }
}
