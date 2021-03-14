// #define SHOW_DEBUG_SPHERE

using System;
using System.Collections;
using System.Collections.Generic;
using EzySlice;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Shatter
{
    public class Shatter : MonoBehaviour
    {
        public enum DestroyOnCompleteType
        {
            Disabled,
            Script,
            GameObject
        }

        [Tooltip("After the shatter has completed, optionally destroy this script or it's GameObject")]
        public DestroyOnCompleteType destroyOnComplete;

        [Tooltip("GameObject to shatter")] public GameObject objectToShatter;

        [Tooltip("Cross section material to use")]
        public Material crossSectionMaterial;

        [Tooltip("How many iterations to shatter")]
        public int shatterCount;

        public List<Shrapnel> shrapnels;

        [Range(0f, 5f), Tooltip(">0 will fade shrapnel out")]
        public float fadeOutTime;

        [Tooltip("Enable gravity for shrapnel when shattered")]
        public bool enableGravity;

#if DEBUG
        [Tooltip("Enable test plane for a single slice")]
        public bool enableTestPlane;

        [Tooltip("Test plane object to use")] public GameObject testPlane;
#endif

        private void Awake()
        {
            gameObject.SetActive(true);
        }

#if SHOW_DEBUG_SPHERE
        private BoundingSphere debugSphere;
#endif
        public void SlicePlane(GameObject planeObject)
        {
            shrapnels = new List<Shrapnel>();

            var plane = new Plane(planeObject.transform.up, planeObject.transform.position);
            var textureRegion = new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f);

            var slicedHull = objectToShatter.SliceInstantiate(
                plane,
                textureRegion,
                crossSectionMaterial);

            PostShatter(objectToShatter, slicedHull);

            if (Application.isPlaying) Destroy(objectToShatter);
            else DestroyImmediate(objectToShatter);
        }

        private SlicedHull RandomSliceObject(GameObject obj, TextureRegion textureRegion)
        {
            var r = obj.GetComponent<Renderer>();

            // if (newMaterials is null) newMaterials = r.materials;

            var objBounds = r.bounds;
            const float oneOnSqrt2 = 0.7f; // just less than 1/sqrt(2) - should produce a cut through most meshes with a tight bounds
            var plane = GetRandomPlane(objBounds.center, objBounds.extents * oneOnSqrt2);
            var slicedHull = obj.SliceInstantiate(
                plane,
                textureRegion,
                crossSectionMaterial);

            PostShatter(objectToShatter, slicedHull);

#if SHOW_DEBUG_SPHERE
            debugSphere = new BoundingSphere(objBounds.center, objBounds.extents.magnitude /* * oneOnSqrt2*/);
            DebugExtension.DebugWireSphere(debugSphere.position, Color.white, debugSphere.radius, 2);
#endif
            return slicedHull;
        }

        private static Plane GetRandomPlane(in Vector3 positionOffset, in Vector3 scale)
        {
            var randomPosition = Random.insideUnitSphere;
            randomPosition.Scale(scale);

            randomPosition += positionOffset;

            var randomDirection = Random.insideUnitSphere.normalized;

            return new Plane(randomDirection, randomPosition);
        }

        public void Explode()
        {
            // var q = 0;
        }

        public void Gravity()
        {
            if (shrapnels.Count > 0)
            {
                var g = !shrapnels[0].GetComponent<Rigidbody>().useGravity;
                foreach (var s in shrapnels)
                {
                    s.GetComponent<Rigidbody>().useGravity = g;
                }
            }
            else
            {
                var rb = objectToShatter.GetComponent<Rigidbody>();
                rb.useGravity = !rb.useGravity;
            }
        }

        // This method can be compounded to iteratively shatter previous shatters
        public void RandomShatter()
        {
            print($"RandomShatter {objectToShatter.name}");

            shrapnels = new List<Shrapnel>();

            var textureRegion = new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f);

            var allSlicedHulls = new List<SlicedHull>();
            allSlicedHulls.Add(RandomSliceObject(objectToShatter, textureRegion));
#if DEBUG
            var materialsTest = allSlicedHulls[0].HullObject(0).GetComponent<MeshRenderer>().sharedMaterials;
#endif
            for (var i = 1; i < shatterCount; ++i)
            {
                var count = allSlicedHulls.Count;
                for (var j = 0; j < count; ++j)
                {
                    var hull = allSlicedHulls[j];
                    for (var k = 0; k < 2; ++k)
                    {
                        var obj = hull.HullObject(k);
                        var slicedHull = RandomSliceObject(obj, textureRegion);

                        if (slicedHull != null)
                        {
                            allSlicedHulls.Add(slicedHull);
                            if (Application.isPlaying) Destroy(obj);
                            else DestroyImmediate(obj);
                        }
                        else
                        {
                            // Debug.Break();
                            // add the hull back in and try again
                            // allSlicedHulls.Add(slicedHull);
                            shrapnels.Add(obj.AddComponent<Shrapnel>());
                        }
                    }
                }
                allSlicedHulls.RemoveRange(0, count);
            }

            // PostShatter(objectToShatter);

            if (Application.isPlaying) Destroy(objectToShatter);
            else DestroyImmediate(objectToShatter);
        }

        // Add colliders, rigidbodies etc. for the final objects ready to go into the world.
        private void PostShatter(GameObject initialObject, SlicedHull slicedHull)
        {
            // foreach (var slicedHull in allSlicedHulls)
            {
                Debug.Assert(slicedHull != null);

                var rbSource = initialObject.GetComponentInChildren<Rigidbody>();
                for (var i = 0; i < 2; ++i)
                {
                    var hullShrapnel = slicedHull.HullObject(i);
                    if (hullShrapnel)
                    {
                        Debug.Assert(!hullShrapnel.GetComponent<MeshCollider>());
                        hullShrapnel.AddComponent<MeshCollider>().convex = true;
                        if (rbSource)
                        {
                            var rb = hullShrapnel.AddComponent<Rigidbody>();
                            rb.detectCollisions = false;
                            rb.velocity = rbSource.velocity;
                            rb.angularVelocity = rbSource.angularVelocity;
                            rb.useGravity = enableGravity;
                            rb.isKinematic = rbSource.isKinematic;
                            rb.drag = rbSource.drag;
                            rb.angularDrag = rbSource.angularDrag;
                            rb.mass = rbSource.mass * slicedHull.HullVolume(i) / slicedHull.SourceVolume;

                            if (Application.isPlaying)
                            {
                                hullShrapnel.AddComponent<EnableCollisions>();
                            }
                        }
                        shrapnels.Add(hullShrapnel.AddComponent<Shrapnel>());
                    }
                }

                // Shards.Remove(initialObject);

                // foreach (var shrapnel in shrapnels)
                // {
                //     var rb = shrapnel.GetComponent<Rigidbody>();
                // }
            }

            if (destroyOnComplete != DestroyOnCompleteType.Disabled)
            {
                StartCoroutine(DestroyOnComplete());
            }
        }

        private IEnumerator DestroyOnComplete()
        {
            var n = 0;
            for (;;)
            {
                foreach (var shard in shrapnels)
                {
                    if (shard)
                        ++n;
                }
                if (n == 0) break;
                yield return null;
            }

            switch (destroyOnComplete)
            {
                case DestroyOnCompleteType.Disabled:
                    break;

                case DestroyOnCompleteType.Script:
                    if (Application.isPlaying) Destroy(this);
                    else DestroyImmediate(this);
                    break;

                case DestroyOnCompleteType.GameObject:
                    if (Application.isPlaying) Destroy(gameObject);
                    else DestroyImmediate(gameObject);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}