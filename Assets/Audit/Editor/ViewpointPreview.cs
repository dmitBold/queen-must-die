using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace QMDA.Audit
{
    /// <summary>
    /// Renders a PNG per saved viewpoint with a temporary hidden camera (scene is not modified, not marked dirty).
    /// Output: AuditReports/preview/VP_<name>.png. Used to sanity-check viewpoints before measuring.
    /// </summary>
    public static class ViewpointPreview
    {
        [MenuItem("Tools/Audit/Viewpoints/Render previews")]
        static void Menu() { Debug.Log("[Audit] " + Render()); }

        /// <summary>Renders every viewpoint, or only the comma separated names in 'only' (each Render is a full URP frame, keep it small).</summary>
        public static string Render(int width = 960, int height = 540, string only = "")
        {
            var set = Viewpoints.Load();
            var filter = string.IsNullOrEmpty(only) ? null : new System.Collections.Generic.HashSet<string>(only.Split(','));
            string dir = Path.Combine(AuditPaths.Ensure(), "preview");
            Directory.CreateDirectory(dir);

            var go = new GameObject("__AuditPreviewCam") { hideFlags = HideFlags.HideAndDontSave };
            var cam = go.AddComponent<Camera>();
            cam.enabled = false;
            cam.allowHDR = true;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 500f;
            cam.clearFlags = CameraClearFlags.Skybox;
            var rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            var log = new StringBuilder();
            var prev = RenderTexture.active;
            try
            {
                foreach (var v in set.points)
                {
                    if (filter != null && !filter.Contains(v.name)) continue;
                    cam.transform.SetPositionAndRotation(v.position, Quaternion.Euler(v.euler));
                    cam.fieldOfView = v.fov > 1f ? v.fov : 60f;
                    cam.targetTexture = rt;
                    cam.Render();
                    RenderTexture.active = rt;
                    tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    tex.Apply();
                    string file = Path.Combine(dir, "VP_" + AuditPaths.Safe(v.name) + ".png");
                    File.WriteAllBytes(file, tex.EncodeToPNG());
                    log.AppendLine(file);
                }
            }
            finally
            {
                RenderTexture.active = prev;
                cam.targetTexture = null;
                RenderTexture.ReleaseTemporary(rt);
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(go);
            }
            return log.ToString();
        }
    }
}
