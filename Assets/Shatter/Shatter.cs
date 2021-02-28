// #define SHOW_DEBUG_SPHERE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using EzySlice;
using UnityEditor;
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

        public List<Shard> shards;

#if DEBUG
        public bool enableTestPlane;
        public GameObject testPlane;
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
            shards = new List<Shard>();

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

        private SlicedHull RandomSliceObject(GameObject obj)
        {
            var textureRegion = new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f);

            var r = obj.GetComponent<Renderer>();

            // if (newMaterials is null) newMaterials = r.materials;

            var objBounds = r.bounds;
            const float oneOnSqrt2 = 0.7f; // just less than 1/sqrt(2) - should produce a cut through most meshes with a tight bounds
            var plane = GetRandomPlane(objBounds.center, objBounds.extents * oneOnSqrt2);
            var slicedHull = obj.SliceInstantiate(
                plane,
                textureRegion,
                crossSectionMaterial);

            // PostShatter(objectToShatter, slicedHull);

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
            if (shards.Count > 0)
            {
                var g = !shards[0].GetComponent<Rigidbody>().useGravity;
                foreach (var s in shards)
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

            shards = new List<Shard>();

            var allSlicedHulls = new List<SlicedHull>();

            allSlicedHulls.Add(RandomSliceObject(objectToShatter));
            // var m = allSlicedHulls[0].HullObject(0).GetComponent<MeshRenderer>().materials;

            for (var i = 1; i < shatterCount; ++i)
            {
                var count = allSlicedHulls.Count;
                for (var j = 0; j < count; ++j)
                {
                    var hull = allSlicedHulls[j];
                    for (var k = 0; k < 2; ++k)
                    {
                        var obj = hull.HullObject(k);
                        var slicedHull = RandomSliceObject(obj);

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
                            shards.Add(obj.AddComponent<Shard>());
                        }
                    }
                }
                allSlicedHulls.RemoveRange(0, count);
            }

            foreach (var slicedHull in allSlicedHulls)
            {
                PostShatter(objectToShatter, slicedHull);
            }

            if (Application.isPlaying) Destroy(objectToShatter);
            else DestroyImmediate(objectToShatter);
        }

        private void PostShatter(GameObject initialObject, SlicedHull slicedHull)
        {
            if (slicedHull == null)
                return;

            Debug.Assert(slicedHull.HullMesh(0) && slicedHull.HullMesh(1), "There should be an upper and lower hull");

            // add rigidbodies and colliders
            var rbSource = initialObject.GetComponentInChildren<Rigidbody>();
            for (var i = 0; i < 2; ++i)
            {
                var shattered = slicedHull.HullObject(i);
                Debug.Assert(!shattered.GetComponent<MeshCollider>());
                shattered.AddComponent<MeshCollider>().convex = true;
                if (rbSource)
                {
                    var rb = shattered.AddComponent<Rigidbody>();
                    rb.detectCollisions = false;
                    rb.velocity = rbSource.velocity;
                    rb.angularVelocity = rbSource.angularVelocity;
                    rb.useGravity = rbSource.useGravity; // TODO: use gravity?
                    rb.isKinematic = rbSource.isKinematic;
                    rb.drag = rbSource.drag;
                    rb.angularDrag = rbSource.angularDrag;
                    rb.mass = rbSource.mass * slicedHull.HullVolume(i) / slicedHull.SourceVolume;

                    if (Application.isPlaying)
                    {
                        var enableCollisions = shattered.AddComponent<EnableCollisions>();
                        enableCollisions.enableGravity = true;
                        // enableCollisions.enableAfterFrames = 60;
                    }
                }

                shards.Add(shattered.AddComponent<Shard>());
            }

            // Shards.Remove(initialObject);

            // foreach (var shard in shards)
            // {
            //     var rb = shard.GetComponent<Rigidbody>();
            // }

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
                foreach (var shard in shards)
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

#if UNITY_EDITOR
        public void OnValidate()
        {
            if (testPlane)
            {
                var r = testPlane.GetComponent<Renderer>();
                if (r)
                    r.enabled = enableTestPlane;
            }
        }
#endif
    }
}