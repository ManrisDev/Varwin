using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin.Public;

namespace Varwin.Core.Behaviours
{
    public class VarwinBehaviourHelper
    {
        private static readonly Dictionary<Type, List<Type>> ProtectedComponentsCache = new();
        private static readonly Dictionary<Type, List<Type>> BehavioursRequiredComponentsCache = new();
        private static readonly Dictionary<Type, HashSet<BehaviourType>> DisabledBehavioursCache = new();

        public static void ClearCache()
        {
            ProtectedComponentsCache.Clear();
            DisabledBehavioursCache.Clear();
        }

        public virtual bool CanAddBehaviour(GameObject gameObject, Type behaviourType)
        {
            if (IsDisabledBehaviour(gameObject))
            {
                return false;
            }

            if (!BehavioursRequiredComponentsCache.ContainsKey(behaviourType))
            {
                var requiredComponentTypes = behaviourType
                    .GetCustomAttributes(typeof(RequireComponentInChildrenAttribute), true)
                    .OfType<RequireComponentInChildrenAttribute>()
                    .Select(x => x.RequiredComponent);

                BehavioursRequiredComponentsCache.Add(behaviourType, requiredComponentTypes.ToList());
            }

            var protectedComponents = new List<Type>();
            var objectMonoBehaviours = gameObject.GetComponentsInChildren<MonoBehaviour>(true);

            foreach (var childBehaviour in objectMonoBehaviours)
            {
                TryAddToProtectedCache(childBehaviour.GetType());
                protectedComponents.AddRange(ProtectedComponentsCache[childBehaviour.GetType()]);
            }

            foreach (var type in BehavioursRequiredComponentsCache[behaviourType])
            {
                if (objectMonoBehaviours.Any(x => x.GetType() == type))
                {
                    return false;
                }

                if (protectedComponents.Contains(type))
                {
                    return false;
                }
            }

            if (gameObject.GetComponentInChildren<VarwinBot>())
            {
                return false;
            }

            return !gameObject.GetComponent(behaviourType);
        }

        public virtual bool IsDisabledBehaviour(GameObject targetGameObject)
        {
            return false;
        }

        private static void TryAddToProtectedCache(Type baseType)
        { 
            if(ProtectedComponentsCache.ContainsKey(baseType))
                return;

            ProtectedComponentsCache[baseType] = baseType.GetCustomAttributes(typeof(ProtectComponentAttribute), true)
                .OfType<ProtectComponentAttribute>()
                .Select(x => x.ProtectedComponent)
                .ToList();
        }
        
        protected bool IsDisabledBehaviour(GameObject targetGameObject, BehaviourType behaviourType)
        {
            foreach (var objectBehaviour in targetGameObject.GetComponentsInChildren<MonoBehaviour>())
            {
                var monoBehaviourType = objectBehaviour.GetType();
                if (!DisabledBehavioursCache.ContainsKey(monoBehaviourType))
                {
                    var disabledBehaviours = monoBehaviourType.GetCustomAttributes(typeof(DisableDefaultBehaviourAttribute), true)
                        .OfType<DisableDefaultBehaviourAttribute>()
                        .Select(x => x.BehaviourType)
                        .ToHashSet();

                    DisabledBehavioursCache.Add(monoBehaviourType, disabledBehaviours);
                }

                if (DisabledBehavioursCache[monoBehaviourType].Contains(behaviourType))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
