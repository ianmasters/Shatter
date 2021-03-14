using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A type-safe, generic object pool. This object pool requires you to derive a class from it,
/// and specify the type of object to pool.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObjectPool<T> : MonoBehaviour where T : PooledObject<T>
{
    [Tooltip("Prefab for this object pool")]
    public T prefab;

    [Tooltip("Size of this object pool")] public int size;

    // The list of free and used objects for tracking.
    private List<T> freeList;
    private List<T> usedList;

    private void Awake()
    {
        freeList = new List<T>(size);
        usedList = new List<T>(size);

        prefab.gameObject.SetActive(false); // disable the prefab so SetActive will call OnEnable so we can perform CoRoutines etc.

        // Instantiate the pooled objects and disable them.
        for (var i = 0; i < size; i++)
        {
            var pooledObject = Instantiate(prefab, transform);
#if DEBUG
            pooledObject.name = $"{prefab.name}{i}";
#endif
            pooledObject.pool = this;
            pooledObject.gameObject.SetActive(false);
            freeList.Add(pooledObject);
        }

        // if(!prefabActive) prefab.gameObject.SetActive(false);
    }

    /// <summary>
    /// Returns an object from the pool. Returns null if there are no more objects free in the pool.
    /// </summary>
    /// <returns>Object of type T from the pool.</returns>
    public T Instantiate(Transform parent = null)
    {
        var numFree = freeList.Count;
        if (numFree == 0)
            return null;

        // Pull an object from the end of the free list.
        var pooledObject = freeList[numFree - 1];
        freeList.RemoveAt(numFree - 1);
        usedList.Add(pooledObject);
        if (parent) pooledObject.transform.SetParent(parent, false);
        pooledObject.gameObject.SetActive(true);
        return pooledObject;
    }

    /// <summary>
    /// Returns an object to the pool. The object must have been created by this ObjectPool.
    /// </summary>
    /// <param name="pooledObject">Object previously obtained from this ObjectPool</param>
    public void ReturnObject(T pooledObject)
    {
        // It might already be returned to the free pool. Just ignore it.
        if (usedList.Contains(pooledObject))
        {
            // Disable the object
            pooledObject.gameObject.SetActive(false);

            // Put the pooled object back in the free list.
            usedList.Remove(pooledObject);
            freeList.Add(pooledObject);

            // Re-parent the pooled object to us.
            if (!ReferenceEquals(pooledObject.transform.parent, transform))
            {
                pooledObject.transform.parent = transform;
            }
            pooledObject.transform.localPosition = Vector3.zero;

            // Debug.Log($"{pooledObject.name} returned to pool");
        }
    }
}

public class PooledObject<T> : MonoBehaviour where T : PooledObject<T>
{
    internal ObjectPool<T> pool;

    public void RemoveFromScene()
    {
        if (pool)
        {
            pool.ReturnObject(this as T);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}