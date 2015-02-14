// Created by [Andrés Villalobos](http://twitter.com/matnesis)
// > 2015/02/14 4:21 pm


using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Extension set for a quick & semiautomatic data pool for transform objects.
/// Just put '.lit' after any transform to access his powers.
/// - *.litCreate(* Prepares a pool for the current transform (optional).
/// - *.litSpawn(* Returns a clone from the pool based on the current transform.
/// - *.litRecycle()* Put back the current clone to his pool for reuse.
/// - *.litFlush()* Cleans & destroy all pool elements for the current transform.
/// </summary>
public static class Liteprint
{
    private static Dictionary<Transform, List<Transform>> readyPool;
    private static Dictionary<Transform, Transform> outPool;


    private static void PrepareInternalDictionaries(Transform instance)
    {
        // Dictionaries
        if (readyPool == null)
            readyPool = new Dictionary<Transform, List<Transform>>();

        if (outPool == null)
            outPool = new Dictionary<Transform, Transform>();

        // Pool lists
        if (readyPool.ContainsKey(instance) == false)
            readyPool[instance] = new List<Transform>();

        if (readyPool[instance] == null)
            readyPool[instance] = new List<Transform>();
    }


    private static Transform GetParent(Transform from)
    {
        string parentName = "[Liteprint::" + from.name + "." + from.GetInstanceID() + "]";
        GameObject parent = GameObject.Find(parentName);
        if (parent == null)
            parent = new GameObject(parentName);

        return parent.transform;
    }


    /// <summary>
    /// Prepares a pool for the current transform.
    /// </summary>
    public static void litCreatePool(this Transform instance, int quantity)
    {
        PrepareInternalDictionaries(instance);

        Vector3 pos = instance.position;
        while (quantity-- > 0)
        {
            Transform newClone = MonoBehaviour.Instantiate(instance, pos + new Vector3(-9999, -9999, -9999), Quaternion.identity) as Transform;
            newClone.parent = GetParent(instance);
            readyPool[instance].Add(newClone);
        }
    }


    /// <summary>
    /// Returns a clone from the pool based on the current transform.
    /// </summary>
    public static Transform litSpawn(this Transform instance, Vector3 position, Quaternion rotation)
    {
        PrepareInternalDictionaries(instance);

        // If not enough, create more
        if (readyPool[instance].Count < 1)
            instance.litCreatePool(2);

        // First on the pool
        Transform spawn = readyPool[instance][0];

        // Skip nulls & retry
        if (spawn == null)
        {
            readyPool[instance].RemoveAt(0);
            return instance.litSpawn(position, rotation);
        }

        // Allocation
        spawn.parent = GetParent(instance);
        spawn.position = position;
        spawn.rotation = rotation;

        // Pool swap 
        readyPool[instance].RemoveAt(0);
        outPool[spawn] = instance;

        return spawn;
    }


    /// <summary>
    /// Put back the current clone to his pool for reuse.
    /// </summary>
    public static bool litRecycle(this Transform instance)
    {
        PrepareInternalDictionaries(instance);

        if (outPool.ContainsKey(instance) == true)
        {
            readyPool[outPool[instance]].Add(instance);
            return true;
        }

        return false;
    }


    /// <summary>
    /// Cleans & destroy all pool elements for the current transform.
    /// </summary>
    public static void litFlush(this Transform instance)
    {
        if (readyPool.ContainsKey(instance))
        {
            foreach (Transform t in readyPool[instance])
            {
                if (outPool.ContainsKey(t))
                    outPool.Remove(t);

                MonoBehaviour.Destroy(t.gameObject);
            }
        }
    }
}
