using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlaneEx
{
    public enum SideOfPlane
    {
        Up,
        Down,
        On
    }
    
    private const float epsilon = 0.0001f;

    public static SideOfPlane SideOf(this Plane plane, in Vector3 pt)
    {
        // var p = plane.GetSide(pt);
        
        var result = Vector3.Dot(plane.normal, pt) + plane.distance;

        if (result > epsilon)
        {
            return SideOfPlane.Up;
        }
        
        if (result < -epsilon)
        {
            return SideOfPlane.Down;
        }

        return SideOfPlane.On;
    }
    
#if __UNITY_EDITOR
    public static void OnDebugDraw(in Color drawColor)
    {
        // NOTE -> Gizmos are only supported in the editor. We will keep these function
        // signatures for consistency however at final build, these will do nothing
        // TO/DO -> Should we throw a runtime exception if this function tried to get executed
        // at runtime?
        if (transRef == null)
            return;

        var prevColor = Gizmos.color;
        var prevMatrix = Gizmos.matrix;

        // TO-DO
        Gizmos.matrix = Matrix4x4.TRS(transRef.position, transRef.rotation, transRef.localScale);
        Gizmos.color = drawColor;

        Gizmos.DrawWireCube(Vector3.zero, new Vector3(1.0f, 0.0f, 1.0f));

        Gizmos.color = prevColor;
        Gizmos.matrix = prevMatrix;
    }
#endif
}
