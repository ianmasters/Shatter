using UnityEngine;

public static class MatrixEx
{
    public static Quaternion ExtractRotation(this Matrix4x4 matrix)
    {
        // Quaternion q;
        // q.w = Mathf.Sqrt(Mathf.Max(0,1 + matrix[0,0] + matrix[1,1]+ matrix[2,2])) / 2;
        // q.x = Mathf.Sqrt(Mathf.Max(0,1 + matrix[0,0] - matrix[1,1]- matrix[2,2])) / 2;
        // q.y = Mathf.Sqrt(Mathf.Max(0,1 - matrix[0,0] + matrix[1,1]- matrix[2,2])) / 2;
        // q.z = Mathf.Sqrt(Mathf.Max(0,1 - matrix[0,0] - matrix[1,1]+ matrix[2,2])) / 2;
        // q.x *= Mathf.Sign(q.x * (matrix[2,1] - matrix[1,2]));
        // q.y *= Mathf.Sign(q.y * (matrix[0,2] - matrix[2,0]));
        // q.z *= Mathf.Sign(q.z * (matrix[1,0] - matrix[0,1]));
        // return q;
        
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;
 
        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;
 
        return Quaternion.LookRotation(forward, upwards);
    }
 
    public static Vector3 ExtractX(this Matrix4x4 matrix)
    {
        Vector3 x;
        x.x = matrix.m00;
        x.y = matrix.m10;
        x.z = matrix.m20;
        return x;
    }

    public static Vector3 ExtractY(this Matrix4x4 matrix)
    {
        Vector3 y;
        y.x = matrix.m01;
        y.y = matrix.m11;
        y.z = matrix.m21;
        return y;
    }
    
    public static Vector3 ExtractZ(this Matrix4x4 matrix)
    {
        Vector3 z;
        z.x = matrix.m02;
        z.y = matrix.m12;
        z.z = matrix.m22;
        return z;
    }
    
    public static Vector3 ExtractPosition(this Matrix4x4 matrix)
    {
        Vector3 position;
        position.x = matrix.m03;
        position.y = matrix.m13;
        position.z = matrix.m23;
        return position;
    }

    public static void SetPosition(this Matrix4x4 matrix, float x, float y, float z)
    {
        matrix.m03 = x;
        matrix.m13 = y;
        matrix.m23 = z;
    }
    
    public static void SetPosition(this Matrix4x4 matrix, Vector3 position)
    {
        matrix.m03 = position.x;
        matrix.m13 = position.y;
        matrix.m23 = position.z;
    }

    // public static void SetScale(this Matrix4x4 matrix, Vector3 scale)
    // {
    //     var x = matrix.ExtractX();
    //     var y = matrix.ExtractY();
    //     var z = matrix.ExtractZ();
    //     var lx = x.magnitude;
    //     var ly = y.magnitude;
    //     var lz = z.magnitude;
    //     matrix.m00 = x.x * scale.x / lx;
    //     matrix.m10 = x.x * scale.x / lx;
    //     matrix.m20 = x.x * scale.x / lx;
    // }

    public static Vector3 ExtractScale(this Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }

    public static Matrix4x4 RotateX(float angle)
    {
        return Matrix4x4.Rotate(Quaternion.Euler(angle, 0, 0));
    }
    public static Matrix4x4 RotateY(float angle)
    {
        return Matrix4x4.Rotate(Quaternion.Euler(0, angle, 0));
    }
    public static Matrix4x4 RotateZ(float angle)
    {
        return Matrix4x4.Rotate(Quaternion.Euler(0, 0,angle));
    }

    public static Matrix4x4 Translate(float x, float y, float z)
    {
        return Matrix4x4.Translate(new Vector3(x, y, z));
    }
}
