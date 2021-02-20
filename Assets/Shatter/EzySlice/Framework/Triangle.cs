using UnityEngine;
using static UnityEngine.Vector3;

// ReSharper disable once CheckNamespace
namespace EzySlice
{
    /**
     * Represents a simple 3D Triangle structure with position
     * and UV map. The UV is required if the slicer needs
     * to recalculate the new UV position for texture mapping.
     */
    public struct Triangle
    {
        // the points which represent this triangle
        // these have to be set and are immutable. Cannot be
        // changed once set

        // the UV coordinates of this triangle
        // these are optional and may not be set

        // the Normals of the Vertices
        // these are optional and may not be set
        private bool mNorSet;
        private Vector3 mNorA;
        private Vector3 mNorB;
        private Vector3 mNorC;

        // the Tangents of the Vertices
        // these are optional and may not be set
        private bool mTanSet;
        private Vector4 mTanA;
        private Vector4 mTanB;
        private Vector4 mTanC;

        public Triangle(in Vector3 posA, in Vector3 posB, in Vector3 posC)
        {
            PositionA = posA;
            PositionB = posB;
            PositionC = posC;

            HasUV = false;
            UvA = Vector2.zero;
            UvB = Vector2.zero;
            UvC = Vector2.zero;

            mNorSet = false;
            mNorA = zero;
            mNorB = zero;
            mNorC = zero;

            mTanSet = false;
            mTanA = Vector4.zero;
            mTanB = Vector4.zero;
            mTanC = Vector4.zero;
        }

        public Vector3 PositionA { get; }

        public Vector3 PositionB { get; }

        public Vector3 PositionC { get; }

        public bool HasUV { get; private set; }

        public void SetUV(in Vector2 inUvA, in Vector2 inUvB, in Vector2 inUvC)
        {
            UvA = inUvA;
            UvB = inUvB;
            UvC = inUvC;
            HasUV = true;
        }

        public Vector2 UvA { get; private set; }

        public Vector2 UvB { get; private set; }

        public Vector2 UvC { get; private set; }

        public bool hasNormal => mNorSet;

        public void SetNormal(in Vector3 norA, in Vector3 norB, in Vector3 norC)
        {
            mNorA = norA;
            mNorB = norB;
            mNorC = norC;
            mNorSet = true;
        }

        public Vector3 normalA => mNorA;

        public Vector3 normalB => mNorB;

        public Vector3 normalC => mNorC;

        public bool hasTangent => mTanSet;

        public void SetTangent(in Vector4 tanA, in Vector4 tanB, in Vector4 tanC)
        {
            mTanA = tanA;
            mTanB = tanB;
            mTanC = tanC;
            mTanSet = true;
        }

        public Vector4 tangentA => mTanA;

        public Vector4 tangentB => mTanB;

        public Vector4 tangentC => mTanC;

        /**
         * Compute and set the tangents of this triangle
         * Derived From https://answers.unity.com/questions/7789/calculating-tangents-vector4.html
         */
        public void ComputeTangents()
        {
            // computing tangents requires both UV and normals set
            if (!mNorSet || !HasUV)
            {
                return;
            }

            var v1 = PositionA;
            var v2 = PositionB;
            var v3 = PositionC;

            var w1 = UvA;
            var w2 = UvB;
            var w3 = UvC;

            var x1 = v2.x - v1.x;
            var x2 = v3.x - v1.x;
            var y1 = v2.y - v1.y;
            var y2 = v3.y - v1.y;
            var z1 = v2.z - v1.z;
            var z2 = v3.z - v1.z;

            var s1 = w2.x - w1.x;
            var s2 = w3.x - w1.x;
            var t1 = w2.y - w1.y;
            var t2 = w3.y - w1.y;

            var r = 1.0f / (s1 * t2 - s2 * t1);

            var sDir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            var tDir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

            var n1 = mNorA;
            var nt1 = sDir;

            OrthoNormalize(ref n1, ref nt1);
            var tanA = new Vector4(nt1.x, nt1.y, nt1.z, (Dot(Cross(n1, nt1), tDir) < 0.0f) ? -1.0f : 1.0f);

            var n2 = mNorB;
            var nt2 = sDir;

            OrthoNormalize(ref n2, ref nt2);
            var tanB = new Vector4(nt2.x, nt2.y, nt2.z, (Dot(Cross(n2, nt2), tDir) < 0.0f) ? -1.0f : 1.0f);

            var n3 = mNorC;
            var nt3 = sDir;

            OrthoNormalize(ref n3, ref nt3);
            var tanC = new Vector4(nt3.x, nt3.y, nt3.z, (Dot(Cross(n3, nt3), tDir) < 0.0f) ? -1.0f : 1.0f);

            // finally set the tangents of this object
            SetTangent(tanA, tanB, tanC);
        }

        /**
         * Calculate the Barycentric coordinate weight values u-v-w for Point p in respect to the provided
         * triangle. This is useful for computing new UV coordinates for arbitrary points.
         */
        private readonly Vector3 Barycentric(in Vector3 p)
        {
            var a = PositionA;
            var b = PositionB;
            var c = PositionC;

            var m = Cross(b - a, c - a);

            float nu;
            float nv;
            float ood;

            var x = Mathf.Abs(m.x);
            var y = Mathf.Abs(m.y);
            var z = Mathf.Abs(m.z);

            // compute areas of plane with largest projections
            if (x >= y && x >= z)
            {
                // area of PBC in yz plane
                nu = Intersector.TriArea2D(p.y, p.z, b.y, b.z, c.y, c.z);
                // area of PCA in yz plane
                nv = Intersector.TriArea2D(p.y, p.z, c.y, c.z, a.y, a.z);
                // 1/2*area of ABC in yz plane
                ood = 1.0f / m.x;
            }
            else if (y >= x && y >= z)
            {
                // project in xz plane
                nu = Intersector.TriArea2D(p.x, p.z, b.x, b.z, c.x, c.z);
                nv = Intersector.TriArea2D(p.x, p.z, c.x, c.z, a.x, a.z);
                ood = 1.0f / -m.y;
            }
            else
            {
                // project in xy plane
                nu = Intersector.TriArea2D(p.x, p.y, b.x, b.y, c.x, c.y);
                nv = Intersector.TriArea2D(p.x, p.y, c.x, c.y, a.x, a.y);
                ood = 1.0f / m.z;
            }

            var u = nu * ood;
            var v = nv * ood;
            var w = 1.0f - u - v;

            return new Vector3(u, v, w);
        }

        /**
         * Generate a set of new UV coordinates for the provided point pt in respect to Triangle.
         * 
         * Uses weight values for the computation, so this triangle must have UV's set to return
         * the correct results. Otherwise Vector2.zero will be returned. check via hasUV().
         */
        public readonly Vector2 GenerateUV(in Vector3 pt)
        {
            // if not set, result will be zero, quick exit
            if (!HasUV)
            {
                return Vector2.zero;
            }

            var weights = Barycentric(pt);

            return (weights.x * UvA) + (weights.y * UvB) + (weights.z * UvC);
        }

        /**
         * Generates a set of new Normal coordinates for the provided point pt in respect to Triangle.
         * 
         * Uses weight values for the computation, so this triangle must have Normal's set to return
         * the correct results. Otherwise Vector3.zero will be returned. check via hasNormal().
         */
        public readonly Vector3 GenerateNormal(in Vector3 pt)
        {
            // if not set, result will be zero, quick exit
            if (!mNorSet)
            {
                return zero;
            }

            var weights = Barycentric(pt);

            return (weights.x * mNorA) + (weights.y * mNorB) + (weights.z * mNorC);
        }

        /**
         * Generates a set of new Tangent coordinates for the provided point pt in respect to Triangle.
         * 
         * Uses weight values for the computation, so this triangle must have Tangent's set to return
         * the correct results. Otherwise Vector4.zero will be returned. check via hasTangent().
         */
        public readonly Vector4 GenerateTangent(in Vector3 pt)
        {
            // if not set, result will be zero, quick exit
            if (!mNorSet)
            {
                return Vector4.zero;
            }

            var weights = Barycentric(pt);

            return (weights.x * mTanA) + (weights.y * mTanB) + (weights.z * mTanC);
        }

        /**
         * Helper function to split this triangle by the provided plane and store
         * the results inside the IntersectionResult structure.
         * Returns true on success or false otherwise
         */
        public bool Split(in Plane pl, IntersectionResult result)
        {
            Intersector.Intersect(pl, this, result);

            return result.IsValid;
        }

        /**
         * Check the triangle winding order, if it's Clock Wise or Counter Clock Wise 
         */
        public readonly bool IsCWinding()
        {
            return SignedSquare(PositionA, PositionB, PositionC) >= float.Epsilon;
        }

        /**
         * Returns the Signed square of a given triangle, useful for checking the
         * winding order
         */
        private static float SignedSquare(in Vector3 a, in Vector3 b, in Vector3 c)
        {
            return a.x * (b.y * c.z - b.z * c.y) -
                   a.y * (b.x * c.z - b.z * c.x) +
                   a.z * (b.x * c.y - b.y * c.x);
        }

        /**
         * Editor only DEBUG functionality. This should not be compiled in the final
         * Version.
         */
        public void OnDebugDraw()
        {
            OnDebugDraw(Color.white);
        }

        public readonly void OnDebugDraw(in Color drawColor)
        {
#if UNITY_EDITOR
            var prevColor = Gizmos.color;

            Gizmos.color = drawColor;

            Gizmos.DrawLine(PositionA, PositionB);
            Gizmos.DrawLine(PositionB, PositionC);
            Gizmos.DrawLine(PositionC, PositionA);

            Gizmos.color = prevColor;
#endif
        }
    }
}