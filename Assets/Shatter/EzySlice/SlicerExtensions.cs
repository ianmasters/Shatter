using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace EzySlice
{
    /**
     * Define Extension methods for easy access to slicer functionality
     */
    public static class SlicerExtensions
    {
        /**
         * SlicedHull Return functions and appropriate overrides!
         */
        public static SlicedHull Slice(this GameObject obj, in Plane pl, Material crossSectionMaterial = null)
        {
            return Slice(obj, pl, new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f), crossSectionMaterial);
        }

        public static SlicedHull Slice(this GameObject obj, in Vector3 position, in Vector3 direction, Material crossSectionMaterial = null)
        {
            return Slice(obj, position, direction, new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f), crossSectionMaterial);
        }

        public static SlicedHull Slice(this GameObject obj, in Vector3 position, in Vector3 direction, TextureRegion textureRegion, Material crossSectionMaterial = null)
        {
            var cuttingPlane = new Plane();

            var refUp = obj.transform.InverseTransformDirection(direction);
            var refPt = obj.transform.InverseTransformPoint(position);

            cuttingPlane.SetNormalAndPosition(refUp, refPt);

            return Slice(obj, cuttingPlane, textureRegion, crossSectionMaterial);
        }

        public static SlicedHull Slice(this GameObject obj, in Plane pl, in TextureRegion textureRegion, Material crossSectionMaterial = null)
        {
            return Slicer.Slice(obj, pl, textureRegion, crossSectionMaterial);
        }

        /**
         * These functions (and overrides) will return the final instantiated GameObjects types
         */
        // public static GameObject[] SliceInstantiate(this GameObject obj, Plane plane)
        // {
        //     return SliceInstantiate(obj, plane, new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f));
        // }

        // public static GameObject[] SliceInstantiate(this GameObject obj, Vector3 position, Vector3 direction)
        // {
        //     return SliceInstantiate(obj, position, direction, null);
        // }

        public static SlicedHull SliceInstantiate(this GameObject obj, in Vector3 position, in Vector3 direction, Material crossSectionMat)
        {
            return SliceInstantiate(obj, position, direction, new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f), crossSectionMat);
        }

        public static SlicedHull SliceInstantiate(this GameObject obj, in Vector3 positionInWorldSpace, in Vector3 directionInWorldSpace, in TextureRegion cuttingRegion, Material crossSectionMaterial = null)
        {
            var cuttingPlane = new Plane(directionInWorldSpace, positionInWorldSpace);

            return SliceInstantiate(obj, cuttingPlane, cuttingRegion, crossSectionMaterial);
        }

        public static SlicedHull SliceInstantiate(this GameObject obj, in Plane planeInWorldSpace, in TextureRegion cuttingRegion, Material crossSectionMaterial = null)
        {
            // Transform the plane into object space for cutting
            var plane = obj.transform.InverseTransformPlane(planeInWorldSpace);
            
            var slicedHull = Slicer.Slice(obj, plane, cuttingRegion, crossSectionMaterial);

            if (slicedHull == null)
            {
                return null;
            }

            var upperHull = slicedHull.CreateUpperHull(obj, crossSectionMaterial);
            var lowerHull = slicedHull.CreateLowerHull(obj, crossSectionMaterial);

            if (upperHull is null && lowerHull is null)
            {
                // nothing to return, so return nothing!
                return null;
            }

            return slicedHull;
        }
    }
}