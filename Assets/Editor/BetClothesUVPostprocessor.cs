using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

// Bet's new (Meshy) clothes texture is baked into a re-packed UV layout (UV_PACK in Bet_Rigged.blend):
// the same UV islands as the original clothes UV, but without overlapping islands.
// The Bet FBX files still carry the old clothes UV, so this postprocessor swaps uv0 of the clothes mesh
// for the packed layout on import, using the lookup exported from Blender.
public class BetClothesUVPostprocessor : AssetPostprocessor
{
    public const string LookupPath = "Assets/Visual/3D models/Bet/NewTextures/Bet_Clothes_UVPack.txt";

    static readonly HashSet<string> Models = new HashSet<string>
    {
        "Bet.fbx", "Bet on throne.fbx", "Bet on throne OLD.fbx", "Bet on throne 2.fbx",
        "Bet_docs.fbx", "Bet_walking.fbx", "Bet_execution.fbx",
        "Bet floor captured.fbx", "Bet wall captured_right .fbx"
    };

    struct Entry { public Vector3 pos; public Vector2 oldUv, newUv; }

    const float Q = 20000f;

    void OnPostprocessModel(GameObject root)
    {
        if (!Models.Contains(Path.GetFileName(assetPath))) return;
        if (!File.Exists(LookupPath)) { Debug.LogWarning("[BetClothesUV] lookup not found: " + LookupPath); return; }
        var entries = Load();
        var grid = new Dictionary<long, List<int>>();
        for (int i = 0; i < entries.Count; i++)
        {
            long k = Key(Mathf.RoundToInt(entries[i].oldUv.x * Q), Mathf.RoundToInt(entries[i].oldUv.y * Q));
            if (!grid.TryGetValue(k, out var l)) grid[k] = l = new List<int>();
            l.Add(i);
        }

        var done = new HashSet<Mesh>();
        foreach (var r in root.GetComponentsInChildren<Renderer>(true))
        {
            Mesh mesh = r is SkinnedMeshRenderer s ? s.sharedMesh : r.GetComponent<MeshFilter>()?.sharedMesh;
            if (mesh == null || done.Contains(mesh)) continue;
            string n = (mesh.name + " " + r.name).ToUpperInvariant();
            if (!n.Contains("CLOTH")) continue;
            done.Add(mesh);
            Apply(mesh, entries, grid);
        }
        if (done.Count == 0) Debug.LogWarning("[BetClothesUV] " + assetPath + ": clothes mesh not found");
    }

    static long Key(int x, int y) => ((long)x << 32) ^ (uint)y;

    static List<Entry> Load()
    {
        var list = new List<Entry>();
        foreach (var line in File.ReadAllLines(LookupPath))
        {
            var p = line.Split(' ');
            if (p.Length < 7) continue;
            float f(int i) => float.Parse(p[i], CultureInfo.InvariantCulture);
            list.Add(new Entry { pos = new Vector3(f(0), f(1), f(2)), oldUv = new Vector2(f(3), f(4)), newUv = new Vector2(f(5), f(6)) });
        }
        return list;
    }

    static List<int> Candidates(Vector2 uv, List<Entry> e, Dictionary<long, List<int>> grid)
    {
        var res = new List<int>();
        int x = Mathf.RoundToInt(uv.x * Q), y = Mathf.RoundToInt(uv.y * Q);
        for (int dx = -2; dx <= 2; dx++)
            for (int dy = -2; dy <= 2; dy++)
                if (grid.TryGetValue(Key(x + dx, y + dy), out var l))
                    foreach (var i in l)
                        if ((e[i].oldUv - uv).sqrMagnitude < 1e-8f) res.Add(i);
        return res;
    }

    void Apply(Mesh mesh, List<Entry> e, Dictionary<long, List<int>> grid)
    {
        var verts = mesh.vertices;
        var uvs = mesh.uv;
        if (uvs == null || uvs.Length != verts.Length) { Debug.LogWarning("[BetClothesUV] " + assetPath + ": no uv0"); return; }

        var cands = new List<int>[verts.Length];
        // Fit an affine transform (Unity mesh space -> Blender object space) from vertices whose old UV is unique.
        var M = new double[4, 4]; var B = new double[3, 4]; int pairs = 0;
        for (int v = 0; v < verts.Length; v++)
        {
            var c = cands[v] = Candidates(uvs[v], e, grid);
            if (c.Count == 0) continue;
            bool unique = true;
            foreach (var i in c) if ((e[i].pos - e[c[0]].pos).sqrMagnitude > 1e-10f) { unique = false; break; }
            if (!unique) continue;
            double[] x = { verts[v].x, verts[v].y, verts[v].z, 1 };
            var y = e[c[0]].pos;
            for (int a = 0; a < 4; a++)
            {
                for (int b = 0; b < 4; b++) M[a, b] += x[a] * x[b];
                B[0, a] += x[a] * y.x; B[1, a] += x[a] * y.y; B[2, a] += x[a] * y.z;
            }
            pairs++;
        }
        var A = new double[3][];
        for (int k = 0; k < 3; k++) A[k] = Solve(M, new[] { B[k, 0], B[k, 1], B[k, 2], B[k, 3] });
        if (A[0] == null) { Debug.LogWarning("[BetClothesUV] " + assetPath + ": could not fit transform"); return; }
        Vector3 T(Vector3 p) => new Vector3(
            (float)(A[0][0] * p.x + A[0][1] * p.y + A[0][2] * p.z + A[0][3]),
            (float)(A[1][0] * p.x + A[1][1] * p.y + A[1][2] * p.z + A[1][3]),
            (float)(A[2][0] * p.x + A[2][1] * p.y + A[2][2] * p.z + A[2][3]));

        var newUv = new Vector2[verts.Length];
        int byUv = 0, byPos = 0, fallback = 0; double err = 0, maxErr = 0;
        for (int v = 0; v < verts.Length; v++)
        {
            var p = T(verts[v]);
            var c = cands[v];
            int best = -1; float bd = float.MaxValue;
            IEnumerable<int> pool = c.Count > 0 ? (IEnumerable<int>)c : AllIdx(e.Count);
            foreach (var i in pool) { float d = (e[i].pos - p).sqrMagnitude; if (d < bd) { bd = d; best = i; } }
            newUv[v] = e[best].newUv;
            double dist = System.Math.Sqrt(bd); err += dist; if (dist > maxErr) maxErr = dist;
            if (c.Count == 0) fallback++; else if (c.Count == 1) byUv++; else byPos++;
        }
        mesh.uv = newUv;
        Debug.Log($"[BetClothesUV] {assetPath}: '{mesh.name}' {verts.Length} verts remapped (uv {byUv}, uv+pos {byPos}, fallback {fallback}), fit pairs {pairs}, mean pos err {err / verts.Length:F5}, max {maxErr:F5}");
    }

    static IEnumerable<int> AllIdx(int n) { for (int i = 0; i < n; i++) yield return i; }

    static double[] Solve(double[,] m, double[] b)
    {
        int n = 4; var a = new double[n, n + 1];
        for (int i = 0; i < n; i++) { for (int j = 0; j < n; j++) a[i, j] = m[i, j]; a[i, n] = b[i]; }
        for (int c = 0; c < n; c++)
        {
            int piv = c;
            for (int r = c + 1; r < n; r++) if (System.Math.Abs(a[r, c]) > System.Math.Abs(a[piv, c])) piv = r;
            if (System.Math.Abs(a[piv, c]) < 1e-12) return null;
            for (int j = 0; j <= n; j++) { var t = a[c, j]; a[c, j] = a[piv, j]; a[piv, j] = t; }
            for (int r = 0; r < n; r++)
            {
                if (r == c) continue;
                double f = a[r, c] / a[c, c];
                for (int j = c; j <= n; j++) a[r, j] -= f * a[c, j];
            }
        }
        var x = new double[n];
        for (int i = 0; i < n; i++) x[i] = a[i, n] / a[i, i];
        return x;
    }
}
