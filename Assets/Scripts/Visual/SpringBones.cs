using System.Collections.Generic;
using UnityEngine;

/// Simple spring-bone ("Dynamic Bone"-like) solver for cloth chains.
/// Every transform whose name starts with `prefix` is simulated; chains are built from parent/child links.
/// Optional capsule colliders (between two transforms) keep the cloth from passing through the body.
[DefaultExecutionOrder(1000)]
public class SpringBones : MonoBehaviour
{
    [System.Serializable]
    public class Capsule
    {
        public Transform a, b;      // capsule axis from a to b
        public Vector3 offsetA, offsetB; // local offsets in a / b space
        public float radius = 0.5f; // in model units (multiplied by this object's scale)
        public Vector3 A => a.TransformPoint(offsetA);
        public Vector3 B => b.TransformPoint(offsetB);
    }

    public string prefix = "Cloth_";
    [Range(0f, 1f)] public float stiffness = 0.15f;   // pull back to rest shape
    [Range(0f, 1f)] public float damping = 0.2f;      // velocity loss per step
    public Vector3 gravity = new Vector3(0, -1.5f, 0);// world units/s^2 (scaled by model size)
    [Range(0f, 1f)] public float stiffnessFalloff = 0.5f; // tips are softer
    public bool scaleByModel = true;
    [Range(0f, 90f)] public float maxAngle = 35f;     // max deviation of each bone from its rest direction relative to the body (not the parent bone)
    public List<Capsule> colliders = new List<Capsule>();

    [System.Serializable]
    public class MobilityOverride { public string prefix; [Range(0f, 1f)] public float mobility = 1f; }
    [Range(0f, 1f)] public float mobility = 0.8f;     // 1 = full swing, lower = calmer cloth
    public List<MobilityOverride> mobilityOverrides = new List<MobilityOverride> {
        new MobilityOverride { prefix = "Cloth_AP", mobility = 1f },   // armpits keep full mobility
        new MobilityOverride { prefix = "Cloth_HD", mobility = 0.3f }, // hood: subtle sway
    };

    class Node
    {
        public Transform t; public Quaternion restLocalRot; public Vector3 localTail; public float len;
        public Vector3 cur, prev; public float stiff; public float[] minDist; public Transform anchor; public Vector3 anchorPos, anchorTail; public float mob = 1f;
    }
    readonly List<List<Node>> chains = new List<List<Node>>();
    float scale = 1f;

    static Vector3 Closest(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a; float l2 = ab.sqrMagnitude;
        if (l2 < 1e-8f) return a;
        return a + ab * Mathf.Clamp01(Vector3.Dot(p - a, ab) / l2);
    }

    void Start()
    {
        if (scaleByModel) scale = transform.lossyScale.y;
        var all = GetComponentsInChildren<Transform>(true);
        foreach (var t in all)
        {
            if (!t.name.StartsWith(prefix)) continue;
            if (t.parent != null && t.parent.name.StartsWith(prefix)) continue; // not a chain root
            var chain = new List<Node>();
            var cur = t;
            while (cur != null)
            {
                Transform child = null;
                foreach (Transform c in cur) if (c.name.StartsWith(prefix)) { child = c; break; }
                var n = new Node { t = cur, restLocalRot = cur.localRotation };
                Vector3 tailWorld = child != null ? child.position : cur.position + (cur.position - cur.parent.position).normalized * (chain.Count > 0 ? chain[chain.Count - 1].len : 1f);
                n.localTail = cur.InverseTransformPoint(tailWorld);
                n.len = Vector3.Distance(cur.position, tailWorld);
                n.cur = n.prev = tailWorld;
                n.anchor = t.parent; // body bone the chain hangs from
                n.mob = mobility;
                foreach (var o in mobilityOverrides) if (o != null && !string.IsNullOrEmpty(o.prefix) && cur.name.StartsWith(o.prefix)) n.mob = o.mobility;
                n.anchorPos = n.anchor.InverseTransformPoint(cur.position);
                n.anchorTail = n.anchor.InverseTransformPoint(tailWorld);
                // colliders never push a point further out than it sits in the rest pose
                n.minDist = new float[colliders.Count];
                for (int i = 0; i < colliders.Count; i++)
                {
                    var c = colliders[i];
                    if (c == null || c.a == null || c.b == null) continue;
                    float d0 = (tailWorld - Closest(tailWorld, c.A, c.B)).magnitude;
                    n.minDist[i] = Mathf.Min(c.radius * scale, d0 * 0.98f);
                }
                chain.Add(n);
                cur = child;
            }
            for (int i = 0; i < chain.Count; i++) chain[i].stiff = Mathf.Lerp(1f, 1f - stiffnessFalloff, chain.Count > 1 ? i / (float)(chain.Count - 1) : 0f);
            chains.Add(chain);
        }
    }

    void LateUpdate()
    {
        float dt = Mathf.Min(Time.deltaTime, 1f / 30f);
        if (dt <= 0f) return;
        foreach (var chain in chains)
        {
            foreach (var n in chain)
            {
                n.t.localRotation = n.restLocalRot;
                Vector3 pos = n.t.position;
                Vector3 restTail = n.t.TransformPoint(n.localTail);
                float worldLen = (restTail - pos).magnitude;
                // rest direction of this bone in body space (independent of how the parent bones are bent)
                Vector3 restDir = (n.anchor.TransformPoint(n.anchorTail) - n.anchor.TransformPoint(n.anchorPos)).normalized;
                Vector3 goal = pos + restDir * worldLen;
                Vector3 vel = (n.cur - n.prev) * (1f - damping);
                Vector3 next = n.cur + vel + gravity * scale * dt * dt;
                next += (goal - n.cur) * stiffness * n.stiff;

                // bend limit (relative to body) + length
                Vector3 dir = next - pos;
                if (dir.sqrMagnitude < 1e-8f) dir = restDir;
                Vector3 d2 = dir.normalized;
                float ang = Vector3.Angle(restDir, d2);
                next = pos + d2 * worldLen;

                // body collision (tail point and bone midpoint), a few iterations
                if (colliders.Count > 0)
                {
                    for (int it = 0; it < 3; it++)
                    {
                        for (int i = 0; i < colliders.Count; i++)
                        {
                            var c = colliders[i];
                            if (c == null || c.a == null || c.b == null) continue;
                            float r = n.minDist[i];
                            if (r <= 0f) continue;
                            Vector3 ca = c.A, cb = c.B;
                            Vector3 q = Closest(next, ca, cb);
                            Vector3 v = next - q; float d = v.magnitude;
                            if (d < r) next = q + (d > 1e-5f ? v / d : restDir) * r;
                            Vector3 mid = (pos + next) * 0.5f;
                            Vector3 qm = Closest(mid, ca, cb);
                            Vector3 vm = mid - qm; float dm = vm.magnitude;
                            if (dm < r) next += (dm > 1e-5f ? vm / dm : restDir) * (r - dm) * 2f;
                        }
                        next = pos + (next - pos).normalized * worldLen;
                    }
                }

                n.prev = n.cur; n.cur = next;
                // mobility: the bone shows only part of the simulated swing (simulation state stays unscaled)
                Vector3 shown = next - pos;
                if (n.mob < 0.999f) shown = Vector3.Slerp(restTail - pos, shown, n.mob);
                n.t.rotation = Quaternion.FromToRotation(restTail - pos, shown) * n.t.rotation;
            }
        }
    }
}
