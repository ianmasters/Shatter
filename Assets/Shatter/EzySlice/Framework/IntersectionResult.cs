using UnityEngine;

// ReSharper disable once CheckNamespace
namespace EzySlice
{
    /**
     * A Basic Structure which contains intersection information
     * for Plane->Triangle Intersection Tests
     * TO-DO -> This structure can be optimized to hold less data
     * via an optional indices array. Could lead for a faster
     * intersection test as well.
     */
    public sealed class IntersectionResult
    {
        // general tag to check if this structure is valid

        // our intersection points/triangles

        // our counters. We use raw arrays for performance reasons

        public IntersectionResult()
        {
            IsValid = false;

            UpperHull = new Triangle[2];
            LowerHull = new Triangle[2];
            IntersectionPoints = new Vector3[2];

            UpperHullCount = 0;
            LowerHullCount = 0;
            IntersectionPointCount = 0;
        }

        public Triangle[] UpperHull { get; }

        public Triangle[] LowerHull { get; }

        public Vector3[] IntersectionPoints { get; }

        public int UpperHullCount { get; private set; }

        public int LowerHullCount { get; private set; }

        public int IntersectionPointCount { get; private set; }

        public bool IsValid { get; private set; }

        /**
         * Used by the intersector, adds a new triangle to the
         * upper hull section
         */
        public IntersectionResult AddUpperHull(in Triangle tri)
        {
            UpperHull[UpperHullCount++] = tri;

            IsValid = true;

            return this;
        }

        /**
         * Used by the intersector, adds a new triangle to the
         * lower gull section
         */
        public IntersectionResult AddLowerHull(in Triangle tri)
        {
            LowerHull[LowerHullCount++] = tri;

            IsValid = true;

            return this;
        }

        /**
         * Used by the intersector, adds a new intersection point
         * which is shared by both upper->lower hulls
         */
        public void AddIntersectionPoint(in Vector3 pt)
        {
            IntersectionPoints[IntersectionPointCount++] = pt;
        }

        /**
         * Clear the current state of this object 
         */
        public void Clear()
        {
            IsValid = false;
            UpperHullCount = 0;
            LowerHullCount = 0;
            IntersectionPointCount = 0;
        }

        /**
         * Editor only DEBUG functionality. This should not be compiled in the final
         * Version.
         */
        public void OnDebugDraw()
        {
            OnDebugDraw(Color.white);
        }

#if UNITY_EDITOR
        public void OnDebugDraw(in Color drawColor)
        {
            if (!IsValid)
            {
                return;
            }

            var prevColor = Gizmos.color;

            Gizmos.color = drawColor;

            // draw the intersection points
            for (var i = 0; i < IntersectionPointCount; i++)
            {
                Gizmos.DrawSphere(IntersectionPoints[i], 0.1f);
            }

            // draw the upper hull in RED
            for (var i = 0; i < UpperHullCount; i++)
            {
                UpperHull[i].OnDebugDraw(Color.red);
            }

            // draw the lower hull in BLUE
            for (var i = 0; i < LowerHullCount; i++)
            {
                LowerHull[i].OnDebugDraw(Color.blue);
            }

            Gizmos.color = prevColor;
        }
#endif
    }
}