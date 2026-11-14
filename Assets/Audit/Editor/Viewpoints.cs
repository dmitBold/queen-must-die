using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace QMDA.Audit
{
    [Serializable]
    public class Viewpoint
    {
        public string name;
        public Vector3 position;
        public Vector3 euler;
        public float fov;
    }

    [Serializable]
    public class ViewpointSet
    {
        public List<Viewpoint> points = new List<Viewpoint>();
    }

    /// <summary>
    /// Fixed camera positions used for repeatable perf measurements.
    /// Stored in AuditReports/viewpoints.json (committed, so before/after runs use identical spots).
    /// </summary>
    public static class Viewpoints
    {
        public static ViewpointSet Load()
        {
            string p = AuditPaths.ViewpointsFile;
            if (!File.Exists(p)) return new ViewpointSet();
            var set = JsonUtility.FromJson<ViewpointSet>(File.ReadAllText(p));
            return set ?? new ViewpointSet();
        }

        public static void Save(ViewpointSet set)
        {
            AuditPaths.Ensure();
            File.WriteAllText(AuditPaths.ViewpointsFile, JsonUtility.ToJson(set, true));
        }

        /// <summary>Adds or replaces a viewpoint by name. Callable from CLI (script eval).</summary>
        public static string Add(string name, Vector3 pos, Vector3 euler, float fov = 60f)
        {
            var set = Load();
            set.points.RemoveAll(v => v.name == name);
            set.points.Add(new Viewpoint { name = name, position = pos, euler = euler, fov = fov });
            Save(set);
            return "saved '" + name + "' at " + pos + " (total " + set.points.Count + ")";
        }

        public static string AddFromSceneView(string name)
        {
            var sv = SceneView.lastActiveSceneView;
            if (sv == null) return "no active Scene View";
            var t = sv.camera.transform;
            return Add(name, t.position, t.eulerAngles, 60f);
        }

        /// <summary>Same, but from the Main Camera (handy in Play Mode via CLI).</summary>
        public static string AddFromMainCamera(string name)
        {
            var c = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (c == null) return "no camera";
            return Add(name, c.transform.position, c.transform.eulerAngles, c.fieldOfView);
        }

        public static string Remove(string name)
        {
            var set = Load();
            int n = set.points.RemoveAll(v => v.name == name);
            Save(set);
            return "removed " + n;
        }

        public static string List()
        {
            var set = Load();
            var sb = new System.Text.StringBuilder();
            foreach (var v in set.points) sb.AppendLine(v.name + " " + v.position + " " + v.euler + " fov=" + v.fov);
            return sb.Length == 0 ? "(none)" : sb.ToString();
        }

        [MenuItem("Tools/Audit/Viewpoints/Add from Scene View")]
        static void MenuAdd()
        {
            string name = "VP_" + (Load().points.Count + 1);
            Debug.Log("[Audit] " + AddFromSceneView(name));
        }

        [MenuItem("Tools/Audit/Viewpoints/Reveal file")]
        static void MenuReveal()
        {
            AuditPaths.Ensure();
            EditorUtility.RevealInFinder(File.Exists(AuditPaths.ViewpointsFile) ? AuditPaths.ViewpointsFile : AuditPaths.ReportsDir);
        }
    }
}
