using UnityEngine;

/// <summary>
/// Debug Extension
/// 	- Static class that extends Unity's debugging functionality.
/// 	- Attempts to mimic Unity's existing debugging behaviour for ease-of-use.
/// 	- Includes gizmo drawing methods for less memory-intensive debug visualization.
/// </summary>

// ReSharper disable once CheckNamespace
public static class DebugExtension
{
    #region DebugDrawFunctions

    /// <summary>
    /// 	- Debugs a point.
    /// </summary>
    /// <param name='position'>
    /// 	- The point to debug.
    /// </param>
    /// <param name='color'>
    /// 	- The color of the point.
    /// </param>
    /// <param name='scale'>
    /// 	- The size of the point.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the point.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not this point should be faded when behind other objects.
    /// </param>
    public static void DebugPoint(in Vector3 position, Color color, float scale = 1.0f, float duration = 0, bool depthTest = true)
    {
        color = (color == default(Color)) ? Color.white : color;

        Debug.DrawRay(position + (Vector3.up * (scale * 0.5f)), -Vector3.up * scale, color, duration, depthTest);
        Debug.DrawRay(position + (Vector3.right * (scale * 0.5f)), -Vector3.right * scale, color, duration, depthTest);
        Debug.DrawRay(position + (Vector3.forward * (scale * 0.5f)), -Vector3.forward * scale, color, duration, depthTest);
    }

    /// <summary>
    /// 	- Debugs a point.
    /// </summary>
    /// <param name='position'>
    /// 	- The point to debug.
    /// </param>
    /// <param name='scale'>
    /// 	- The size of the point.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the point.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not this point should be faded when behind other objects.
    /// </param>
    public static void DebugPoint(in Vector3 position, float scale = 1.0f, float duration = 0, bool depthTest = true)
    {
        DebugPoint(position, Color.white, scale, duration, depthTest);
    }

    /// <summary>
    /// 	- Debugs an axis-aligned bounding box.
    /// </summary>
    /// <param name='bounds'>
    /// 	- The bounds to debug.
    /// </param>
    /// <param name='color'>
    /// 	- The color of the bounds.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the bounds.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not the bounds should be faded when behind other objects.
    /// </param>
    public static void DebugBounds(in Bounds bounds, in Color color, float duration = 0, bool depthTest = true)
    {
        var center = bounds.center;

        var x = bounds.extents.x;
        var y = bounds.extents.y;
        var z = bounds.extents.z;

        var ruf = center + new Vector3(x, y, z);
        var rub = center + new Vector3(x, y, -z);
        var luf = center + new Vector3(-x, y, z);
        var lub = center + new Vector3(-x, y, -z);

        var rdf = center + new Vector3(x, -y, z);
        var rdb = center + new Vector3(x, -y, -z);
        var lfd = center + new Vector3(-x, -y, z);
        var lbd = center + new Vector3(-x, -y, -z);

        Debug.DrawLine(ruf, luf, color, duration, depthTest);
        Debug.DrawLine(ruf, rub, color, duration, depthTest);
        Debug.DrawLine(luf, lub, color, duration, depthTest);
        Debug.DrawLine(rub, lub, color, duration, depthTest);

        Debug.DrawLine(ruf, rdf, color, duration, depthTest);
        Debug.DrawLine(rub, rdb, color, duration, depthTest);
        Debug.DrawLine(luf, lfd, color, duration, depthTest);
        Debug.DrawLine(lub, lbd, color, duration, depthTest);

        Debug.DrawLine(rdf, lfd, color, duration, depthTest);
        Debug.DrawLine(rdf, rdb, color, duration, depthTest);
        Debug.DrawLine(lfd, lbd, color, duration, depthTest);
        Debug.DrawLine(lbd, rdb, color, duration, depthTest);
    }

    /// <summary>
    /// 	- Debugs an axis-aligned bounding box.
    /// </summary>
    /// <param name='bounds'>
    /// 	- The bounds to debug.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the bounds.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not the bounds should be faded when behind other objects.
    /// </param>
    public static void DebugBounds(in Bounds bounds, float duration = 0, bool depthTest = true)
    {
        DebugBounds(bounds, Color.white, duration, depthTest);
    }

    /// <summary>
    /// 	- Debugs a local cube.
    /// </summary>
    /// <param name='transform'>
    /// 	- The transform that the cube will be local to.
    /// </param>
    /// <param name='size'>
    /// 	- The size of the cube.
    /// </param>
    /// <param name='color'>
    /// 	- Color of the cube.
    /// </param>
    /// <param name='center'>
    /// 	- The position (relative to transform) where the cube will be debugged.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the cube.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not the cube should be faded when behind other objects.
    /// </param>
    public static void DebugLocalCube(Transform transform, in Vector3 size, in Color color, in Vector3 center = default, float duration = 0, bool depthTest = true)
    {
        var lbb = transform.TransformPoint(center + ((-size) * 0.5f));
        var rbb = transform.TransformPoint(center + (new Vector3(size.x, -size.y, -size.z) * 0.5f));

        var lbf = transform.TransformPoint(center + (new Vector3(size.x, -size.y, size.z) * 0.5f));
        var rbf = transform.TransformPoint(center + (new Vector3(-size.x, -size.y, size.z) * 0.5f));

        var lub = transform.TransformPoint(center + (new Vector3(-size.x, size.y, -size.z) * 0.5f));
        var rub = transform.TransformPoint(center + (new Vector3(size.x, size.y, -size.z) * 0.5f));

        var luf = transform.TransformPoint(center + ((size) * 0.5f));
        var ruf = transform.TransformPoint(center + (new Vector3(-size.x, size.y, size.z) * 0.5f));

        Debug.DrawLine(lbb, rbb, color, duration, depthTest);
        Debug.DrawLine(rbb, lbf, color, duration, depthTest);
        Debug.DrawLine(lbf, rbf, color, duration, depthTest);
        Debug.DrawLine(rbf, lbb, color, duration, depthTest);

        Debug.DrawLine(lub, rub, color, duration, depthTest);
        Debug.DrawLine(rub, luf, color, duration, depthTest);
        Debug.DrawLine(luf, ruf, color, duration, depthTest);
        Debug.DrawLine(ruf, lub, color, duration, depthTest);

        Debug.DrawLine(lbb, lub, color, duration, depthTest);
        Debug.DrawLine(rbb, rub, color, duration, depthTest);
        Debug.DrawLine(lbf, luf, color, duration, depthTest);
        Debug.DrawLine(rbf, ruf, color, duration, depthTest);
    }

    /// <summary>
    /// 	- Debugs a local cube.
    /// </summary>
    /// <param name='transform'>
    /// 	- The transform that the cube will be local to.
    /// </param>
    /// <param name='size'>
    /// 	- The size of the cube.
    /// </param>
    /// <param name='center'>
    /// 	- The position (relative to transform) where the cube will be debugged.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the cube.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not the cube should be faded when behind other objects.
    /// </param>
    public static void DebugLocalCube(Transform transform, in Vector3 size, in Vector3 center = default(Vector3), float duration = 0, bool depthTest = true)
    {
        DebugLocalCube(transform, size, Color.white, center, duration, depthTest);
    }

    /// <summary>
    /// 	- Debugs a local cube.
    /// </summary>
    /// <param name='space'>
    /// 	- The space the cube will be local to.
    /// </param>
    /// <param name='size'>
    ///		- The size of the cube.
    /// </param>
    /// <param name='color'>
    /// 	- Color of the cube.
    /// </param>
    /// <param name='center'>
    /// 	- The position (relative to transform) where the cube will be debugged.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the cube.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not the cube should be faded when behind other objects.
    /// </param>
    public static void DebugLocalCube(Matrix4x4 space, in Vector3 size, Color color, in Vector3 center = default, float duration = 0, bool depthTest = true)
    {
        color = color == default ? Color.white : color;

        var lbb = space.MultiplyPoint3x4(center + ((-size) * 0.5f));
        var rbb = space.MultiplyPoint3x4(center + (new Vector3(size.x, -size.y, -size.z) * 0.5f));

        var lbf = space.MultiplyPoint3x4(center + (new Vector3(size.x, -size.y, size.z) * 0.5f));
        var rbf = space.MultiplyPoint3x4(center + (new Vector3(-size.x, -size.y, size.z) * 0.5f));

        var lub = space.MultiplyPoint3x4(center + (new Vector3(-size.x, size.y, -size.z) * 0.5f));
        var rub = space.MultiplyPoint3x4(center + (new Vector3(size.x, size.y, -size.z) * 0.5f));

        var luf = space.MultiplyPoint3x4(center + ((size) * 0.5f));
        var ruf = space.MultiplyPoint3x4(center + (new Vector3(-size.x, size.y, size.z) * 0.5f));

        Debug.DrawLine(lbb, rbb, color, duration, depthTest);
        Debug.DrawLine(rbb, lbf, color, duration, depthTest);
        Debug.DrawLine(lbf, rbf, color, duration, depthTest);
        Debug.DrawLine(rbf, lbb, color, duration, depthTest);

        Debug.DrawLine(lub, rub, color, duration, depthTest);
        Debug.DrawLine(rub, luf, color, duration, depthTest);
        Debug.DrawLine(luf, ruf, color, duration, depthTest);
        Debug.DrawLine(ruf, lub, color, duration, depthTest);

        Debug.DrawLine(lbb, lub, color, duration, depthTest);
        Debug.DrawLine(rbb, rub, color, duration, depthTest);
        Debug.DrawLine(lbf, luf, color, duration, depthTest);
        Debug.DrawLine(rbf, ruf, color, duration, depthTest);
    }

    /// <summary>
    /// 	- Debugs a local cube.
    /// </summary>
    /// <param name='space'>
    /// 	- The space the cube will be local to.
    /// </param>
    /// <param name='size'>
    ///		- The size of the cube.
    /// </param>
    /// <param name='center'>
    /// 	- The position (relative to transform) where the cube will be debugged.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the cube.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not the cube should be faded when behind other objects.
    /// </param>
    public static void DebugLocalCube(in Matrix4x4 space, in Vector3 size, in Vector3 center = default(Vector3), float duration = 0, bool depthTest = true)
    {
        DebugLocalCube(space, size, Color.white, center, duration, depthTest);
    }

    /// <summary>
    /// 	- Debugs a circle.
    /// </summary>
    /// <param name='position'>
    /// 	- Where the center of the circle will be positioned.
    /// </param>
    /// <param name='normal'>
    /// 	- The direction perpendicular to the surface of the circle.
    /// </param>
    /// <param name='color'>
    /// 	- The color of the circle.
    /// </param>
    /// <param name='radius'>
    /// 	- The radius of the circle.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the circle.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not the circle should be faded when behind other objects.
    /// </param>
    public static void DebugCircle(in Vector3 position, in Vector3 normal, Color color, float radius = 1.0f, float duration = 0, bool depthTest = true)
    {
        var up = normal.normalized * radius;
        var forward = Vector3.Slerp(up, -up, 0.5f);
        var right = Vector3.Cross(up, forward).normalized * radius;

        var matrix = new Matrix4x4
        {
            [0] = right.x,
            [1] = right.y,
            [2] = right.z,
            [4] = up.x,
            [5] = up.y,
            [6] = up.z,
            [8] = forward.x,
            [9] = forward.y,
            [10] = forward.z
        };

        var lastPoint = position + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
        var nextPoint = Vector3.zero;

        color = (color == default(Color)) ? Color.white : color;

        for (var i = 0; i < 91; i++)
        {
            nextPoint.x = Mathf.Cos((i * 4) * Mathf.Deg2Rad);
            nextPoint.z = Mathf.Sin((i * 4) * Mathf.Deg2Rad);
            nextPoint.y = 0;

            nextPoint = position + matrix.MultiplyPoint3x4(nextPoint);

            Debug.DrawLine(lastPoint, nextPoint, color, duration, depthTest);
            lastPoint = nextPoint;
        }
    }

    /// <summary>
    /// 	- Debugs a circle.
    /// </summary>
    /// <param name='position'>
    /// 	- Where the center of the circle will be positioned.
    /// </param>
    /// <param name='color'>
    /// 	- The color of the circle.
    /// </param>
    /// <param name='radius'>
    /// 	- The radius of the circle.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the circle.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not the circle should be faded when behind other objects.
    /// </param>
    public static void DebugCircle(in Vector3 position, in Color color, float radius = 1.0f, float duration = 0, bool depthTest = true)
    {
        DebugCircle(position, Vector3.up, color, radius, duration, depthTest);
    }

    /// <summary>
    /// 	- Debugs a circle.
    /// </summary>
    /// <param name='position'>
    /// 	- Where the center of the circle will be positioned.
    /// </param>
    /// <param name='up'>
    /// 	- The direction perpendicular to the surface of the circle.
    /// </param>
    /// <param name='radius'>
    /// 	- The radius of the circle.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the circle.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not the circle should be faded when behind other objects.
    /// </param>
    public static void DebugCircle(in Vector3 position, in Vector3 up, float radius = 1.0f, float duration = 0, bool depthTest = true)
    {
        DebugCircle(position, up, Color.white, radius, duration, depthTest);
    }

    /// <summary>
    /// 	- Debugs a circle.
    /// </summary>
    /// <param name='position'>
    /// 	- Where the center of the circle will be positioned.
    /// </param>
    /// <param name='radius'>
    /// 	- The radius of the circle.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the circle.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not the circle should be faded when behind other objects.
    /// </param>
    public static void DebugCircle(in Vector3 position, float radius = 1.0f, float duration = 0, bool depthTest = true)
    {
        DebugCircle(position, Vector3.up, Color.white, radius, duration, depthTest);
    }

    /// <summary>
    /// 	- Debugs a wire sphere.
    /// </summary>
    /// <param name='position'>
    /// 	- The position of the center of the sphere.
    /// </param>
    /// <param name='color'>
    /// 	- The color of the sphere.
    /// </param>
    /// <param name='radius'>
    /// 	- The radius of the sphere.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the sphere.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not the sphere should be faded when behind other objects.
    /// </param>
    public static void DebugWireSphere(in Vector3 position, in Color color, float radius = 1.0f, float duration = 0, bool depthTest = true)
    {
        var angle = 10.0f;

        var x = new Vector3(position.x, position.y + radius * Mathf.Sin(0), position.z + radius * Mathf.Cos(0));
        var y = new Vector3(position.x + radius * Mathf.Cos(0), position.y, position.z + radius * Mathf.Sin(0));
        var z = new Vector3(position.x + radius * Mathf.Cos(0), position.y + radius * Mathf.Sin(0), position.z);

        for (var i = 1; i < 37; i++)
        {
            var newX = new Vector3(position.x, position.y + radius * Mathf.Sin(angle * i * Mathf.Deg2Rad), position.z + radius * Mathf.Cos(angle * i * Mathf.Deg2Rad));
            var newY = new Vector3(position.x + radius * Mathf.Cos(angle * i * Mathf.Deg2Rad), position.y, position.z + radius * Mathf.Sin(angle * i * Mathf.Deg2Rad));
            var newZ = new Vector3(position.x + radius * Mathf.Cos(angle * i * Mathf.Deg2Rad), position.y + radius * Mathf.Sin(angle * i * Mathf.Deg2Rad), position.z);

            Debug.DrawLine(x, newX, color, duration, depthTest);
            Debug.DrawLine(y, newY, color, duration, depthTest);
            Debug.DrawLine(z, newZ, color, duration, depthTest);

            x = newX;
            y = newY;
            z = newZ;
        }
    }

    /// <summary>
    /// 	- Debugs a wire sphere.
    /// </summary>
    /// <param name='position'>
    /// 	- The position of the center of the sphere.
    /// </param>
    /// <param name='radius'>
    /// 	- The radius of the sphere.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the sphere.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not the sphere should be faded when behind other objects.
    /// </param>
    public static void DebugWireSphere(in Vector3 position, float radius = 1.0f, float duration = 0, bool depthTest = true)
    {
        DebugWireSphere(position, Color.white, radius, duration, depthTest);
    }

    /// <summary>
    /// 	- Debugs a cylinder.
    /// </summary>
    /// <param name='start'>
    /// 	- The position of one end of the cylinder.
    /// </param>
    /// <param name='end'>
    /// 	- The position of the other end of the cylinder.
    /// </param>
    /// <param name='color'>
    /// 	- The color of the cylinder.
    /// </param>
    /// <param name='radius'>
    /// 	- The radius of the cylinder.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the cylinder.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not the cylinder should be faded when behind other objects.
    /// </param>
    public static void DebugCylinder(in Vector3 start, in Vector3 end, in Color color, float radius = 1, float duration = 0, bool depthTest = true)
    {
        var up = (end - start).normalized * radius;
        var forward = Vector3.Slerp(up, -up, 0.5f);
        var right = Vector3.Cross(up, forward).normalized * radius;

        //Radial circles
        DebugCircle(start, up, color, radius, duration, depthTest);
        DebugCircle(end, -up, color, radius, duration, depthTest);
        DebugCircle((start + end) * 0.5f, up, color, radius, duration, depthTest);

        //Side lines
        Debug.DrawLine(start + right, end + right, color, duration, depthTest);
        Debug.DrawLine(start - right, end - right, color, duration, depthTest);

        Debug.DrawLine(start + forward, end + forward, color, duration, depthTest);
        Debug.DrawLine(start - forward, end - forward, color, duration, depthTest);

        //Start end cap
        Debug.DrawLine(start - right, start + right, color, duration, depthTest);
        Debug.DrawLine(start - forward, start + forward, color, duration, depthTest);

        //End end cap
        Debug.DrawLine(end - right, end + right, color, duration, depthTest);
        Debug.DrawLine(end - forward, end + forward, color, duration, depthTest);
    }

    /// <summary>
    /// 	- Debugs a cylinder.
    /// </summary>
    /// <param name='start'>
    /// 	- The position of one end of the cylinder.
    /// </param>
    /// <param name='end'>
    /// 	- The position of the other end of the cylinder.
    /// </param>
    /// <param name='radius'>
    /// 	- The radius of the cylinder.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the cylinder.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not the cylinder should be faded when behind other objects.
    /// </param>
    public static void DebugCylinder(in Vector3 start, in Vector3 end, float radius = 1, float duration = 0, bool depthTest = true)
    {
        DebugCylinder(start, end, Color.white, radius, duration, depthTest);
    }

    /// <summary>
    /// 	- Debugs a cone.
    /// </summary>
    /// <param name='position'>
    /// 	- The position for the tip of the cone.
    /// </param>
    /// <param name='direction'>
    /// 	- The direction for the cone gets wider in.
    /// </param>
    /// <param name='angle'>
    /// 	- The angle of the cone.
    /// </param>
    /// <param name='color'>
    /// 	- The color of the cone.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the cone.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not the cone should be faded when behind other objects.
    /// </param>
    public static void DebugCone(in Vector3 position, in Vector3 direction, in Color color, float angle = 45, float duration = 0, bool depthTest = true)
    {
        var length = direction.magnitude;
        var d = direction / length;

        var forward = d;
        var up = Vector3.Slerp(forward, -forward, 0.5f);
        var right = Vector3.Cross(forward, up).normalized * length;

        var slerpVector = Vector3.Slerp(forward, up, angle / 90.0f);

        var farPlane = new Plane(-d, position + forward);
        var distRay = new Ray(position, slerpVector);

        farPlane.Raycast(distRay, out var dist);

        Debug.DrawRay(position, slerpVector.normalized * dist, color);
        Debug.DrawRay(position, Vector3.Slerp(forward, -up, angle / 90.0f).normalized * dist, color, duration, depthTest);
        Debug.DrawRay(position, Vector3.Slerp(forward, right, angle / 90.0f).normalized * dist, color, duration, depthTest);
        Debug.DrawRay(position, Vector3.Slerp(forward, -right, angle / 90.0f).normalized * dist, color, duration, depthTest);

        DebugCircle(position + forward, direction, color, (forward - (slerpVector.normalized * dist)).magnitude, duration, depthTest);
        DebugCircle(position + (forward * 0.5f), direction, color, ((forward * 0.5f) - (slerpVector.normalized * (dist * 0.5f))).magnitude, duration, depthTest);
    }

    /// <summary>
    /// 	- Debugs a cone.
    /// </summary>
    /// <param name='position'>
    /// 	- The position for the tip of the cone.
    /// </param>
    /// <param name='direction'>
    /// 	- The direction for the cone gets wider in.
    /// </param>
    /// <param name='angle'>
    /// 	- The angle of the cone.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the cone.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not the cone should be faded when behind other objects.
    /// </param>
    public static void DebugCone(in Vector3 position, in Vector3 direction, float angle = 45, float duration = 0, bool depthTest = true)
    {
        DebugCone(position, direction, Color.white, angle, duration, depthTest);
    }

    /// <summary>
    /// 	- Debugs a cone.
    /// </summary>
    /// <param name='position'>
    /// 	- The position for the tip of the cone.
    /// </param>
    /// <param name='angle'>
    /// 	- The angle of the cone.
    /// </param>
    /// <param name='color'>
    /// 	- The color of the cone.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the cone.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not the cone should be faded when behind other objects.
    /// </param>
    public static void DebugCone(in Vector3 position, in Color color, float angle = 45, float duration = 0, bool depthTest = true)
    {
        DebugCone(position, Vector3.up, color, angle, duration, depthTest);
    }

    /// <summary>
    /// 	- Debugs a cone.
    /// </summary>
    /// <param name='position'>
    /// 	- The position for the tip of the cone.
    /// </param>
    /// <param name='angle'>
    /// 	- The angle of the cone.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the cone.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not the cone should be faded when behind other objects.
    /// </param>
    public static void DebugCone(in Vector3 position, float angle = 45, float duration = 0, bool depthTest = true)
    {
        DebugCone(position, Vector3.up, Color.white, angle, duration, depthTest);
    }

    /// <summary>
    /// 	- Debugs an arrow.
    /// </summary>
    /// <param name='position'>
    /// 	- The start position of the arrow.
    /// </param>
    /// <param name='direction'>
    /// 	- The direction the arrow will point in.
    /// </param>
    /// <param name='color'>
    /// 	- The color of the arrow.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the arrow.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not the arrow should be faded when behind other objects. 
    /// </param>
    public static void DebugArrow(in Vector3 position, in Vector3 direction, in Color color, float duration = 0, bool depthTest = true)
    {
        Debug.DrawRay(position, direction, color, duration, depthTest);
        DebugExtension.DebugCone(position + direction, -direction * 0.333f, color, 15, duration, depthTest);
    }

    /// <summary>
    /// 	- Debugs an arrow.
    /// </summary>
    /// <param name='position'>
    /// 	- The start position of the arrow.
    /// </param>
    /// <param name='direction'>
    /// 	- The direction the arrow will point in.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the arrow.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not the arrow should be faded when behind other objects. 
    /// </param>
    public static void DebugArrow(in Vector3 position, in Vector3 direction, float duration = 0, bool depthTest = true)
    {
        DebugArrow(position, direction, Color.white, duration, depthTest);
    }

    /// <summary>
    /// 	- Debugs a capsule.
    /// </summary>
    /// <param name='start'>
    /// 	- The position of one end of the capsule.
    /// </param>
    /// <param name='end'>
    /// 	- The position of the other end of the capsule.
    /// </param>
    /// <param name='color'>
    /// 	- The color of the capsule.
    /// </param>
    /// <param name='radius'>
    /// 	- The radius of the capsule.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the capsule.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not the capsule should be faded when behind other objects.
    /// </param>
    public static void DebugCapsule(in Vector3 start, in Vector3 end, in Color color, float radius = 1, float duration = 0, bool depthTest = true)
    {
        var up = (end - start).normalized * radius;
        var forward = Vector3.Slerp(up, -up, 0.5f);
        var right = Vector3.Cross(up, forward).normalized * radius;

        var height = (start - end).magnitude;
        var sideLength = Mathf.Max(0, (height * 0.5f) - radius);
        var middle = (end + start) * 0.5f;

        var s = middle + ((start - middle).normalized * sideLength);
        var e = middle + ((end - middle).normalized * sideLength);

        //Radial circles
        DebugCircle(s, up, color, radius, duration, depthTest);
        DebugCircle(e, -up, color, radius, duration, depthTest);

        //Side lines
        Debug.DrawLine(s + right, e + right, color, duration, depthTest);
        Debug.DrawLine(s - right, e - right, color, duration, depthTest);

        Debug.DrawLine(s + forward, e + forward, color, duration, depthTest);
        Debug.DrawLine(s - forward, e - forward, color, duration, depthTest);

        for (int i = 1; i < 26; i++)
        {
            //Start end cap
            Debug.DrawLine(Vector3.Slerp(right, -up, i / 25.0f) + s, Vector3.Slerp(right, -up, (i - 1) / 25.0f) + s, color, duration, depthTest);
            Debug.DrawLine(Vector3.Slerp(-right, -up, i / 25.0f) + s, Vector3.Slerp(-right, -up, (i - 1) / 25.0f) + s, color, duration, depthTest);
            Debug.DrawLine(Vector3.Slerp(forward, -up, i / 25.0f) + s, Vector3.Slerp(forward, -up, (i - 1) / 25.0f) + s, color, duration, depthTest);
            Debug.DrawLine(Vector3.Slerp(-forward, -up, i / 25.0f) + s, Vector3.Slerp(-forward, -up, (i - 1) / 25.0f) + s, color, duration, depthTest);

            //End end cap
            Debug.DrawLine(Vector3.Slerp(right, up, i / 25.0f) + e, Vector3.Slerp(right, up, (i - 1) / 25.0f) + e, color, duration, depthTest);
            Debug.DrawLine(Vector3.Slerp(-right, up, i / 25.0f) + e, Vector3.Slerp(-right, up, (i - 1) / 25.0f) + e, color, duration, depthTest);
            Debug.DrawLine(Vector3.Slerp(forward, up, i / 25.0f) + e, Vector3.Slerp(forward, up, (i - 1) / 25.0f) + e, color, duration, depthTest);
            Debug.DrawLine(Vector3.Slerp(-forward, up, i / 25.0f) + e, Vector3.Slerp(-forward, up, (i - 1) / 25.0f) + e, color, duration, depthTest);
        }
    }

    /// <summary>
    /// 	- Debugs a capsule.
    /// </summary>
    /// <param name='start'>
    /// 	- The position of one end of the capsule.
    /// </param>
    /// <param name='end'>
    /// 	- The position of the other end of the capsule.
    /// </param>
    /// <param name='radius'>
    /// 	- The radius of the capsule.
    /// </param>
    /// <param name='duration'>
    /// 	- How long to draw the capsule.
    /// </param>
    /// <param name='depthTest'>
    /// 	- Whether or not the capsule should be faded when behind other objects.
    /// </param>
    public static void DebugCapsule(in Vector3 start, in Vector3 end, float radius = 1, float duration = 0, bool depthTest = true)
    {
        DebugCapsule(start, end, Color.white, radius, duration, depthTest);
    }

    #endregion

    #region GizmoDrawFunctions

    /// <summary>
    /// 	- Draws a point.
    /// </summary>
    /// <param name='position'>
    /// 	- The point to draw.
    /// </param>
    ///  <param name='color'>
    /// 	- The color of the drawn point.
    /// </param>
    /// <param name='scale'>
    /// 	- The size of the drawn point.
    /// </param>
    public static void DrawPoint(in Vector3 position, in Color color, float scale = 1.0f)
    {
        var oldColor = Gizmos.color;

        Gizmos.color = color;
        Gizmos.DrawRay(position + (Vector3.up * (scale * 0.5f)), -Vector3.up * scale);
        Gizmos.DrawRay(position + (Vector3.right * (scale * 0.5f)), -Vector3.right * scale);
        Gizmos.DrawRay(position + (Vector3.forward * (scale * 0.5f)), -Vector3.forward * scale);

        Gizmos.color = oldColor;
    }

    /// <summary>
    /// 	- Draws a point.
    /// </summary>
    /// <param name='position'>
    /// 	- The point to draw.
    /// </param>
    /// <param name='scale'>
    /// 	- The size of the drawn point.
    /// </param>
    public static void DrawPoint(in Vector3 position, float scale = 1.0f)
    {
        DrawPoint(position, Color.white, scale);
    }

    /// <summary>
    /// 	- Draws an axis-aligned bounding box.
    /// </summary>
    /// <param name='bounds'>
    /// 	- The bounds to draw.
    /// </param>
    /// <param name='color'>
    /// 	- The color of the bounds.
    /// </param>
    public static void DrawBounds(in Bounds bounds, in Color color)
    {
        var center = bounds.center;

        var x = bounds.extents.x;
        var y = bounds.extents.y;
        var z = bounds.extents.z;

        var ruf = center + new Vector3(x, y, z);
        var rub = center + new Vector3(x, y, -z);
        var luf = center + new Vector3(-x, y, z);
        var lub = center + new Vector3(-x, y, -z);

        var rdf = center + new Vector3(x, -y, z);
        var rdb = center + new Vector3(x, -y, -z);
        var lfd = center + new Vector3(-x, -y, z);
        var lbd = center + new Vector3(-x, -y, -z);

        var oldColor = Gizmos.color;
        Gizmos.color = color;

        Gizmos.DrawLine(ruf, luf);
        Gizmos.DrawLine(ruf, rub);
        Gizmos.DrawLine(luf, lub);
        Gizmos.DrawLine(rub, lub);

        Gizmos.DrawLine(ruf, rdf);
        Gizmos.DrawLine(rub, rdb);
        Gizmos.DrawLine(luf, lfd);
        Gizmos.DrawLine(lub, lbd);

        Gizmos.DrawLine(rdf, lfd);
        Gizmos.DrawLine(rdf, rdb);
        Gizmos.DrawLine(lfd, lbd);
        Gizmos.DrawLine(lbd, rdb);

        Gizmos.color = oldColor;
    }

    /// <summary>
    /// 	- Draws an axis-aligned bounding box.
    /// </summary>
    /// <param name='bounds'>
    /// 	- The bounds to draw.
    /// </param>
    public static void DrawBounds(in Bounds bounds)
    {
        DrawBounds(bounds, Color.white);
    }

    /// <summary>
    /// 	- Draws a local cube.
    /// </summary>
    /// <param name='transform'>
    /// 	- The transform the cube will be local to.
    /// </param>
    /// <param name='size'>
    /// 	- The local size of the cube.
    /// </param>
    /// <param name='center'>
    ///		- The local position of the cube.
    /// </param>
    /// <param name='color'>
    /// 	- The color of the cube.
    /// </param>
    public static void DrawLocalCube(Transform transform,in Vector3 size, in Color color, in Vector3 center = default)
    {
        var oldColor = Gizmos.color;
        Gizmos.color = color;

        var lbb = transform.TransformPoint(center + ((-size) * 0.5f));
        var rbb = transform.TransformPoint(center + (new Vector3(size.x, -size.y, -size.z) * 0.5f));

        var lbf = transform.TransformPoint(center + (new Vector3(size.x, -size.y, size.z) * 0.5f));
        var rbf = transform.TransformPoint(center + (new Vector3(-size.x, -size.y, size.z) * 0.5f));

        var lub = transform.TransformPoint(center + (new Vector3(-size.x, size.y, -size.z) * 0.5f));
        var rub = transform.TransformPoint(center + (new Vector3(size.x, size.y, -size.z) * 0.5f));

        var luf = transform.TransformPoint(center + ((size) * 0.5f));
        var ruf = transform.TransformPoint(center + (new Vector3(-size.x, size.y, size.z) * 0.5f));

        Gizmos.DrawLine(lbb, rbb);
        Gizmos.DrawLine(rbb, lbf);
        Gizmos.DrawLine(lbf, rbf);
        Gizmos.DrawLine(rbf, lbb);

        Gizmos.DrawLine(lub, rub);
        Gizmos.DrawLine(rub, luf);
        Gizmos.DrawLine(luf, ruf);
        Gizmos.DrawLine(ruf, lub);

        Gizmos.DrawLine(lbb, lub);
        Gizmos.DrawLine(rbb, rub);
        Gizmos.DrawLine(lbf, luf);
        Gizmos.DrawLine(rbf, ruf);

        Gizmos.color = oldColor;
    }

    /// <summary>
    /// 	- Draws a local cube.
    /// </summary>
    /// <param name='transform'>
    /// 	- The transform the cube will be local to.
    /// </param>
    /// <param name='size'>
    /// 	- The local size of the cube.
    /// </param>
    /// <param name='center'>
    ///		- The local position of the cube.
    /// </param>	
    public static void DrawLocalCube(Transform transform, in Vector3 size, in Vector3 center = default)
    {
        DrawLocalCube(transform, size, Color.white, center);
    }

    /// <summary>
    /// 	- Draws a local cube.
    /// </summary>
    /// <param name='space'>
    /// 	- The space the cube will be local to.
    /// </param>
    /// <param name='size'>
    /// 	- The local size of the cube.
    /// </param>
    /// <param name='center'>
    /// 	- The local position of the cube.
    /// </param>
    /// <param name='color'>
    /// 	- The color of the cube.
    /// </param>
    public static void DrawLocalCube(Matrix4x4 space, in Vector3 size, in Color color, Vector3 center = default)
    {
        var oldColor = Gizmos.color;
        Gizmos.color = color;

        var lbb = space.MultiplyPoint3x4(center + ((-size) * 0.5f));
        var rbb = space.MultiplyPoint3x4(center + (new Vector3(size.x, -size.y, -size.z) * 0.5f));

        var lbf = space.MultiplyPoint3x4(center + (new Vector3(size.x, -size.y, size.z) * 0.5f));
        var rbf = space.MultiplyPoint3x4(center + (new Vector3(-size.x, -size.y, size.z) * 0.5f));

        var lub = space.MultiplyPoint3x4(center + (new Vector3(-size.x, size.y, -size.z) * 0.5f));
        var rub = space.MultiplyPoint3x4(center + (new Vector3(size.x, size.y, -size.z) * 0.5f));

        var luf = space.MultiplyPoint3x4(center + ((size) * 0.5f));
        var ruf = space.MultiplyPoint3x4(center + (new Vector3(-size.x, size.y, size.z) * 0.5f));

        Gizmos.DrawLine(lbb, rbb);
        Gizmos.DrawLine(rbb, lbf);
        Gizmos.DrawLine(lbf, rbf);
        Gizmos.DrawLine(rbf, lbb);

        Gizmos.DrawLine(lub, rub);
        Gizmos.DrawLine(rub, luf);
        Gizmos.DrawLine(luf, ruf);
        Gizmos.DrawLine(ruf, lub);

        Gizmos.DrawLine(lbb, lub);
        Gizmos.DrawLine(rbb, rub);
        Gizmos.DrawLine(lbf, luf);
        Gizmos.DrawLine(rbf, ruf);

        Gizmos.color = oldColor;
    }

    /// <summary>
    /// 	- Draws a local cube.
    /// </summary>
    /// <param name='space'>
    /// 	- The space the cube will be local to.
    /// </param>
    /// <param name='size'>
    /// 	- The local size of the cube.
    /// </param>
    /// <param name='center'>
    /// 	- The local position of the cube.
    /// </param>
    public static void DrawLocalCube(in Matrix4x4 space, in Vector3 size, in Vector3 center = default)
    {
        DrawLocalCube(space, size, Color.white, center);
    }

    /// <summary>
    /// 	- Draws a circle.
    /// </summary>
    /// <param name='position'>
    /// 	- Where the center of the circle will be positioned.
    /// </param>
    /// <param name='normal'>
    /// 	- The direction perpendicular to the surface of the circle.
    /// </param>
    /// <param name='color'>
    /// 	- The color of the circle.
    /// </param>
    /// <param name='radius'>
    /// 	- The radius of the circle.
    /// </param>
    public static void DrawCircle(in Vector3 position, in Vector3 normal, in Color color, float radius = 1.0f)
    {
        var up = ((normal == Vector3.zero) ? Vector3.up : normal).normalized * radius;
        var forward = Vector3.Slerp(up, -up, 0.5f);
        var right = Vector3.Cross(up, forward).normalized * radius;

        var matrix = new Matrix4x4
        {
            [0] = right.x,
            [1] = right.y,
            [2] = right.z,
            [4] = up.x,
            [5] = up.y,
            [6] = up.z,
            [8] = forward.x,
            [9] = forward.y,
            [10] = forward.z
        };

        var lastPoint = position + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
        var nextPoint = Vector3.zero;

        var oldColor = Gizmos.color;
        Gizmos.color = (color == default(Color)) ? Color.white : color;

        for (var i = 0; i < 91; i++)
        {
            nextPoint.x = Mathf.Cos((i * 4) * Mathf.Deg2Rad);
            nextPoint.z = Mathf.Sin((i * 4) * Mathf.Deg2Rad);
            nextPoint.y = 0;

            nextPoint = position + matrix.MultiplyPoint3x4(nextPoint);

            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }

        Gizmos.color = oldColor;
    }

    /// <summary>
    /// 	- Draws a circle.
    /// </summary>
    /// <param name='position'>
    /// 	- Where the center of the circle will be positioned.
    /// </param>
    /// <param name='color'>
    /// 	- The color of the circle.
    /// </param>
    /// <param name='radius'>
    /// 	- The radius of the circle.
    /// </param>
    public static void DrawCircle(in Vector3 position, in Color color, float radius = 1.0f)
    {
        DrawCircle(position, Vector3.up, color, radius);
    }

    /// <summary>
    /// 	- Draws a circle.
    /// </summary>
    /// <param name='position'>
    /// 	- Where the center of the circle will be positioned.
    /// </param>
    /// <param name='up'>
    /// 	- The direction perpendicular to the surface of the circle.
    /// </param>
    /// <param name='radius'>
    /// 	- The radius of the circle.
    /// </param>
    public static void DrawCircle(in Vector3 position, in Vector3 up, float radius = 1.0f)
    {
        DrawCircle(position, position, Color.white, radius);
    }

    /// <summary>
    /// 	- Draws a circle.
    /// </summary>
    /// <param name='position'>
    /// 	- Where the center of the circle will be positioned.
    /// </param>
    /// <param name='radius'>
    /// 	- The radius of the circle.
    /// </param>
    public static void DrawCircle(in Vector3 position, float radius = 1.0f)
    {
        DrawCircle(position, Vector3.up, Color.white, radius);
    }

    // Wire sphere already exists

    /// <summary>
    /// 	- Draws a cylinder.
    /// </summary>
    /// <param name='start'>
    /// 	- The position of one end of the cylinder.
    /// </param>
    /// <param name='end'>
    /// 	- The position of the other end of the cylinder.
    /// </param>
    /// <param name='color'>
    /// 	- The color of the cylinder.
    /// </param>
    /// <param name='radius'>
    /// 	- The radius of the cylinder.
    /// </param>
    public static void DrawCylinder(in Vector3 start, in Vector3 end, in Color color, float radius = 1.0f)
    {
        var up = (end - start).normalized * radius;
        var forward = Vector3.Slerp(up, -up, 0.5f);
        var right = Vector3.Cross(up, forward).normalized * radius;

        //Radial circles
        DebugExtension.DrawCircle(start, up, color, radius);
        DebugExtension.DrawCircle(end, -up, color, radius);
        DebugExtension.DrawCircle((start + end) * 0.5f, up, color, radius);

        var oldColor = Gizmos.color;
        Gizmos.color = color;

        //Side lines
        Gizmos.DrawLine(start + right, end + right);
        Gizmos.DrawLine(start - right, end - right);

        Gizmos.DrawLine(start + forward, end + forward);
        Gizmos.DrawLine(start - forward, end - forward);

        //Start end cap
        Gizmos.DrawLine(start - right, start + right);
        Gizmos.DrawLine(start - forward, start + forward);

        //End end cap
        Gizmos.DrawLine(end - right, end + right);
        Gizmos.DrawLine(end - forward, end + forward);

        Gizmos.color = oldColor;
    }

    /// <summary>
    /// 	- Draws a cylinder.
    /// </summary>
    /// <param name='start'>
    /// 	- The position of one end of the cylinder.
    /// </param>
    /// <param name='end'>
    /// 	- The position of the other end of the cylinder.
    /// </param>
    /// <param name='radius'>
    /// 	- The radius of the cylinder.
    /// </param>
    public static void DrawCylinder(in Vector3 start, in Vector3 end, float radius = 1.0f)
    {
        DrawCylinder(start, end, Color.white, radius);
    }

    /// <summary>
    /// 	- Draws a cone.
    /// </summary>
    /// <param name='position'>
    /// 	- The position for the tip of the cone.
    /// </param>
    /// <param name='direction'>
    /// 	- The direction for the cone to get wider in.
    /// </param>
    /// <param name='color'>
    /// 	- The color of the cone.
    /// </param>
    /// <param name='angle'>
    /// 	- The angle of the cone.
    /// </param>
    public static void DrawCone(in Vector3 position, in Vector3 direction, in Color color, float angle = 45)
    {
        var length = direction.magnitude;

        var forward = direction;
        var up = Vector3.Slerp(forward, -forward, 0.5f);
        var right = Vector3.Cross(forward, up).normalized * length;
        var normal = direction.normalized;

        var slerpVector = Vector3.Slerp(forward, up, angle / 90.0f);

        var farPlane = new Plane(-normal, position + forward);
        var distRay = new Ray(position, slerpVector);

        farPlane.Raycast(distRay, out var dist);

        var oldColor = Gizmos.color;
        Gizmos.color = color;

        Gizmos.DrawRay(position, slerpVector.normalized * dist);
        Gizmos.DrawRay(position, Vector3.Slerp(forward, -up, angle / 90.0f).normalized * dist);
        Gizmos.DrawRay(position, Vector3.Slerp(forward, right, angle / 90.0f).normalized * dist);
        Gizmos.DrawRay(position, Vector3.Slerp(forward, -right, angle / 90.0f).normalized * dist);

        DrawCircle(position + forward, normal, color, (forward - (slerpVector.normalized * dist)).magnitude);
        DrawCircle(position + (forward * 0.5f), normal, color, ((forward * 0.5f) - (slerpVector.normalized * (dist * 0.5f))).magnitude);

        Gizmos.color = oldColor;
    }

    /// <summary>
    /// 	- Draws a cone.
    /// </summary>
    /// <param name='position'>
    /// 	- The position for the tip of the cone.
    /// </param>
    /// <param name='direction'>
    /// 	- The direction for the cone to get wider in.
    /// </param>
    /// <param name='angle'>
    /// 	- The angle of the cone.
    /// </param>
    public static void DrawCone(in Vector3 position, in Vector3 direction, float angle = 45)
    {
        DrawCone(position, direction, Color.white, angle);
    }

    /// <summary>
    /// 	- Draws a cone.
    /// </summary>
    /// <param name='position'>
    /// 	- The position for the tip of the cone.
    /// </param>
    /// <param name='color'>
    /// 	- The color of the cone.
    /// </param>
    /// <param name='angle'>
    /// 	- The angle of the cone.
    /// </param>
    public static void DrawCone(in Vector3 position, in Color color, float angle = 45)
    {
        DrawCone(position, Vector3.up, color, angle);
    }

    /// <summary>
    /// 	- Draws a cone.
    /// </summary>
    /// <param name='position'>
    /// 	- The position for the tip of the cone.
    /// </param>
    /// <param name='angle'>
    /// 	- The angle of the cone.
    /// </param>
    public static void DrawCone(in Vector3 position, float angle = 45)
    {
        DrawCone(position, Vector3.up, Color.white, angle);
    }

    /// <summary>
    /// 	- Draws an arrow.
    /// </summary>
    /// <param name='position'>
    /// 	- The start position of the arrow.
    /// </param>
    /// <param name='direction'>
    /// 	- The direction the arrow will point in.
    /// </param>
    /// <param name='color'>
    /// 	- The color of the arrow.
    /// </param>
    public static void DrawArrow(in Vector3 position, in Vector3 direction, in Color color)
    {
        var oldColor = Gizmos.color;
        Gizmos.color = color;

        Gizmos.DrawRay(position, direction);
        DrawCone(position + direction, -direction * 0.333f, color, 15);

        Gizmos.color = oldColor;
    }

    /// <summary>
    /// 	- Draws an arrow.
    /// </summary>
    /// <param name='position'>
    /// 	- The start position of the arrow.
    /// </param>
    /// <param name='direction'>
    /// 	- The direction the arrow will point in.
    /// </param>
    public static void DrawArrow(in Vector3 position, in Vector3 direction)
    {
        DrawArrow(position, direction, Color.white);
    }

    /// <summary>
    /// 	- Draws a capsule.
    /// </summary>
    /// <param name='start'>
    /// 	- The position of one end of the capsule.
    /// </param>
    /// <param name='end'>
    /// 	- The position of the other end of the capsule.
    /// </param>
    /// <param name='color'>
    /// 	- The color of the capsule.
    /// </param>
    /// <param name='radius'>
    /// 	- The radius of the capsule.
    /// </param>
    public static void DrawCapsule(in Vector3 start, in Vector3 end, in Color color, float radius = 1)
    {
        var up = (end - start).normalized * radius;
        var forward = Vector3.Slerp(up, -up, 0.5f);
        var right = Vector3.Cross(up, forward).normalized * radius;

        var oldColor = Gizmos.color;
        Gizmos.color = color;

        var height = (start - end).magnitude;
        var sideLength = Mathf.Max(0, (height * 0.5f) - radius);
        var middle = (end + start) * 0.5f;

        var s = middle + ((start - middle).normalized * sideLength);
        var e = middle + ((end - middle).normalized * sideLength);

        //Radial circles
        DrawCircle(s, up, color, radius);
        DrawCircle(e, -up, color, radius);

        //Side lines
        Gizmos.DrawLine(s + right, e + right);
        Gizmos.DrawLine(s - right, e - right);

        Gizmos.DrawLine(s + forward, e + forward);
        Gizmos.DrawLine(s - forward, e - forward);

        for (int i = 1; i < 26; i++)
        {
            //Start end cap
            Gizmos.DrawLine(Vector3.Slerp(right, -up, i / 25.0f) + s, Vector3.Slerp(right, -up, (i - 1) / 25.0f) + s);
            Gizmos.DrawLine(Vector3.Slerp(-right, -up, i / 25.0f) + s, Vector3.Slerp(-right, -up, (i - 1) / 25.0f) + s);
            Gizmos.DrawLine(Vector3.Slerp(forward, -up, i / 25.0f) + s, Vector3.Slerp(forward, -up, (i - 1) / 25.0f) + s);
            Gizmos.DrawLine(Vector3.Slerp(-forward, -up, i / 25.0f) + s, Vector3.Slerp(-forward, -up, (i - 1) / 25.0f) + s);

            //End end cap
            Gizmos.DrawLine(Vector3.Slerp(right, up, i / 25.0f) + e, Vector3.Slerp(right, up, (i - 1) / 25.0f) + e);
            Gizmos.DrawLine(Vector3.Slerp(-right, up, i / 25.0f) + e, Vector3.Slerp(-right, up, (i - 1) / 25.0f) + e);
            Gizmos.DrawLine(Vector3.Slerp(forward, up, i / 25.0f) + e, Vector3.Slerp(forward, up, (i - 1) / 25.0f) + e);
            Gizmos.DrawLine(Vector3.Slerp(-forward, up, i / 25.0f) + e, Vector3.Slerp(-forward, up, (i - 1) / 25.0f) + e);
        }

        Gizmos.color = oldColor;
    }

    /// <summary>
    /// 	- Draws a capsule.
    /// </summary>
    /// <param name='start'>
    /// 	- The position of one end of the capsule.
    /// </param>
    /// <param name='end'>
    /// 	- The position of the other end of the capsule.
    /// </param>
    /// <param name='radius'>
    /// 	- The radius of the capsule.
    /// </param>
    public static void DrawCapsule(in Vector3 start, in Vector3 end, float radius = 1)
    {
        DrawCapsule(start, end, Color.white, radius);
    }

    #endregion

    #region DebugFunctions

    /// <summary>
    /// 	- Gets the methods of an object.
    /// </summary>
    /// <returns>
    /// 	- A list of methods accessible from this object.
    /// </returns>
    /// <param name='obj'>
    /// 	- The object to get the methods of.
    /// </param>
    /// <param name='includeInfo'>
    /// 	- Whether or not to include each method's method info in the list.
    /// </param>
    public static string MethodsOfObject(object obj, bool includeInfo = false)
    {
        var methods = "";
        var methodInfos = obj.GetType().GetMethods();
        foreach (var t in methodInfos)
        {
            if (includeInfo)
            {
                methods += t + "\n";
            }

            else
            {
                methods += t.Name + "\n";
            }
        }

        return methods;
    }

    /// <summary>
    /// 	- Gets the methods of a type.
    /// </summary>
    /// <returns>
    /// 	- A list of methods accessible from this type.
    /// </returns>
    /// <param name='type'>
    /// 	- The type to get the methods of.
    /// </param>
    /// <param name='includeInfo'>
    /// 	- Whether or not to include each method's method info in the list.
    /// </param>
    public static string MethodsOfType(System.Type type, bool includeInfo = false)
    {
        var methods = "";
        var methodInfos = type.GetMethods();
        foreach (var t in methodInfos)
        {
            if (includeInfo)
            {
                methods += t + "\n";
            }

            else
            {
                methods += t.Name + "\n";
            }
        }

        return methods;
    }

    #endregion
}