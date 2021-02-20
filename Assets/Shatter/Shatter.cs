using System.Collections.Generic;
using EzySlice;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Examples.Scripts
{
    /**
 * Represents a really badly written shatter script! use for reference purposes only.
 */
    public class Shatter : MonoBehaviour
    {
        public GameObject objectToShatter;
        public Material crossSectionMaterial;
        public int shatterCount;

#if DEBUG
        public bool enableTestPlane;
        public GameObject testPlane;
#endif

        public List<GameObject> Shards { get; private set; }

        private void Awake()
        {
            gameObject.SetActive(true);
            // Shards = new List<GameObject>();
        }

        private BoundingSphere debugSphere;

        private SlicedHull ShatterObject(GameObject obj)
        {
            var textureRegion = new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f);
            SlicedHull slicedHull;

            if (enableTestPlane && testPlane)
            {
                var plane = new Plane(testPlane.transform.up, testPlane.transform.position);
                
                slicedHull = obj.SliceInstantiate(
                    plane,
                    textureRegion,
                    crossSectionMaterial);
            }
            else
            {
                var col = obj.GetComponent<Collider>();
                Debug.Assert(col);
                var objBounds = col.bounds;
                const float oneOnSqrt2 = 0.7f; // just less than 1/sqrt(2) - should produce a cut through most meshes with a tight bounds
                var plane = GetRandomPlane(objBounds.center, objBounds.extents * oneOnSqrt2);
                slicedHull = obj.SliceInstantiate(
                    plane,
                    textureRegion,
                    crossSectionMaterial);

#if DEBUG
                debugSphere = new BoundingSphere(objBounds.center, objBounds.extents.magnitude /* * oneOnSqrt2*/);
                DebugExtension.DebugWireSphere(debugSphere.position, Color.white, debugSphere.radius, 2);
#endif
            }

            if (slicedHull is null)
            {
                Debug.Break();
            }
            return slicedHull;
        }

        private static Plane GetRandomPlane(in Vector3 positionOffset, in Vector3 scale)
        {
            var randomPosition = Random.insideUnitSphere;

            randomPosition += positionOffset;

            var randomDirection = Random.insideUnitSphere.normalized;

            return new Plane(randomDirection, randomPosition);
        }

        public void Explode()
        {
            // var q = 0;
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            if (testPlane)
            {
                testPlane.SetActive(enableTestPlane);
            }
        }
#endif

        public void Gravity()
        {
            if (Shards.Count > 0)
            {
                var g = !Shards[0].GetComponent<Rigidbody>().useGravity;
                foreach (var s in Shards)
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
            GameObject objectToSplit;

            if (Shards is null || !Application.isPlaying)
            {
                // First shatter
                Shards = new List<GameObject>();
                objectToSplit = objectToShatter;
                print($"RandomShatter {objectToSplit.name}");
            }
            else
            {
                // Otherwise, shatter the previous shards, randomly picked
                do
                {
                    objectToSplit = Shards[Random.Range(0, Shards.Count - 1)];
                    print($"RandomShatter from {objectToSplit.name}");
                    if (!objectToSplit.activeSelf)
                    {
                        Debug.LogWarning("Tried to select an inactive shatter object", objectToSplit);
                    }
                } while (!objectToSplit || !objectToSplit.activeSelf);
            }

            var slicedHull = ShatterObject(objectToSplit);

            if (slicedHull == null)
                return;

            Debug.Assert(slicedHull.HullMesh(0) && slicedHull.HullMesh(1), "There should be an upper and lower hull");

            // add rigidbodies and colliders
            var rbSource = objectToSplit.GetComponentInChildren<Rigidbody>();
            for (var i = 0; i < 2; ++i)
            {
                var shattered = slicedHull.HullObject(i);
                shattered.AddComponent<MeshCollider>().convex = true;
                if (rbSource)
                {
                    var rb = shattered.AddComponent<Rigidbody>();
                    rb.velocity = rbSource.velocity;
                    rb.angularVelocity = rbSource.angularVelocity;
                    rb.useGravity = rbSource.useGravity;
                    rb.isKinematic = rbSource.isKinematic;
                    rb.drag = rbSource.drag;
                    rb.angularDrag = rbSource.angularDrag;
                    rb.mass = rbSource.mass * slicedHull.HullVolume(i) / slicedHull.SourceVolume;
                }

                Shards.Add(shattered);
            }

            if (Application.isPlaying)
                Destroy(objectToSplit);
            else
                DestroyImmediate(objectToSplit);

            Shards.Remove(objectToSplit);
        }
    }
}