using System.Collections.Generic;
using UnityEngine;

namespace NightCycle
{
    /// <summary>
    /// Раскладка клинков без пересечений. Каждый клинок приближён капсулой: отрезок от основания до острия и радиус.
    /// </summary>
    public class BladeLayout
    {
        private const float Epsilon = 1e-6f;

        private readonly List<Capsule> _capsules = new List<Capsule>();
        private readonly float _gap;

        /// <param name="gap">Минимальный зазор между поверхностями капсул, м.</param>
        public BladeLayout(float gap)
        {
            _gap = gap;
        }

        /// <summary>Сколько клинков уже в раскладке.</summary>
        public int Count => _capsules.Count;

        /// <summary>Помещается ли клинок, не задевая уже добавленные.</summary>
        public bool Fits(Vector3 from, Vector3 to, float radius)
        {
            foreach (var capsule in _capsules)
            {
                if (SegmentDistance(from, to, capsule.From, capsule.To) < radius + capsule.Radius + _gap)
                    return false;
            }

            return true;
        }

        /// <summary>Добавляет клинок без проверки. Нужен и для клинков, поставленных вручную.</summary>
        public void Add(Vector3 from, Vector3 to, float radius)
        {
            _capsules.Add(new Capsule(from, to, radius));
        }

        // Ближайшее расстояние между отрезками (Ericson, Real-Time Collision Detection, 5.1.9).
        private static float SegmentDistance(Vector3 p1, Vector3 q1, Vector3 p2, Vector3 q2)
        {
            Vector3 d1 = q1 - p1;
            Vector3 d2 = q2 - p2;
            Vector3 r = p1 - p2;
            float a = Vector3.Dot(d1, d1);
            float e = Vector3.Dot(d2, d2);
            float f = Vector3.Dot(d2, r);
            float s;
            float t;

            if (a <= Epsilon && e <= Epsilon)
                return r.magnitude;

            if (a <= Epsilon)
            {
                s = 0f;
                t = Mathf.Clamp01(f / e);
            }
            else
            {
                float c = Vector3.Dot(d1, r);
                if (e <= Epsilon)
                {
                    t = 0f;
                    s = Mathf.Clamp01(-c / a);
                }
                else
                {
                    float b = Vector3.Dot(d1, d2);
                    float denominator = a * e - b * b;
                    s = denominator > Epsilon ? Mathf.Clamp01((b * f - c * e) / denominator) : 0f;
                    t = (b * s + f) / e;

                    if (t < 0f)
                    {
                        t = 0f;
                        s = Mathf.Clamp01(-c / a);
                    }
                    else if (t > 1f)
                    {
                        t = 1f;
                        s = Mathf.Clamp01((b - c) / a);
                    }
                }
            }

            return Vector3.Distance(p1 + d1 * s, p2 + d2 * t);
        }

        private readonly struct Capsule
        {
            public readonly Vector3 From;
            public readonly Vector3 To;
            public readonly float Radius;

            public Capsule(Vector3 from, Vector3 to, float radius)
            {
                From = from;
                To = to;
                Radius = radius;
            }
        }
    }
}
