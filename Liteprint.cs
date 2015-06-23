// Liteprint v0.1 alpha
// Extension set for a quick & semiautomatic data pool for transform objects.

// Just put '.lp' after any transform to access his powers.

// - .lpRefill( Prepares & fill a pool for the current transform (optional).
// - .lpSpawn( Returns a clone from the pool based on the current transform (Instantiate-like).
// - .lpRecycle() Put back the current clone to his pool for reuse.
// - .lpFlush() Cleans & destroy all pool elements for the current transform.

// Created by Andrés Villalobos [andresalvivar@gmail.com] [twitter.com/matnesis]
// 14/02/2015 4:21 pm


// Copyright (c) 14/02/2015 andresalvivar@gmail.com

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.


using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Extension set for a quick & semiautomatic data pool for transform objects.
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
    /// Prepares & fill a pool for the current transform.
    /// </summary>
    public static void lpRefill(this Transform instance, int quantity)
    {
        PrepareInternalDictionaries(instance);

        Vector3 pos = instance.position;
        while (quantity-- > 0)
        {
            Transform newClone =
                MonoBehaviour.Instantiate(instance, pos + new Vector3(-9999, -9999, -9999), Quaternion.identity) as Transform;
            newClone.parent = GetParent(instance);
            readyPool[instance].Add(newClone);
        }
    }


    /// <summary>
    /// Returns a clone from the pool based on the current transform (Instantiate-like).
    /// </summary>
    public static Transform lpSpawn(this Transform instance, Vector3 position, Quaternion rotation)
    {
        PrepareInternalDictionaries(instance);

        // If not enough, create more
        if (readyPool[instance].Count < 1)
            instance.lpRefill(2);

        // First on the pool
        Transform spawn = readyPool[instance][0];

        // Skip nulls & retry
        if (spawn == null)
        {
            readyPool[instance].RemoveAt(0);
            return instance.lpSpawn(position, rotation);
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
    public static bool lpRecycle(this Transform instance)
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
    public static void lpFlush(this Transform instance)
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
