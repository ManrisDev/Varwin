using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Varwin
{
    public static class BoundsUtils
    {
        public static Bounds GetBounds(IEnumerable<Transform> targets, bool considerRenderers, bool considerColliders)
        {
            if (targets == null)
            {
                return new Bounds(Vector3.zero, Vector3.one);
            }

            var array = targets.ToArray();
            if (array.Length == 0)
            {
                return new Bounds(Vector3.zero, Vector3.one);
            }
            
            var groupBounds = GetBounds(array[0], considerRenderers, considerColliders);
            for (int i = 1; i < array.Length; i++)
            {
                var itemBounds = GetBounds(array[i], considerRenderers, considerColliders);
                groupBounds.Encapsulate(itemBounds);
            }

            return groupBounds;
        }
        
        public static Bounds GetBounds(Transform target, bool considerRenderers, bool considerColliders)
        {
            var bounds = new Bounds(target.position, Vector3.zero);
                
            if (considerRenderers)
            {
                foreach (var renderer in target.GetComponentsInChildren<Renderer>())
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (considerColliders)
            {
                foreach (var collider in target.GetComponentsInChildren<Collider>())
                {
                    bounds.Encapsulate(collider.bounds);
                }
            }

            return bounds;
        }
    }
}