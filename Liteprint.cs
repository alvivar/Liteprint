// Liteprint v0.3 alpha

// Extension set for a quick semiautomatic data pool for transform objects.
// Just use '.lp' on any Transform.

// - .lpRefill() ^ Prepares and fill a pool for the current transform (optional).
// - .lpSpawn() ^ Returns a clone from the pool based on the current transform (Instantiate-like).
// - .lpRecycle() ^ Put back the current clone to his pool for reuse.
// - .lpFlush() ^ Cleans and destroy all pool elements for the current transform.

// By Andrés Villalobos ^ twitter.com/matnesis ^ andresalvivar@gmail.com
// Created 14/02/2015 4:21 pm


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
/// Extension set for a quick semiautomatic data pool for transform objects.
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
        string parentName = "[Liteprint::" + from.name + "(" + from.GetInstanceID() + ")]";
        GameObject parent = GameObject.Find(parentName);
        if (parent == null)
            parent = new GameObject(parentName);

        return parent.transform;
    }


    /// <summary>
    /// Makes sures there is a quantity of ready-to-be-used pool elements.
    /// </summary>
    public static void lpRefill(this Transform instance, int quantity)
    {
        PrepareInternalDictionaries(instance);

        // Take in consideration the current free elements
        quantity = quantity <= readyPool[instance].Count ? 0 : quantity - readyPool[instance].Count;

        while (quantity-- > 0)
        {
            Transform newClone =
                MonoBehaviour.Instantiate(instance, Vector3.zero, Quaternion.identity) as Transform;

            newClone.gameObject.SetActive(false);
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

        // First from the pool
        Transform spawn = readyPool[instance][0];

        // Skip nulls & retry
        if (spawn == null)
        {
            readyPool[instance].RemoveAt(0);
            return instance.lpSpawn(position, rotation);
        }

        // Allocation
        spawn.gameObject.SetActive(true);
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
    /// True on success, False if the Transform doesn't belong to any pool.
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
    /// Cleans and destroy all pool elements for the current transform.
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
