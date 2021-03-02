using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace EzySlice
{
    /**
     * Contains methods for slicing GameObjects
     */
    internal static class Slicer
    {
        /**
         * An internal class for storing internal sub mesh values
         */
        private sealed class SlicedSubMesh
        {
            public readonly List<Triangle> upperHull = new List<Triangle>();
            public readonly List<Triangle> lowerHull = new List<Triangle>();

            /**
             * Check if the sub mesh has had any UV's added.
             * NOTE -> This should be supported properly
             */
            public bool HasUV =>
                // what is this abomination??
                upperHull.Count > 0 ? upperHull[0].HasUV : lowerHull.Count > 0 && lowerHull[0].HasUV;

            /**
             * Check if the sub mesh has had any Normals added.
             * NOTE -> This should be supported properly
             */
            public bool HasNormal =>
                // what is this abomination??
                upperHull.Count > 0 ? upperHull[0].hasNormal : lowerHull.Count > 0 && lowerHull[0].hasNormal;

            /**
             * Check if the sub mesh has had any Tangents added.
             * NOTE -> This should be supported properly
             */
            public bool HasTangent =>
                // what is this abomination??
                upperHull.Count > 0 ? upperHull[0].hasTangent : lowerHull.Count > 0 && lowerHull[0].hasTangent;

            /**
             * Check if proper slicing has occured for this sub mesh. Slice occured if there
             * are triangles in both the upper and lower hulls
             */
            public bool IsValid => upperHull.Count > 0 && lowerHull.Count > 0;
        }

        /**
         * Helper function to accept a GameObject which will transform the plane
         * appropriately before the shatterCount occurs
         * See -> Slice(Mesh, Plane) for more info
         */
        public static SlicedHull Slice(in GameObject obj, in Plane plane, in TextureRegion crossRegion, in Material crossMaterial)
        {
            var filter = obj.GetComponent<MeshFilter>();

            // cannot continue without a proper filter
            if (!filter)
            {
                Debug.LogWarning("EzySlice::Slice -> Provided GameObject must have a MeshFilter Component.");

                return null;
            }

            var renderer = obj.GetComponent<MeshRenderer>();

            // cannot continue without a proper renderer
            if (!renderer)
            {
                Debug.LogWarning("EzySlice::Slice -> Provided GameObject must have a MeshRenderer Component.");

                return null;
            }

            // TODO: only a copy here currently works. Pass in a copy?
#if !POO
            var materials = renderer.sharedMaterials;
#else
            Material[] materials;
            if (Application.isPlaying)
                materials = renderer.sharedMaterials;
            else
                materials = renderer.materials;
#endif
            var mesh = filter.sharedMesh;

            // cannot shatterCount a mesh that doesn't exist
            if (!mesh)
            {
                Debug.LogWarning("EzySlice::Slice -> Provided GameObject must have a Mesh that is not NULL.");

                return null;
            }

            var subMeshCount = mesh.subMeshCount;

            // to make things straightforward, exit without slicing if the materials and mesh
            // array don't match. This shouldn't happen anyway
            if (materials.Length != subMeshCount)
            {
                Debug.LogWarning("EzySlice::Slice -> Provided Material array must match the length of sub meshes.");

                return null;
            }

            // we need to find the index of the material for the cross section.
            // default to the end of the array
            var crossIndex = materials.Length;

            // for cases where the sliced material is null, we will append the cross section to the end
            // of the sub mesh array, this is because the application may want to set/change the material
            // after slicing has occured, so we don't assume anything
            if (crossMaterial)
            {
                for (var i = 0; i < crossIndex; i++)
                {
                    if (materials[i] == crossMaterial)
                    {
                        crossIndex = i;
                        break;
                    }
                }
            }

            return Slice(mesh, plane, crossRegion, crossIndex);
        }

        /**
         * Slice the GameObject mesh (if any) using the Plane, which will generate
         * a maximum of 2 other Meshes.
         * This function will recalculate new UV coordinates to ensure textures are applied
         * properly.
         * Returns null if no intersection has been found or the GameObject does not contain
         * a valid mesh to cut.
         */
        private static SlicedHull Slice(in Mesh sharedMesh, in Plane plane, in TextureRegion region, int crossIndex)
        {
            if (!sharedMesh)
            {
                return null;
            }

            var sourceVertices = sharedMesh.vertices;
            var uv = sharedMesh.uv;
            var norm = sharedMesh.normals;
            var tan = sharedMesh.tangents;

            var subMeshCount = sharedMesh.subMeshCount;

            // each sub mesh will be sliced and placed in its own array structure
            var slices = new SlicedSubMesh[subMeshCount];
            // the cross section hull is common across all sub meshes
            var crossHull = new List<Vector3>();

            // we reuse this object for all intersection tests
            var result = new IntersectionResult();

            // see if we would like to split the mesh using uv, normals and tangents
            var genUV = sourceVertices.Length == uv.Length;
            var genNorm = sourceVertices.Length == norm.Length;
            var genTan = sourceVertices.Length == tan.Length;

            // iterate over all the sub meshes individually. vertices and indices
            // are all shared within the sub mesh
            var mesh = new SlicedSubMesh();
            for (var subMesh = 0; subMesh < subMeshCount; subMesh++)
            {
                var indices = sharedMesh.GetTriangles(subMesh);
                var indicesCount = indices.Length;

                // loop through all the mesh vertices, generating upper and lower hulls
                // and all intersection points
                for (var index = 0; index < indicesCount; index += 3)
                {
                    var i0 = indices[index];
                    var i1 = indices[index + 1];
                    var i2 = indices[index + 2];

                    var newTri = new Triangle(sourceVertices[i0], sourceVertices[i1], sourceVertices[i2]);

                    // generate UV if available
                    if (genUV)
                    {
                        newTri.SetUV(uv[i0], uv[i1], uv[i2]);
                    }

                    // generate normals if available
                    if (genNorm)
                    {
                        newTri.SetNormal(norm[i0], norm[i1], norm[i2]);
                    }

                    // generate tangents if available
                    if (genTan)
                    {
                        newTri.SetTangent(tan[i0], tan[i1], tan[i2]);
                    }

                    // shatterCount this particular triangle with the provided plane
                    if (newTri.Split(plane, result))
                    {
                        int upperHullCount = result.UpperHullCount;
                        int lowerHullCount = result.LowerHullCount;
                        int interHullCount = result.IntersectionPointCount;

                        for (int i = 0; i < upperHullCount; i++)
                        {
                            mesh.upperHull.Add(result.UpperHull[i]);
                        }

                        for (int i = 0; i < lowerHullCount; i++)
                        {
                            mesh.lowerHull.Add(result.LowerHull[i]);
                        }

                        for (int i = 0; i < interHullCount; i++)
                        {
                            crossHull.Add(result.IntersectionPoints[i]);
                        }
                    }
                    else
                    {
                        var sa = plane.SideOf(sourceVertices[i0]);
                        var sb = plane.SideOf(sourceVertices[i1]);
                        var sc = plane.SideOf(sourceVertices[i2]);

                        var side = PlaneEx.SideOfPlane.On;
                        if (sa != PlaneEx.SideOfPlane.On)
                        {
                            side = sa;
                        }

                        if (sb != PlaneEx.SideOfPlane.On)
                        {
                            Debug.Assert(side == PlaneEx.SideOfPlane.On || side == sb);
                            side = sb;
                        }

                        if (sc != PlaneEx.SideOfPlane.On)
                        {
                            Debug.Assert(side == PlaneEx.SideOfPlane.On || side == sc);
                            side = sc;
                        }

                        if (side == PlaneEx.SideOfPlane.Up || side == PlaneEx.SideOfPlane.On)
                        {
                            mesh.upperHull.Add(newTri);
                        }
                        else
                        {
                            mesh.lowerHull.Add(newTri);
                        }
                    }
                }

                // register into the index
                slices[subMesh] = mesh;
            }

            // check if slicing actually occured
            foreach (var slice in slices)
            {
                // check if at least one of the sub meshes was sliced. If so, stop checking
                // because we need to go through the generation step
                if (slice != null && slice.IsValid)
                {
                    return CreateFrom(slices, CreateFrom(crossHull, plane.normal, region), crossIndex);
                }
            }

            // no slicing occured, just return null to signify
            return null;
        }

        /**
         * Generates a single SlicedHull from a set of cut sub meshes 
         */
        private static SlicedHull CreateFrom(in SlicedSubMesh[] meshes, in List<Triangle> crossRegion, int crossSectionIndex)
        {
            var subMeshCount = meshes.Length;

            var upperHullCount = 0;
            var lowerHullCount = 0;

            // get the total amount of upper, lower and intersection counts
            for (var subMesh = subMeshCount - 1; subMesh >= 0; subMesh--)
            {
                upperHullCount += meshes[subMesh].upperHull.Count;
                lowerHullCount += meshes[subMesh].lowerHull.Count;
            }

            Mesh upperHull = CreateUpperHull(meshes, upperHullCount, crossRegion, crossSectionIndex, out var upperHullVertices);
            Mesh lowerHull = CreateLowerHull(meshes, lowerHullCount, crossRegion, crossSectionIndex, out var lowerHullVertices);

            return new SlicedHull(upperHull, lowerHull, upperHullVertices, lowerHullVertices);
        }

        private static Mesh CreateUpperHull(in SlicedSubMesh[] mesh, int total, in List<Triangle> crossSection, int crossSectionIndex, out Vector3[] hullVertices)
        {
            return CreateHull(mesh, total, crossSection, crossSectionIndex, true, out hullVertices);
        }

        private static Mesh CreateLowerHull(in SlicedSubMesh[] mesh, int total, in List<Triangle> crossSection, int crossSectionIndex, out Vector3[] hullVertices)
        {
            return CreateHull(mesh, total, crossSection, crossSectionIndex, false, out hullVertices);
        }

        /**
         * Generate a single Mesh HULL of either the UPPER or LOWER hulls. 
         */
        private static Mesh CreateHull(in SlicedSubMesh[] meshes, int total, in List<Triangle> crossSection, int crossIndex, bool isUpper, out Vector3[] hullVertices)
        {
            if (total <= 0)
            {
                hullVertices = null;
                return null;
            }

            int subMeshCount = meshes.Length;
            int crossCount = crossSection?.Count ?? 0;

            Mesh newMesh = new Mesh();

            var arrayLen = (total + crossCount) * 3;

            var hasUV = meshes[0].HasUV;
            var hasNormal = meshes[0].HasNormal;
            var hasTangent = meshes[0].HasTangent;

            // vertices and uv's are common for all sub meshes
            hullVertices = new Vector3[arrayLen];
            var newUvs = hasUV ? new Vector2[arrayLen] : null;
            var newNormals = hasNormal ? new Vector3[arrayLen] : null;
            var newTangents = hasTangent ? new Vector4[arrayLen] : null;

            // each index refers to our sub mesh triangles
            var triangles = new List<int[]>(subMeshCount);

            var vIndex = 0;

            // first we generate all our vertices, uv's and triangles
            for (var subMesh = 0; subMesh < subMeshCount; subMesh++)
            {
                // pick the hull we will be playing around with
                var hull = isUpper ? meshes[subMesh].upperHull : meshes[subMesh].lowerHull;
                var hullCount = hull.Count;

                var indices = new int[hullCount * 3];

                // fill our mesh arrays
                for (int i = 0, triIndex = 0; i < hullCount; i++, triIndex += 3)
                {
                    var newTri = hull[i];

                    var i0 = vIndex + 0;
                    var i1 = vIndex + 1;
                    var i2 = vIndex + 2;

                    // add the vertices
                    hullVertices[i0] = newTri.PositionA;
                    hullVertices[i1] = newTri.PositionB;
                    hullVertices[i2] = newTri.PositionC;

                    // add the UV coordinates if any
                    if (hasUV)
                    {
                        newUvs[i0] = newTri.UvA;
                        newUvs[i1] = newTri.UvB;
                        newUvs[i2] = newTri.UvC;
                    }

                    // add the Normals if any
                    if (hasNormal)
                    {
                        newNormals[i0] = newTri.normalA;
                        newNormals[i1] = newTri.normalB;
                        newNormals[i2] = newTri.normalC;
                    }

                    // add the Tangents if any
                    if (hasTangent)
                    {
                        newTangents[i0] = newTri.tangentA;
                        newTangents[i1] = newTri.tangentB;
                        newTangents[i2] = newTri.tangentC;
                    }

                    // triangles are returned in clockwise order from the intersection, no need to sort these
                    indices[triIndex] = i0;
                    indices[triIndex + 1] = i1;
                    indices[triIndex + 2] = i2;

                    vIndex += 3;
                }

                // add triangles to the index for later generation
                triangles.Add(indices);
            }

            // generate the cross section required for this particular hull
            if (crossSection != null && crossCount > 0)
            {
                var crossIndices = new int[crossCount * 3];

                for (int i = 0, triIndex = 0; i < crossCount; i++, triIndex += 3)
                {
                    Triangle newTri = crossSection[i];

                    var i0 = vIndex + 0;
                    var i1 = vIndex + 1;
                    var i2 = vIndex + 2;

                    // add the vertices
                    hullVertices[i0] = newTri.PositionA;
                    hullVertices[i1] = newTri.PositionB;
                    hullVertices[i2] = newTri.PositionC;

                    // add the UV coordinates if any
                    if (hasUV)
                    {
                        newUvs[i0] = newTri.UvA;
                        newUvs[i1] = newTri.UvB;
                        newUvs[i2] = newTri.UvC;
                    }

                    // add the Normals if any
                    if (hasNormal)
                    {
                        // invert the normals depending on upper or lower hull
                        if (isUpper)
                        {
                            newNormals[i0] = -newTri.normalA;
                            newNormals[i1] = -newTri.normalB;
                            newNormals[i2] = -newTri.normalC;
                        }
                        else
                        {
                            newNormals[i0] = newTri.normalA;
                            newNormals[i1] = newTri.normalB;
                            newNormals[i2] = newTri.normalC;
                        }
                    }

                    // add the Tangents if any
                    if (hasTangent)
                    {
                        newTangents[i0] = newTri.tangentA;
                        newTangents[i1] = newTri.tangentB;
                        newTangents[i2] = newTri.tangentC;
                    }

                    // add triangles in clockwise for upper
                    // and reversed for lower hulls, to ensure the mesh
                    // is facing the right direction
                    if (isUpper)
                    {
                        crossIndices[triIndex] = i0;
                        crossIndices[triIndex + 1] = i1;
                        crossIndices[triIndex + 2] = i2;
                    }
                    else
                    {
                        crossIndices[triIndex] = i0;
                        crossIndices[triIndex + 1] = i2;
                        crossIndices[triIndex + 2] = i1;
                    }

                    vIndex += 3;
                }

                // add triangles to the index for later generation
                if (triangles.Count <= crossIndex)
                {
                    triangles.Add(crossIndices);
                }
                else
                {
                    // otherwise, we need to merge the triangles for the provided subsection
                    var prevTriangles = triangles[crossIndex];
                    var merged = new int[prevTriangles.Length + crossIndices.Length];

                    System.Array.Copy(prevTriangles, merged, prevTriangles.Length);
                    System.Array.Copy(crossIndices, 0, merged, prevTriangles.Length, crossIndices.Length);

                    // replace the previous array with the new merged array
                    triangles[crossIndex] = merged;
                }
            }

            int totalTriangles = triangles.Count;

            newMesh.subMeshCount = totalTriangles;
            // fill the mesh structure
            newMesh.vertices = hullVertices;

            if (hasUV)
            {
                newMesh.uv = newUvs;
            }

            if (hasNormal)
            {
                newMesh.normals = newNormals;
            }

            if (hasTangent)
            {
                newMesh.tangents = newTangents;
            }

            // add the sub meshes
            for (var i = 0; i < totalTriangles; i++)
            {
                newMesh.SetTriangles(triangles[i], i, false); // Both meshes share vertices so the bounds will remain the same
            }

            return newMesh;
        }

        /**
         * Generate Two Meshes (an upper and lower) cross section from a set of intersection
         * points and a plane Normal. Intersection Points do not have to be in order.
         */
        private static List<Triangle> CreateFrom(in List<Vector3> intPoints, in Vector3 planeNormal, in TextureRegion region)
        {
            if (Triangulator.MonotoneChain(intPoints, planeNormal, out var tris, region))
            {
                return tris;
            }

            return null;
        }
    }
}