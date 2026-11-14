using System.Collections.Generic;
using UnityEngine;

namespace QMDA.Audit
{
    public static partial class SceneAuditor
    {
        // LOD membership computed once, shared by several sections
        static HashSet<Renderer> _lod0, _lodOther;

        static void BuildLodSets()
        {
            _lod0 = new HashSet<Renderer>();
            _lodOther = new HashSet<Renderer>();
            foreach (var g in Object.FindObjectsByType<LODGroup>(Inc, NoSort))
            {
                var l = g.GetLODs();
                for (int i = 0; i < l.Length; i++)
                {
                    if (l[i].renderers == null) continue;
                    foreach (var r in l[i].renderers)
                        if (r != null) (i == 0 ? _lod0 : _lodOther).Add(r);
                }
            }
        }

        static void MeshStats(AuditReport rep)
        {
            BuildLodSets();
            var cache = new Dictionary<Mesh, long>();
            var mrs = Object.FindObjectsByType<MeshRenderer>(Inc, NoSort);
            var smrs = Object.FindObjectsByType<SkinnedMeshRenderer>(Inc, NoSort);

            long trisAll = 0, trisActive = 0, trisNear = 0, trisSkinned = 0;
            int active = 0, noMesh = 0, multiMat = 0, inLod = 0, instOn = 0, slots = 0;
            var meshes = new Dictionary<string, long>();
            var meshBase = new Dictionary<string, long>();
            var meshHeavy = new Dictionary<string, long>();
            var mats = new Dictionary<string, long>();
            var shaders = new Dictionary<string, long>();
            var pairs = new Dictionary<string, long>();
            var matInfo = new Dictionary<string, string>();
            var outsideLod = new Dictionary<string, long>();

            foreach (var m in mrs)
            {
                var sm = m.sharedMaterials;
                if (sm.Length > 1) multiMat++;
                var mesh = MeshOf(m);
                if (mesh == null) noMesh++;
                long t = Tris(mesh, cache);
                trisAll += t;
                bool act = IsActive(m);
                if (act)
                {
                    active++;
                    trisActive += t;
                    if (!(_lodOther.Contains(m) && !_lod0.Contains(m))) trisNear += t;
                }
                if (_lod0.Contains(m) || _lodOther.Contains(m)) inLod++;
                else if (mesh != null) Inc1(outsideLod, LodSuffix.Replace(mesh.name, ""));

                if (mesh != null)
                {
                    Inc1(meshes, mesh.name);
                    Inc1(meshBase, LodSuffix.Replace(mesh.name, ""));
                    Inc1(meshHeavy, mesh.name, t);
                }
                foreach (var mat in sm)
                {
                    if (mat == null) continue;
                    slots++;
                    if (mat.enableInstancing) instOn++;
                    string sh = mat.shader != null ? mat.shader.name : "null";
                    Inc1(mats, mat.name);
                    Inc1(shaders, sh);
                    matInfo[mat.name] = sh + (mat.enableInstancing ? " | inst" : "") + " | q" + mat.renderQueue;
                    if (mesh != null) Inc1(pairs, mesh.name + " + " + mat.name);
                }
            }

            foreach (var s in smrs)
            {
                trisSkinned += Tris(s.sharedMesh, cache);
                if (IsActive(s)) active++;
                foreach (var mat in s.sharedMaterials)
                {
                    if (mat == null) continue;
                    slots++;
                    Inc1(mats, mat.name);
                    if (mat.shader != null) Inc1(shaders, mat.shader.name);
                }
            }

            var ov = rep.Sec("Overview");
            ov.Add("GameObjects (Transforms, incl. inactive)", Object.FindObjectsByType<Transform>(Inc, NoSort).Length);
            ov.Add("MeshRenderer total", mrs.Length);
            ov.Add("Renderers active (Mesh + Skinned)", active);
            ov.Add("SkinnedMeshRenderer total", smrs.Length);
            ov.Add("MeshRenderer without mesh", noMesh);
            ov.Add("MeshRenderer with >1 material", multiMat);
            ov.Add("Material slots", slots);
            ov.Add("Material slots with enableInstancing", instOn);
            ov.Add("Unique meshes", meshes.Count);
            ov.Add("Unique mesh base names (no _LODn)", meshBase.Count);
            ov.Add("Unique materials (by name)", mats.Count);
            ov.Add("Unique shaders", shaders.Count);
            ov.Add("Triangles: all MeshRenderers, all LODs, incl. inactive", trisAll);
            ov.Add("Triangles: active MeshRenderers, all LODs", trisActive);
            ov.Add("Triangles: active, LOD0 + non-LOD only (upper bound)", trisNear);
            ov.Add("Triangles: SkinnedMeshRenderer (all)", trisSkinned);
            ov.Add("Colliders / MeshColliders", Object.FindObjectsByType<Collider>(Inc, NoSort).Length + " / " + Object.FindObjectsByType<MeshCollider>(Inc, NoSort).Length);

            var lod = rep.Sec("LOD coverage");
            lod.Add("MeshRenderers inside a LODGroup", inLod);
            lod.Add("MeshRenderers outside any LODGroup", mrs.Length - inLod);

            rep.Tab("Top 15 meshes by instance count").AddTop(meshes, 15);
            rep.Tab("Top 15 mesh base names (LOD suffix stripped)").AddTop(meshBase, 15);
            rep.Tab("Top 15 meshes by total triangles (tris x instances)").AddTop(meshHeavy, 15);
            rep.Tab("Top 15 materials by slot count").AddTop(mats, 15, k => matInfo.TryGetValue(k, out var s) ? s : "");
            rep.Tab("Top 15 mesh+material pairs (merge/instancing candidates)").AddTop(pairs, 15);
            rep.Tab("Shaders by slot count").AddTop(shaders, 15);
            rep.Tab("Top 20 mesh base names OUTSIDE any LODGroup").AddTop(outsideLod, 20);
        }
    }
}
