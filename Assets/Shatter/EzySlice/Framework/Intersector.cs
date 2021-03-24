using UnityEngine;

// ReSharper disable once CheckNamespace
namespace EzySlice
{
    /**
     * Contains static functionality to perform geometric intersection tests.
     */
    public static class Intersector
    {
        /**
         * Perform an intersection between Plane and Line, storing intersection point
         * in reference q. Function returns true if intersection has been found or
         * false otherwise.
         */
        public static bool Intersect(in Plane plane, in Line ln, out Vector3 q)
        {
            return Intersect(plane, ln.PositionA, ln.PositionB, out q);
        }


        private const float epsilon = 0.0001f;

        /**
         * Perform an intersection between Plane and Line made up of points a and b. Intersection
         * point will be stored in reference q. Function returns true if intersection has been
         * found or false otherwise.
         */
        private static bool Intersect(in Plane plane, in Vector3 a, in Vector3 b, out Vector3 q)
        {
            var normal = plane.normal;
            var ab = b - a;

            // TODO: IAM note -plane.distance here
            var t = (-plane.distance - Vector3.Dot(normal, a)) / Vector3.Dot(normal, ab);

            // need to be careful and compensate for floating errors
            if (t >= -epsilon && t <= (1 + epsilon))
            {
                q = a + t * ab;

                return true;
            }

            q = Vector3.zero;

            return false;
        }

        /**
         * Support functionality 
         */
        public static float TriArea2D(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            return (x1 - x2) * (y2 - y3) - (x2 - x3) * (y1 - y2);
        }

        /**
         * Perform an intersection between Plane and Triangle. This is a comprehensive function
         * which also builds a HULL Hierarchy useful for decimation projects. This obviously
         * comes at the cost of more complex code and runtime checks, but the returned results
         * are much more flexible.
         * Results will be filled into the IntersectionResult reference. Check result.isValid()
         * for the final results.
         */
        public static void Intersect(in Plane plane, in Triangle tri, IntersectionResult result)
        {
            // clear the previous results from the IntersectionResult
            result.Clear();

            // grab local variables for easier access
            var a = tri.PositionA;
            var b = tri.PositionB;
            var c = tri.PositionC;

            // check to see which side of the plane the points all
            // lay in. SideOf operation is a simple dot product and some comparison
            // operations, so these are a very quick checks
            var sa = plane.SideOf(a);
            var sb = plane.SideOf(b);
            var sc = plane.SideOf(c);

            // we cannot intersect if the triangle points all fall on the same side
            // of the plane. This is an easy early out test as no intersections are possible.
            if (sa == sb && sb == sc)
            {
                return;
            }

            // detect cases where two points lay straight on the plane, meaning
            // that the plane is actually parallel with one of the edges of the triangle
            if ((sa == PlaneEx.SideOfPlane.On && sa == sb) ||
                (sa == PlaneEx.SideOfPlane.On && sa == sc) ||
                (sb == PlaneEx.SideOfPlane.On && sb == sc))
            {
                return;
            }

            // detect cases where one point is on the plane and the other two are on the same side
            if ((sa == PlaneEx.SideOfPlane.On && sb != PlaneEx.SideOfPlane.On && sb == sc) ||
                (sb == PlaneEx.SideOfPlane.On && sa != PlaneEx.SideOfPlane.On && sa == sc) ||
                (sc == PlaneEx.SideOfPlane.On && sa != PlaneEx.SideOfPlane.On && sa == sb))
            {
                return;
            }

            // keep in mind that intersection points are shared by both
            // the upper HULL and lower HULL hence they lie perfectly
            // on the plane that cut them
            Vector3 qa;
            Vector3 qb;

            // check the cases where the points of the triangle actually lie on the plane itself
            // in these cases, there is only going to be 2 triangles, one for the upper HULL and
            // the other on the lower HULL
            // we just need to figure out which points to accept into the upper or lower hulls.
            if (sa == PlaneEx.SideOfPlane.On)
            {
                // if the point a is on the plane, test line b-c
                if (Intersect(plane, b, c, out qa))
                {
                    // line b-c intersected, construct out triangles and return appropriately
                    result.AddIntersectionPoint(qa);
                    result.AddIntersectionPoint(a);

                    // our two generated triangles, we need to figure out which
                    // triangle goes into the UPPER hull and which goes into the LOWER hull
                    var ta = new Triangle(a, b, qa);
                    var tb = new Triangle(a, qa, c);

                    // generate UV coordinates if there is any
                    if (tri.HasUV)
                    {
                        // the computed UV coordinate if the intersection point
                        var pq = tri.GenerateUV(qa);

                        ta.SetUV(tri.UvA, tri.UvB, pq);
                        tb.SetUV(tri.UvA, pq, tri.UvC);
                    }

                    // generate Normal coordinates if there is any
                    if (tri.hasNormal)
                    {
                        // the computed Normal coordinate if the intersection point
                        var pq = tri.GenerateNormal(qa);

                        ta.SetNormal(tri.normalA, tri.normalB, pq);
                        tb.SetNormal(tri.normalA, pq, tri.normalC);
                    }

                    // generate Tangent coordinates if there is any
                    if (tri.hasTangent)
                    {
                        // the computed Tangent coordinate if the intersection point
                        var pq = tri.GenerateTangent(qa);

                        ta.SetTangent(tri.tangentA, tri.tangentB, pq);
                        tb.SetTangent(tri.tangentA, pq, tri.tangentC);
                    }

                    // b point lies on the upside of the plane
                    if (sb == PlaneEx.SideOfPlane.Up)
                    {
                        result.AddUpperHull(ta).AddLowerHull(tb);
                    }

                    // b point lies on the downside of the plane
                    else if (sb == PlaneEx.SideOfPlane.Down)
                    {
                        result.AddUpperHull(tb).AddLowerHull(ta);
                    }
                }
            }

            // test the case where the b point lies on the plane itself
            else if (sb == PlaneEx.SideOfPlane.On)
            {
                // if the point b is on the plane, test line a-c
                if (Intersect(plane, a, c, out qa))
                {
                    // line a-c intersected, construct out triangles and return appropriately
                    result.AddIntersectionPoint(qa);
                    result.AddIntersectionPoint(b);

                    // our two generated triangles, we need to figure out which
                    // triangle goes into the UPPER hull and which goes into the LOWER hull
                    var ta = new Triangle(a, b, qa);
                    var tb = new Triangle(qa, b, c);

                    // generate UV coordinates if there is any
                    if (tri.HasUV)
                    {
                        // the computed UV coordinate if the intersection point
                        var uvQ = tri.GenerateUV(qa);

                        ta.SetUV(tri.UvA, tri.UvB, uvQ);
                        tb.SetUV(uvQ, tri.UvB, tri.UvC);
                    }

                    // generate Normal coordinates if there is any
                    if (tri.hasNormal)
                    {
                        // the computed Normal coordinate if the intersection point
                        var normalQ = tri.GenerateNormal(qa);

                        ta.SetNormal(tri.normalA, tri.normalB, normalQ);
                        tb.SetNormal(normalQ, tri.normalB, tri.normalC);
                    }

                    // generate Tangent coordinates if there is any
                    if (tri.hasTangent)
                    {
                        // the computed Tangent coordinate if the intersection point
                        var tangentQ = tri.GenerateTangent(qa);

                        ta.SetTangent(tri.tangentA, tri.tangentB, tangentQ);
                        tb.SetTangent(tangentQ, tri.tangentB, tri.tangentC);
                    }

                    // a point lies on the upside of the plane
                    if (sa == PlaneEx.SideOfPlane.Up)
                    {
                        result.AddUpperHull(ta).AddLowerHull(tb);
                    }

                    // a point lies on the downside of the plane
                    else if (sa == PlaneEx.SideOfPlane.Down)
                    {
                        result.AddUpperHull(tb).AddLowerHull(ta);
                    }
                }
            }

            // test the case where the c point lies on the plane itself
            else if (sc == PlaneEx.SideOfPlane.On)
            {
                // if the point c is on the plane, test line a-b
                if (Intersect(plane, a, b, out qa))
                {
                    // line a-c intersected, construct out triangles and return appropriately
                    result.AddIntersectionPoint(qa);
                    result.AddIntersectionPoint(c);

                    // our two generated triangles, we need to figure out which
                    // triangle goes into the UPPER hull and which goes into the LOWER hull
                    var ta = new Triangle(a, qa, c);
                    var tb = new Triangle(qa, b, c);

                    // generate UV coordinates if there is any
                    if (tri.HasUV)
                    {
                        // the computed UV coordinate if the intersection point
                        var pq = tri.GenerateUV(qa);

                        ta.SetUV(tri.UvA, pq, tri.UvC);
                        tb.SetUV(pq, tri.UvB, tri.UvC);
                    }

                    // generate Normal coordinates if there is any
                    if (tri.hasNormal)
                    {
                        // the computed Normal coordinate if the intersection point
                        var pq = tri.GenerateNormal(qa);

                        ta.SetNormal(tri.normalA, pq, tri.normalC);
                        tb.SetNormal(pq, tri.normalB, tri.normalC);
                    }

                    // generate Tangent coordinates if there is any
                    if (tri.hasTangent)
                    {
                        // the computed Tangent coordinate if the intersection point
                        var pq = tri.GenerateTangent(qa);

                        ta.SetTangent(tri.tangentA, pq, tri.tangentC);
                        tb.SetTangent(pq, tri.tangentB, tri.tangentC);
                    }

                    // a point lies on the upside of the plane
                    if (sa == PlaneEx.SideOfPlane.Up)
                    {
                        result.AddUpperHull(ta).AddLowerHull(tb);
                    }

                    // a point lies on the downside of the plane
                    else if (sa == PlaneEx.SideOfPlane.Down)
                    {
                        result.AddUpperHull(tb).AddLowerHull(ta);
                    }
                }
            }

            // at this point, all edge cases have been tested and failed, we need to perform
            // full intersection tests against the lines. From this point onwards we will generate
            // 3 triangles
            else if (sa != sb && Intersect(plane, a, b, out qa))
            {
                // intersection found against a - b
                result.AddIntersectionPoint(qa);

                // since intersection was found against a - b, we need to check which other
                // lines to check (we only need to check one more line) for intersection.
                // the line we check against will be the line against the point which lies on
                // the other side of the plane.
                if (sa == sc)
                {
                    // we likely have an intersection against line b-c which will complete this loop
                    if (Intersect(plane, b, c, out qb))
                    {
                        result.AddIntersectionPoint(qb);

                        // our three generated triangles. Two of these triangles will end
                        // up on either the UPPER or LOWER hulls.
                        var ta = new Triangle(qa, b, qb);
                        var tb = new Triangle(a, qa, qb);
                        var tc = new Triangle(a, qb, c);

                        // generate UV coordinates if there is any
                        if (tri.HasUV)
                        {
                            // the computed UV coordinate if the intersection point
                            var pqa = tri.GenerateUV(qa);
                            var pqb = tri.GenerateUV(qb);

                            ta.SetUV(pqa, tri.UvB, pqb);
                            tb.SetUV(tri.UvA, pqa, pqb);
                            tc.SetUV(tri.UvA, pqb, tri.UvC);
                        }

                        // generate Normal coordinates if there is any
                        if (tri.hasNormal)
                        {
                            // the computed Normal coordinate if the intersection point
                            var pqa = tri.GenerateNormal(qa);
                            var pqb = tri.GenerateNormal(qb);

                            ta.SetNormal(pqa, tri.normalB, pqb);
                            tb.SetNormal(tri.normalA, pqa, pqb);
                            tc.SetNormal(tri.normalA, pqb, tri.normalC);
                        }

                        // generate Tangent coordinates if there is any
                        if (tri.hasTangent)
                        {
                            // the computed Tangent coordinate if the intersection point
                            var pqa = tri.GenerateTangent(qa);
                            var pqb = tri.GenerateTangent(qb);

                            ta.SetTangent(pqa, tri.tangentB, pqb);
                            tb.SetTangent(tri.tangentA, pqa, pqb);
                            tc.SetTangent(tri.tangentA, pqb, tri.tangentC);
                        }

                        if (sa == PlaneEx.SideOfPlane.Up)
                        {
                            result.AddUpperHull(tb).AddUpperHull(tc).AddLowerHull(ta);
                        }
                        else
                        {
                            result.AddLowerHull(tb).AddLowerHull(tc).AddUpperHull(ta);
                        }
                    }
                }
                else
                {
                    // in this scenario, the point a is a "lone" point which lies in either upper
                    // or lower HULL. We need to perform another intersection to find the last point
                    if (Intersect(plane, a, c, out qb))
                    {
                        result.AddIntersectionPoint(qb);

                        // our three generated triangles. Two of these triangles will end
                        // up on either the UPPER or LOWER hulls.
                        var ta = new Triangle(a, qa, qb);
                        var tb = new Triangle(qa, b, c);
                        var tc = new Triangle(qb, qa, c);

                        // generate UV coordinates if there is any
                        if (tri.HasUV)
                        {
                            // the computed UV coordinate if the intersection point
                            var pqa = tri.GenerateUV(qa);
                            var pqb = tri.GenerateUV(qb);

                            ta.SetUV(tri.UvA, pqa, pqb);
                            tb.SetUV(pqa, tri.UvB, tri.UvC);
                            tc.SetUV(pqb, pqa, tri.UvC);
                        }

                        // generate Normal coordinates if there is any
                        if (tri.hasNormal)
                        {
                            // the computed Normal coordinate if the intersection point
                            var pqa = tri.GenerateNormal(qa);
                            var pqb = tri.GenerateNormal(qb);

                            ta.SetNormal(tri.normalA, pqa, pqb);
                            tb.SetNormal(pqa, tri.normalB, tri.normalC);
                            tc.SetNormal(pqb, pqa, tri.normalC);
                        }

                        // generate Tangent coordinates if there is any
                        if (tri.hasTangent)
                        {
                            // the computed Tangent coordinate if the intersection point
                            var pqa = tri.GenerateTangent(qa);
                            var pqb = tri.GenerateTangent(qb);

                            ta.SetTangent(tri.tangentA, pqa, pqb);
                            tb.SetTangent(pqa, tri.tangentB, tri.tangentC);
                            tc.SetTangent(pqb, pqa, tri.tangentC);
                        }

                        if (sa == PlaneEx.SideOfPlane.Up)
                        {
                            result.AddUpperHull(ta).AddLowerHull(tb).AddLowerHull(tc);
                        }
                        else if (sa == PlaneEx.SideOfPlane.Down)
                        {
                            result.AddLowerHull(ta).AddUpperHull(tb).AddUpperHull(tc);
                        }
#if SHITDEBUG
                        else
                        {
                            int q = 0;
                        }
#endif
                    }
                }
            }

            // if line a-b did not intersect (or the lie on the same side of the plane)
            // this simplifies the problem a fair bit. This means we have an intersection 
            // in line a-c and b-c, which we can use to build a new UPPER and LOWER hulls
            // we are expecting both of these intersection tests to pass, otherwise something
            // went wrong (float errors? missed a checked case?)
            else if (Intersect(plane, c, a, out qa) && Intersect(plane, c, b, out qb))
            {
                // in here we know that line a-b actually lie on the same side of the plane, this will
                // simplify the rest of the logic. We also have our intersection points
                // the computed UV coordinate of the intersection point

                result.AddIntersectionPoint(qa);
                result.AddIntersectionPoint(qb);

                // our three generated triangles. Two of these triangles will end
                // up on either the UPPER or LOWER hulls.
                var ta = new Triangle(qa, qb, c);
                var tb = new Triangle(a, qb, qa);
                var tc = new Triangle(a, b, qb);

                // generate UV coordinates if there is any
                if (tri.HasUV)
                {
                    // the computed UV coordinate if the intersection point
                    var pqa = tri.GenerateUV(qa);
                    var pqb = tri.GenerateUV(qb);

                    ta.SetUV(pqa, pqb, tri.UvC);
                    tb.SetUV(tri.UvA, pqb, pqa);
                    tc.SetUV(tri.UvA, tri.UvB, pqb);
                }

                // generate Normal coordinates if there is any
                if (tri.hasNormal)
                {
                    // the computed Normal coordinate if the intersection point
                    var pqa = tri.GenerateNormal(qa);
                    var pqb = tri.GenerateNormal(qb);

                    ta.SetNormal(pqa, pqb, tri.normalC);
                    tb.SetNormal(tri.normalA, pqb, pqa);
                    tc.SetNormal(tri.normalA, tri.normalB, pqb);
                }

                // generate Tangent coordinates if there is any
                if (tri.hasTangent)
                {
                    // the computed Tangent coordinate if the intersection point
                    var pqa = tri.GenerateTangent(qa);
                    var pqb = tri.GenerateTangent(qb);

                    ta.SetTangent(pqa, pqb, tri.tangentC);
                    tb.SetTangent(tri.tangentA, pqb, pqa);
                    tc.SetTangent(tri.tangentA, tri.tangentB, pqb);
                }

                if (sa == PlaneEx.SideOfPlane.Up)
                {
                    result.AddUpperHull(tb).AddUpperHull(tc).AddLowerHull(ta);
                }
                else
                {
                    result.AddLowerHull(tb).AddLowerHull(tc).AddUpperHull(ta);
                }
            }
        }
    }
}