
// Liteprint v0.3.2 alpha

// Unity Transform Pool Extension Set. Just use '.lite' on any Transform.

// .liteRefill( ~> Makes sures there is a quantity of free pool elements ready to be used.
// .liteInstantiate( ~> Returns a clone from the pool based on the current Transform.
// .liteRecycle( ~> Use this on a clone to put him back to his pool for reuse.
// .liteFlush( ~> Cleans the pool and destroys all elements for the current transform.

// By Andrés Villalobos ~ twitter.com/matnesis ~ andresalvivar@gmail.com
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


namespace matnesis.Liteprint
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Unity Transform Pool Extension Set.
    /// </summary>
    public static class Liteprint
    {
        private static Dictionary<Transform, List<Transform>> _readyPool;
        private static Dictionary<Transform, Transform> _outPool; // <Instance used, Prefab pool>
        private static Transform _parent;


        private static void PrepareInternalData(Transform readyPoolPrefab = null)
        {
            if (_readyPool == null)
                _readyPool = new Dictionary<Transform, List<Transform>>();

            if (readyPoolPrefab && !_readyPool.ContainsKey(readyPoolPrefab))
                _readyPool[readyPoolPrefab] = new List<Transform>();


            if (_outPool == null)
                _outPool = new Dictionary<Transform, Transform>();
        }


        /// <summary>
        /// Gets a Transform that will hold the pool as children.
        /// </summary>
        private static Transform GetParent() // Transform from)
        {
            // Search
            if (_parent == null)
            {
                string parentName = "[@Liteprint]"; // + from.name + "@" + from.GetInstanceID() + "]";

                GameObject parent = GameObject.Find(parentName);
                if (parent == null)
                    parent = new GameObject(parentName);

                _parent = parent.transform;
            }


            return _parent;
        }


        /// <summary>
        /// Makes sures there is a quantity of free pool elements ready to be used.
        /// </summary>
        public static void liteRefill(this Transform prefab, int quantity, Vector3 position)
        {
            PrepareInternalData(prefab);


            // Take in consideration the current free elements
            quantity = quantity <= _readyPool[prefab].Count ? 0 : quantity - _readyPool[prefab].Count;

            while (quantity-- > 0)
            {
                Transform newClone = MonoBehaviour.Instantiate(prefab, position, Quaternion.identity) as Transform;
                newClone.parent = GetParent();
                _readyPool[prefab].Add(newClone);
            }
        }


        /// <summary>
        /// Returns a clone from the pool based on the current Transform.
        /// </summary>
        public static Transform liteInstantiate(this Transform prefab, Vector3 position, Quaternion rotation)
        {
            PrepareInternalData(prefab);


            // If empty, create
            if (_readyPool[prefab].Count < 1)
                prefab.liteRefill(1, position);

            // Take the First
            Transform spawn = _readyPool[prefab][0];

            // Retry on null
            if (spawn == null)
            {
                _readyPool[prefab].RemoveAt(0);
                return prefab.liteInstantiate(position, rotation);
            }


            // Allocation
            spawn.gameObject.SetActive(true); // Just in case
            spawn.position = position;
            spawn.rotation = rotation;
            spawn.parent = GetParent();

            // Pool swap
            _readyPool[prefab].RemoveAt(0);
            _outPool[spawn] = prefab;


            return spawn;
        }


        /// <summary>
        /// Use this on a clone to put him back to his pool for reuse. True on
        /// success, False if the Transform doesn't belong to any pool.
        /// </summary>
        public static bool liteRecycle(this Transform usedInstance)
        {
            PrepareInternalData();


            if (_outPool.ContainsKey(usedInstance))
            {
                _readyPool[_outPool[usedInstance]].Add(usedInstance);
                _outPool.Remove(usedInstance);

                return true;
            }

            return false;
        }


        /// <summary>
        /// Cleans the pool and destroys all elements for the current transform.
        /// </summary>
        public static void liteFlush(this Transform prefab)
        {
            PrepareInternalData(prefab);


            if (_readyPool.ContainsKey(prefab))
            {
                for (int i = 0, len = _readyPool[prefab].Count; i < len; i++)
                    MonoBehaviour.Destroy(_readyPool[prefab][i].gameObject);
                _readyPool[prefab].Clear();

                // #todo The _outPool needs cleaning
            }
        }
    }
}
