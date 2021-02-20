using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using static UnityEngine.Object;

public static class TransformEx
{
    //public static Transform FirstChildOrDefault(this Transform parent, Func<Transform, bool> query)
    //{
    //    foreach (Transform child in parent)
    //    {
    //        if (query(child))
    //        {
    //            return child;
    //        }
    //        else
    //        {
    //            var c = FirstChildOrDefault(child, query);
    //            if (c != null)
    //            {
    //                return c;
    //            }
    //        }
    //    }
    //    return null;
    //}

    /// <summary>
    /// Copy the source transform to the target. Note that scale is only local and may not be what you expect if the target has a world transform scale.
    /// </summary>
    /// <param name="target">The target transform to set.</param>
    /// <param name="source">The source transform.</param>
    public static void CopyTransformFrom(this Transform target, Transform source)
    {
        var s = source.localToWorldMatrix;
        var t = target.worldToLocalMatrix;
        var m = t * s;
        target.SetLocalFromMatrix(m);
    }

    public static Transform FindDeep(this Transform parent, string name, bool activeOnly = true, bool depthFirst = false)
    {
        if (depthFirst)
        {
            if (parent.name == name)
            {
                if (!activeOnly || parent.gameObject.activeSelf)
                    return parent;
            }
            foreach (Transform child in parent)
            {
                if (!activeOnly || child.gameObject.activeSelf)
                {
                    var result = child.FindDeep(name, activeOnly, depthFirst);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }
        else
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(parent);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c.name == name)
                {
                    if (!activeOnly || c.gameObject.activeSelf)
                        return c;
                }
                foreach (Transform t in c)
                    queue.Enqueue(t);
            }
            return null;
        }
    }

    public static void SetLocalFromMatrix(this Transform transform, Matrix4x4 matrix)
    {
        transform.localScale = matrix.ExtractScale();
        transform.localRotation = matrix.ExtractRotation();
        transform.localPosition = matrix.ExtractPosition();
    }

    public static void SetWorldFromMatrix(this Transform transform, Matrix4x4 matrix)
    {
        var scale = matrix.ExtractScale(); // world scale
        if (transform.parent) scale = transform.parent.InverseTransformVector(scale); // scale into local space
        // Assert.AreApproximatelyEqual(1, localScale.x);
        // Assert.AreApproximatelyEqual(1, localScale.y);
        // Assert.AreApproximatelyEqual(1, localScale.z);
        transform.localScale = scale;
        transform.rotation = matrix.ExtractRotation();
        transform.position = matrix.ExtractPosition();
    }

    // Get the local to parent matrix
    public static Matrix4x4 GetLocalMatrix(this Transform transform)
    {
        //TODO: remove appropriate method 
        //var m = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
        var parent = transform.parent;
        var localToWorldMatrix = transform.localToWorldMatrix;
        var m = parent ? parent.localToWorldMatrix.inverse * localToWorldMatrix : localToWorldMatrix;
        return m;
    }

    public static void SetLocalIdentity(this Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public static void SetWorldIdentity(this Transform transform)
    {
        transform.position = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public static Transform GetPreviousSibling(this Transform transform)
    {
        var siblingIndex = transform.GetSiblingIndex();
        return siblingIndex == 0 ? null : transform.parent.GetChild(siblingIndex - 1);
    }

    public static Transform GetNextSibling(this Transform transform)
    {
        var siblingIndex = transform.GetSiblingIndex();
        var parent = transform.parent;
        return siblingIndex == parent.childCount - 1 ? null : parent.GetChild(siblingIndex + 1);
    }

    public static int GetNumSiblingsAfter(this Transform transform)
    {
        var idx = transform.GetSiblingIndex();
        return transform.parent.childCount - idx - 1;
    }

    public static Transform GetFirstChild(this Transform transform)
    {
        return transform.childCount == 0 ? null : transform.GetChild(0);
    }

    public static Transform GetLastChild(this Transform transform)
    {
        var childCount = transform.childCount;
        return childCount == 0 ? null : transform.GetChild(childCount - 1);
    }

//    public static void TraverseDepthFirst(this Transform transform, Delegate @delegate)
//    {
//    }

    public static Bounds TransformBounds(this Transform transform, Bounds localBounds)
    {
        var center = transform.TransformPoint(localBounds.center);

        // transform the local extents' axes
        var extents = localBounds.extents;
        var axisX = transform.TransformVector(extents.x, 0, 0);
        var axisY = transform.TransformVector(0, extents.y, 0);
        var axisZ = transform.TransformVector(0, 0, extents.z);

        // TODO checking
        var e = transform.TransformVector(extents);
        Debug.Assert(e == new Vector3(axisX.magnitude, axisY.magnitude, axisZ.magnitude));

        // sum their absolute value to get the world extents
        extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
        extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

        return new Bounds {center = center, extents = extents};
    }

    public static void DestroyGameObjects(this Transform transform, bool recursive)
    {
        if (recursive)
        {
            foreach (Transform child in transform)
            {
                child.DestroyGameObjects(true);
            }
        }
        Destroy(transform.gameObject);
    }

    public static Plane InverseTransformPlane(this Transform transform, in Plane plane)
    {
#if Q
        var m = transform.localToWorldMatrix;
        var n3 = plane.normal;
        var p3 = n3 * plane.distance;
        var n = m.inverse.transpose * new Vector4(n3.x, n3.y, n3.z, 0);
        var p = m * new Vector4(p3.x, p3.y, p3.z, 1);
        // n = n.normalized;
        return new Plane(n, Vector3.Dot(plane.normal, p));
#elif Q
        var p = plane.normal * -plane.distance;
        p = transform.InverseTransformPoint(p);
        var n = transform.InverseTransformVector(plane.normal);
        var q = new Plane(n, p);
        
        var m = transform.localToWorldMatrix.inverse.transpose;
        var p4 = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, -plane.distance);
        p4 = m * p4;
        n = new Vector3(p.x, p.y, p.z); //.normalized;
        var pos = plane.normal * -plane.distance;
        // pos = transform.InverseTransformPoint(pos);
        var q2 = new Plane(n, pos);
        return p4;
#else
        var p = plane.normal * -plane.distance;
        p = transform.InverseTransformPoint(p);
        var n = transform.InverseTransformDirection(plane.normal);
        n.Scale(transform.lossyScale);
        var pl = new Plane(n, p);
        return pl;
#endif
    }
}