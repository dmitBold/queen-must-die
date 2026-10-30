using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// The baked marble of the King2 and King3 statues lives in a new packed UV layout (UV_MARBLE in
// Blender/King_Statues_Marble.blend), because their original UVs overlap / leave the 0-1 square.
// On import this postprocessor finds, for every triangle of these statue meshes, the matching triangle of the
// .blend (by original UVs, then position) and puts the new UVs in uv0, splitting vertices at the new seams.
public class StatueMarbleUVPostprocessor : AssetPostprocessor
{
    const string Dir = "Assets/Visual/3D models/STATUES/";
    static readonly Dictionary<string, string> Models = new Dictionary<string, string>
    {
        { Dir + "King2.fbx", Dir + "MarbleBaked/King2_UVMarble_Tris.txt" },
        { Dir + "King3.fbx", Dir + "MarbleBaked/King3_UVMarble_Tris.txt" },
    };
    string LookupPath => Models[assetPath];

    class Lookup
    {
        public Vector3[] p;   // 3 per triangle, Blender world space (rest pose)
        public Vector2[] uv;  // 3 per triangle, new UVs
        public Vector3[] c;   // centroid per triangle
        public Vector2[] ouv; // 3 per triangle, original UVs
        public string[] part; // Blender object per triangle
        public HashSet<string> parts = new HashSet<string>();
        public Dictionary<Vector2Int, List<int>> ogrid = new Dictionary<Vector2Int, List<int>>();
        public const float OCell = 0.004f;
        public static Vector2Int OK(Vector2 v) => new Vector2Int(Mathf.FloorToInt(v.x / OCell), Mathf.FloorToInt(v.y / OCell));
        public Dictionary<Vector3Int, List<int>> grid = new Dictionary<Vector3Int, List<int>>();
        public Dictionary<Vector3Int, List<int>> vgrid = new Dictionary<Vector3Int, List<int>>();
        public Vector3 min, max;
        public const float Cell = 0.06f;
        public static Vector3Int K(Vector3 v) => new Vector3Int(Mathf.FloorToInt(v.x / Cell), Mathf.FloorToInt(v.y / Cell), Mathf.FloorToInt(v.z / Cell));
    }

    static Lookup LoadLookup(string LookupPath)
    {
        var lines = File.ReadAllLines(LookupPath);
        int T = lines.Length;
        var L = new Lookup { p = new Vector3[T * 3], uv = new Vector2[T * 3], ouv = new Vector2[T * 3], c = new Vector3[T], part = new string[T] };
        L.min = Vector3.one * float.MaxValue; L.max = -L.min;
        for (int t = 0; t < T; t++)
        {
            var tab = lines[t].Split('\t');
            L.part[t] = tab[0]; L.parts.Add(tab[0]);
            var s = tab[1].Split(' ');
            float f(int i) => float.Parse(s[i], CultureInfo.InvariantCulture);
            for (int k = 0; k < 3; k++)
            {
                var p = new Vector3(f(k * 7), f(k * 7 + 1), f(k * 7 + 2));
                L.p[t * 3 + k] = p; L.uv[t * 3 + k] = new Vector2(f(k * 7 + 3), f(k * 7 + 4)); L.ouv[t * 3 + k] = new Vector2(f(k * 7 + 5), f(k * 7 + 6));
                L.min = Vector3.Min(L.min, p); L.max = Vector3.Max(L.max, p);
                Add(L.vgrid, Lookup.K(p), t * 3 + k);
            }
            L.c[t] = (L.p[t * 3] + L.p[t * 3 + 1] + L.p[t * 3 + 2]) / 3f;
            Add(L.grid, Lookup.K(L.c[t]), t);
            var oc = Lookup.OK((L.ouv[t * 3] + L.ouv[t * 3 + 1] + L.ouv[t * 3 + 2]) / 3f);
            if (!L.ogrid.TryGetValue(oc, out var ol)) L.ogrid[oc] = ol = new List<int>();
            ol.Add(t);
        }
        return L;
    }

    static void Add(Dictionary<Vector3Int, List<int>> g, Vector3Int k, int i)
    {
        if (!g.TryGetValue(k, out var l)) g[k] = l = new List<int>();
        l.Add(i);
    }

    static float NearestVertex(Lookup L, Vector3 q)
    {
        var k = Lookup.K(q); float best = float.MaxValue;
        for (int r = 0; r <= 3 && best == float.MaxValue; r++)
            for (int x = -r; x <= r; x++) for (int y = -r; y <= r; y++) for (int z = -r; z <= r; z++)
                    if (L.vgrid.TryGetValue(new Vector3Int(k.x + x, k.y + y, k.z + z), out var l))
                        foreach (var i in l) best = Mathf.Min(best, (L.p[i] - q).sqrMagnitude);
        return best == float.MaxValue ? 1f : Mathf.Sqrt(best);
    }

    static Vector3 NearestPoint(Lookup L, Vector3 q)
    {
        var k = Lookup.K(q); float best = float.MaxValue; Vector3 bp = q;
        for (int r = 0; r <= 3 && best == float.MaxValue; r++)
            for (int x = -r; x <= r; x++) for (int y = -r; y <= r; y++) for (int z = -r; z <= r; z++)
                    if (L.vgrid.TryGetValue(new Vector3Int(k.x + x, k.y + y, k.z + z), out var l))
                        foreach (var i in l) { float d = (L.p[i] - q).sqrMagnitude; if (d < best) { best = d; bp = L.p[i]; } }
        return bp;
    }

    static Matrix4x4? FitAffine(List<Vector3> src, List<Vector3> dst)
    {
        if (src.Count < 8) return null;
        var M = new double[4, 4]; var B = new double[3, 4];
        for (int i = 0; i < src.Count; i++)
        {
            double[] x = { src[i].x, src[i].y, src[i].z, 1 };
            for (int a = 0; a < 4; a++) { for (int b = 0; b < 4; b++) M[a, b] += x[a] * x[b]; B[0, a] += x[a] * dst[i].x; B[1, a] += x[a] * dst[i].y; B[2, a] += x[a] * dst[i].z; }
        }
        var R = Matrix4x4.identity;
        for (int k = 0; k < 3; k++)
        {
            var sol = Solve(M, new[] { B[k, 0], B[k, 1], B[k, 2], B[k, 3] });
            if (sol == null) return null;
            for (int c = 0; c < 4; c++) R[k, c] = (float)sol[c];
        }
        return R;
    }

    static double[] Solve(double[,] m, double[] b)
    {
        int n = 4; var a = new double[n, n + 1];
        for (int i = 0; i < n; i++) { for (int j = 0; j < n; j++) a[i, j] = m[i, j]; a[i, n] = b[i]; }
        for (int c = 0; c < n; c++)
        {
            int piv = c; for (int r = c + 1; r < n; r++) if (System.Math.Abs(a[r, c]) > System.Math.Abs(a[piv, c])) piv = r;
            if (System.Math.Abs(a[piv, c]) < 1e-12) return null;
            for (int j = 0; j <= n; j++) { var t = a[c, j]; a[c, j] = a[piv, j]; a[piv, j] = t; }
            for (int r = 0; r < n; r++) { if (r == c) continue; double f = a[r, c] / a[c, c]; for (int j = c; j <= n; j++) a[r, j] -= f * a[c, j]; }
        }
        var x = new double[n]; for (int i = 0; i < n; i++) x[i] = a[i, n] / a[i, i]; return x;
    }

    // the meshes are rebuilt (vertices split at new seams), which only works on readable meshes
    void OnPreprocessModel()
    {
        if (Models.ContainsKey(assetPath) && assetImporter is ModelImporter mi) mi.isReadable = true;
    }

    void OnPostprocessModel(GameObject root)
    {
        if (!Models.ContainsKey(assetPath)) return;
        if (!File.Exists(LookupPath)) { Debug.LogWarning("[StatueUV] lookup not found: " + LookupPath); return; }
        var L = LoadLookup(LookupPath);

        var rends = root.GetComponentsInChildren<Renderer>(true).ToList();
        var items = new List<(Renderer r, Mesh m, Matrix4x4 toModel)>();
        foreach (var r in rends)
        {
            var mesh = r is SkinnedMeshRenderer s ? s.sharedMesh : r.GetComponent<MeshFilter>()?.sharedMesh;
            if (mesh == null || items.Any(i => i.m == mesh)) continue;
            items.Add((r, mesh, root.transform.worldToLocalMatrix * r.transform.localToWorldMatrix));
        }
        if (items.Count == 0) { Debug.LogWarning("[StatueUV] " + assetPath + ": no meshes"); return; }

        // model space -> Blender space: best signed axis permutation, per-axis scale and offset from bounding boxes
        var pts = new List<Vector3>();
        foreach (var it in items) foreach (var v in it.m.vertices) pts.Add(it.toModel.MultiplyPoint3x4(v));
        Vector3 umin = pts.Aggregate(Vector3.one * float.MaxValue, Vector3.Min), umax = pts.Aggregate(-Vector3.one * float.MaxValue, Vector3.Max);
        var sample = Enumerable.Range(0, 300).Select(i => pts[(int)((long)i * pts.Count / 300)]).ToList();
        Matrix4x4 best = Matrix4x4.identity; float bestErr = float.MaxValue;
        int[][] perms = { new[] { 0, 1, 2 }, new[] { 0, 2, 1 }, new[] { 1, 0, 2 }, new[] { 1, 2, 0 }, new[] { 2, 0, 1 }, new[] { 2, 1, 0 } };
        foreach (var pm in perms)
            for (int sg = 0; sg < 8; sg++)
            {
                var M = Matrix4x4.zero;
                for (int a = 0; a < 3; a++)
                {
                    int src = pm[a]; float sign = ((sg >> a) & 1) == 1 ? -1 : 1;
                    float us = umax[src] - umin[src], bs = L.max[a] - L.min[a];
                    float scale = us > 1e-6f ? bs / us : 1f;
                    float uc = (umax[src] + umin[src]) / 2, bc = (L.max[a] + L.min[a]) / 2;
                    M[a, src] = sign * scale; M[a, 3] = bc - sign * scale * uc;
                }
                M[3, 3] = 1;
                float err = 0; foreach (var q in sample) err += NearestVertex(L, M.MultiplyPoint3x4(q));
                err /= sample.Count;
                if (err < bestErr) { bestErr = err; best = M; }
            }
        // refine with a few rounds of trimmed least-squares (affine) on nearest-vertex pairs
        var dense = Enumerable.Range(0, 2500).Select(i => pts[(int)((long)i * pts.Count / 2500)]).ToList();
        for (int iter = 0; iter < 12; iter++)
        {
            var pairs = dense.Select(q => { var b = best.MultiplyPoint3x4(q); var nb = NearestPoint(L, b); return (q, nb, d: (nb - b).magnitude); }).ToList();
            var med = pairs.Select(x => x.d).OrderBy(x => x).ElementAt(pairs.Count / 2);
            var use = pairs.Where(x => x.d <= Mathf.Max(med * 2.5f, 0.01f)).ToList();
            var A = FitAffine(use.Select(x => x.q).ToList(), use.Select(x => x.nb).ToList());
            if (A.HasValue) best = A.Value;
        }
        float finalErr = dense.Average(q => NearestVertex(L, best.MultiplyPoint3x4(q)));
        Debug.Log($"[StatueUV] {assetPath}: {items.Count} meshes, alignment error {bestErr:F4} -> {finalErr:F4}");

        foreach (var it in items)
        {
            // per-mesh translation refinement (parts of older model versions sit slightly elsewhere)
            var M = best * it.toModel; var vs = it.m.vertices;
            var sm = Enumerable.Range(0, Mathf.Min(400, vs.Length)).Select(i => vs[(int)((long)i * vs.Length / Mathf.Min(400, vs.Length))]).ToList();
            float before = sm.Average(v => NearestVertex(L, M.MultiplyPoint3x4(v)));
            var Mt = M;
            for (int iter = 0; iter < 8; iter++)
            {
                var off = Vector3.zero; int c = 0;
                foreach (var v in sm) { var b = Mt.MultiplyPoint3x4(v); var nb = NearestPoint(L, b); if ((nb - b).sqrMagnitude < 0.09f) { off += nb - b; c++; } }
                if (c == 0) break;
                Mt = Matrix4x4.Translate(off / c) * Mt;
            }
            float after = sm.Average(v => NearestVertex(L, Mt.MultiplyPoint3x4(v)));
            Remap(it.m, after < before ? Mt : M, L, it.r.name);
        }
    }

    void Remap(Mesh mesh, Matrix4x4 toBlender, Lookup L, string rname)
    {
        var verts = mesh.vertices; var bl = verts.Select(v => toBlender.MultiplyPoint3x4(v)).ToArray();
        var normals = mesh.normals; var tangents = mesh.tangents; var colors = mesh.colors;
        var uv1 = new List<Vector2>(); mesh.GetUVs(1, uv1);
        var ouv0 = mesh.uv; bool hasOld = ouv0 != null && ouv0.Length == verts.Length;
        string part = L.parts.Contains(rname) ? rname : (L.parts.Contains(mesh.name) ? mesh.name : null);
        int byUv = 0, byPos = 0;
        var bw = mesh.boneWeights; var bind = mesh.bindposes;
        int sub = mesh.subMeshCount;
        var newIndex = new Dictionary<(int, int, int), int>();
        var src = new List<int>(); var nuv = new List<Vector2>();
        var tris = new List<int>[sub];
        double errSum = 0; float errMax = 0; int n = 0;
        for (int s = 0; s < sub; s++)
        {
            var t = mesh.GetTriangles(s); tris[s] = new List<int>(t.Length);
            for (int i = 0; i < t.Length; i += 3)
            {
                Vector3 a = bl[t[i]], b = bl[t[i + 1]], c = bl[t[i + 2]];
                int bestT = -1; float bestD = float.MaxValue; bool uvMatch = false;
                // 1) by original UVs (pose independent), position only breaks ties between identical UV triangles
                if (hasOld)
                {
                    Vector2 oa = ouv0[t[i]], ob = ouv0[t[i + 1]], oc = ouv0[t[i + 2]];
                    var ok = Lookup.OK((oa + ob + oc) / 3f);
                    float bu = float.MaxValue;
                    for (int x = -1; x <= 1; x++) for (int y = -1; y <= 1; y++)
                            if (L.ogrid.TryGetValue(new Vector2Int(ok.x + x, ok.y + y), out var ol))
                                foreach (var ti in ol)
                                {
                                    if (part != null && L.part[ti] != part) continue;
                                    float du = 0;
                                    foreach (var q in new[] { oa, ob, oc })
                                        du += Mathf.Min((L.ouv[ti * 3] - q).sqrMagnitude, Mathf.Min((L.ouv[ti * 3 + 1] - q).sqrMagnitude, (L.ouv[ti * 3 + 2] - q).sqrMagnitude));
                                    if (du > 3e-6f) continue;
                                    float dp = ((a + b + c) / 3f - L.c[ti]).sqrMagnitude;
                                    float score = du * 1e4f + dp;
                                    if (score < bu) { bu = score; bestT = ti; }
                                }
                    uvMatch = bestT >= 0;
                }
                // 2) by position
                var ctr = (a + b + c) / 3f; var k = Lookup.K(ctr);
                if (!uvMatch)
                    for (int r = 0; r <= 2 && bestT < 0; r++)
                        for (int x = -r; x <= r; x++) for (int y = -r; y <= r; y++) for (int z = -r; z <= r; z++)
                                if (L.grid.TryGetValue(new Vector3Int(k.x + x, k.y + y, k.z + z), out var l))
                                    foreach (var ti in l)
                                    {
                                        float d = 0;
                                        foreach (var q in new[] { a, b, c })
                                            d += Mathf.Min((L.p[ti * 3] - q).sqrMagnitude, Mathf.Min((L.p[ti * 3 + 1] - q).sqrMagnitude, (L.p[ti * 3 + 2] - q).sqrMagnitude));
                                        if (d < bestD) { bestD = d; bestT = ti; }
                                    }
                if (uvMatch) byUv++; else byPos++;
                for (int j = 0; j < 3; j++)
                {
                    int vi = t[i + j]; Vector2 u = Vector2.zero;
                    if (bestT >= 0)
                    {
                        int bk = 0; float bd = float.MaxValue;
                        if (uvMatch) { var q = ouv0[vi]; for (int m = 0; m < 3; m++) { float d = (L.ouv[bestT * 3 + m] - q).sqrMagnitude; if (d < bd) { bd = d; bk = m; } } bd = (L.p[bestT * 3 + bk] - bl[vi]).sqrMagnitude; }
                        else { var q = bl[vi]; for (int m = 0; m < 3; m++) { float d = (L.p[bestT * 3 + m] - q).sqrMagnitude; if (d < bd) { bd = d; bk = m; } } }
                        u = L.uv[bestT * 3 + bk];
                        float e = Mathf.Sqrt(bd); errSum += e; errMax = Mathf.Max(errMax, e); n++;
                    }
                    var key = (vi, Mathf.RoundToInt(u.x * 65536), Mathf.RoundToInt(u.y * 65536));
                    if (!newIndex.TryGetValue(key, out int ni)) { ni = src.Count; newIndex[key] = ni; src.Add(vi); nuv.Add(u); }
                    tris[s].Add(ni);
                }
            }
        }
        T[] Pick<T>(T[] arr) => arr != null && arr.Length == verts.Length ? src.Select(i => arr[i]).ToArray() : null;
        var shapes = new List<(string name, float w, Vector3[] dv, Vector3[] dn, Vector3[] dt)>();
        for (int s = 0; s < mesh.blendShapeCount; s++)
            for (int f = 0; f < mesh.GetBlendShapeFrameCount(s); f++)
            {
                var dv = new Vector3[verts.Length]; var dn = new Vector3[verts.Length]; var dt = new Vector3[verts.Length];
                mesh.GetBlendShapeFrameVertices(s, f, dv, dn, dt);
                shapes.Add((mesh.GetBlendShapeName(s), mesh.GetBlendShapeFrameWeight(s, f), Pick(dv), Pick(dn), Pick(dt)));
            }
        mesh.Clear();
        if (src.Count > 65535) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = Pick(verts);
        if (Pick(normals) is Vector3[] nn) mesh.normals = nn;
        if (Pick(tangents) is Vector4[] tt) mesh.tangents = tt;
        if (Pick(colors) is Color[] cc) mesh.colors = cc;
        mesh.SetUVs(0, nuv);
        if (uv1.Count == verts.Length) mesh.SetUVs(1, src.Select(i => uv1[i]).ToList());
        if (Pick(bw) is BoneWeight[] ww) mesh.boneWeights = ww;
        mesh.bindposes = bind;
        mesh.subMeshCount = sub;
        for (int s = 0; s < sub; s++) mesh.SetTriangles(tris[s], s);
        foreach (var sh in shapes) mesh.AddBlendShapeFrame(sh.name, sh.w, sh.dv, sh.dn, sh.dt);
        mesh.RecalculateBounds();
        mesh.RecalculateTangents(); // tangents must follow the new UVs for the normal map
        Debug.Log($"[StatueUV] {assetPath} '{mesh.name}': {verts.Length} -> {src.Count} verts (mesh now v{mesh.vertexCount}, idx {mesh.GetIndexCount(0)}, readable {mesh.isReadable}), tris by uv {byUv} / by position {byPos}, corner error mean {(n > 0 ? errSum / n : 0):F4} max {errMax:F4}");
    }
}
