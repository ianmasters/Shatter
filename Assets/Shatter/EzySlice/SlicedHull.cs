using System.Security.Authentication.ExtendedProtection.Configuration;
using UnityEngine;
using Extensions;

// ReSharper disable once CheckNamespace
namespace EzySlice
{
    /**
     * The final generated data structure from a shatterCount operation. This provides easy access
     * to utility functions and the final Mesh data for each section of the HULL.
     */
    public sealed class SlicedHull
    {
        private readonly GameObject[] hull = new GameObject[2]; // upper : lower
        private readonly Mesh[] hullMesh = new Mesh[2];
        private readonly float[] hullVolume = new float[2];

        private static int _upperHullId;
        private static int _lowerHullId;

        public static void ResetDebug()
        {
            _upperHullId = _lowerHullId = 0;
        }

        public float SourceVolume => hullVolume[0] + hullVolume[1];

        public GameObject HullObject(int i)
        {
            Debug.Assert(i >= 0 && i < 2);
            return hull[i];
        }

        public Mesh HullMesh(int i)
        {
            return hullMesh[i];
        }

        public float HullVolume(int i)
        {
            return i == 0 ? hullVolume[0] : hullVolume[1];
        }

        public SlicedHull(Mesh upperHullMesh, Mesh lowerHullMesh, in Vector3[] upperHullVertices, in Vector3[] lowerHullVertices)
        {
            Debug.Assert(upperHullMesh || lowerHullMesh, "There should be at least one hull mesh to create a SlicedHull");
            hullMesh[0] = upperHullMesh;
            hullMesh[1] = lowerHullMesh;
            hullVolume[0] = upperHullMesh.CalculateVolume(upperHullVertices);
            hullVolume[1] = lowerHullMesh.CalculateVolume(lowerHullVertices);
        }

        private void CreateHull(int hullIndex, GameObject original, Material crossSectionMat)
        {
            GameObject newObject = hull[hullIndex];
            if (newObject)
            {
                Mesh newMesh = hullMesh[hullIndex];

                newObject.transform.localPosition = original.transform.localPosition;
                newObject.transform.localRotation = original.transform.localRotation;
                newObject.transform.localScale = original.transform.localScale;

                var shared = original.GetComponent<MeshRenderer>().sharedMaterials;
                var mesh = original.GetComponent<MeshFilter>().sharedMesh;
                
                var newRenderer = newObject.GetComponent<MeshRenderer>();

                // nothing changed in the hierarchy, the cross section must have been batched
                // with the sub meshes, return as is, no need for any changes
                if (mesh.subMeshCount == newMesh.subMeshCount)
                {
                    // the the material information
                    newRenderer.sharedMaterials = shared;
                }
                else
                {
                    // otherwise the cross section was added to the back of the sub mesh array because
                    // it uses a different material. We need to take this into account
                    var newShared = new Material[shared.Length + 1];

                    // copy our material arrays across using native copy (should be faster than loop)
                    System.Array.Copy(shared, newShared, shared.Length);
                    newShared[shared.Length] = crossSectionMat;
                    newRenderer.sharedMaterials = newShared;
                }
            }
        }

        public GameObject CreateUpperHull(GameObject original, Material crossSectionMat)
        {
            hull[0] = CreateUpperHull();
            CreateHull(0, original, crossSectionMat);
            return hull[0];
        }

        public GameObject CreateLowerHull(GameObject original, Material crossSectionMat)
        {
            hull[1] = CreateLowerHull();
            CreateHull(1, original, crossSectionMat);
            return hull[1];
        }

        /**
         * Generate a new GameObject from the upper hull of the mesh
         * This function will return null if upper hull does not exist
         */
        private GameObject CreateUpperHull()
        {
            return CreateEmptyObject($"Upper_Hull {_upperHullId++}", hullMesh[0]);
        }

        /**
         * Generate a new GameObject from the Lower hull of the mesh
         * This function will return null if lower hull does not exist
         */

        private GameObject CreateLowerHull()
        {
            return CreateEmptyObject($"Lower_Hull {_lowerHullId++}", hullMesh[1]);
        }

        /**
         * Helper function which will create a new GameObject to be able to add
         * a new mesh for rendering and return.
         */
        private static GameObject CreateEmptyObject(string name, Mesh hull)
        {
            if (!hull)
                return null;

            hull.name = name;
            
            var newObject = new GameObject(name);

            newObject.AddComponent<MeshRenderer>();
            newObject.AddComponent<MeshFilter>().mesh = hull;

            return newObject;
        }
    }
}