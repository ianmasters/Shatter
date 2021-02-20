using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Extensions
{
    // Use sharedMesh so not to make a copy
    public static class MeshEx
    {
        // Use sharedMesh so not to make a copy
        public static float CalculateVolume(this Mesh mesh, in Vector3[] vertices)
        {
            var volume = VolumeOfMesh(mesh, vertices);
            // Debug.Log($"The volume of the mesh is {volume}");
            return volume;
        }

        private static float SignedVolumeOfTriangle(in Vector3 p1, in Vector3 p2, in Vector3 p3)
        {
            var v321 = p3.x * p2.y * p1.z;
            var v231 = p2.x * p3.y * p1.z;
            var v312 = p3.x * p1.y * p2.z;
            var v132 = p1.x * p3.y * p2.z;
            var v213 = p2.x * p1.y * p3.z;
            var v123 = p1.x * p2.y * p3.z;
            // 1/6 = 0.166666666666667
            return 0.166666666666667f * (v231 - v321 + v312 - v132 - v213 + v123);
        }

        private static float VolumeOfMesh(in Mesh mesh, in Vector3[] vertices)
        {
            Debug.Assert(mesh, $"No mesh to operate on with VolumeOfMesh");
            var volume = 0f;
            var triangles = mesh.triangles;
            for (var i = 0; i < triangles.Length; i += 3)
            {
                volume += SignedVolumeOfTriangle(
                    vertices[triangles[i]],
                    vertices[triangles[i + 1]],
                    vertices[triangles[i + 2]]);
            }
            return Mathf.Abs(volume);
        }
    }
}