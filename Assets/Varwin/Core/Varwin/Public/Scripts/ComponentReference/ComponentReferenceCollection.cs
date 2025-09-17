using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Varwin.Core.Behaviours;
using UnityEngine;

namespace Varwin.Public
{
    [Serializable]
    public class ComponentReferenceCollection : IList<ComponentReference>
    {
        [SerializeField] public List<ComponentReference> ComponentReferences;

        public ComponentReferenceCollection()
        {
            ComponentReferences = new List<ComponentReference>();
        }
        
        public IEnumerator<ComponentReference> GetEnumerator()
        {
            return ComponentReferences.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(ComponentReference item)
        {
            ComponentReferences.Add(item);
        }

        public void Clear()
        {
            ComponentReferences.Clear();
        }

        public bool Contains(ComponentReference item)
        {
            return ComponentReferences.Contains(item);
        }

        public void CopyTo(ComponentReference[] array, int arrayIndex)
        {
            ComponentReferences.CopyTo(array, arrayIndex);
        }

        public bool Remove(ComponentReference item)
        {
            return ComponentReferences.Remove(item);
        }

        public int Count => ComponentReferences.Count;
        public bool IsReadOnly => false;
        
        public int IndexOf(ComponentReference item)
        {
            return ComponentReferences.IndexOf(item);
        }

        public void Insert(int index, ComponentReference item)
        {
            ComponentReferences.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ComponentReferences.RemoveAt(index);
        }

        public ComponentReference this[int index]
        {
            get => ComponentReferences[index];
            set => ComponentReferences[index] = value;
        }

        public void Setup(GameObject gameObject)
        {
            IEnumerable<MonoBehaviour> monoBehaviours = GetGenericMonoBehaviours(gameObject);
            
            SetupComponentReferences(monoBehaviours);
        }

        public void SetupRuntimeBehaviours(GameObject gameObject)
        {
            IEnumerable<MonoBehaviour> monoBehaviours = gameObject.GetComponentsInChildren<MonoBehaviour>(true)
                .Where(x => x is VarwinBehaviour);
            
            SetupComponentReferences(monoBehaviours);
        }
        
        private void SetupComponentReferences(IEnumerable<MonoBehaviour> monoBehaviours)
        {
            RemoveEmptyComponentReferences();

            HashSet<string> componentReferenceUniqueNames = GetUniqueComponentReferencesNames();

            foreach (var monoBehaviour in monoBehaviours)
            {
                if (ComponentReferences.FirstOrDefault(x => x.Component == monoBehaviour) != null)
                {
                    continue;
                }

                var componentReference = new ComponentReference
                {
                    Name = GenerateUniqueComponentReferenceName(monoBehaviour, componentReferenceUniqueNames),
                    Component = monoBehaviour
                };
                componentReferenceUniqueNames.Add(componentReference.Name);

                ComponentReferences.Add(componentReference);
            }
        }
        
        private void RemoveEmptyComponentReferences()
        {
            for (int i = ComponentReferences.Count - 1; i >= 0; --i)
            {
                var componentReference = ComponentReferences[i];
                if (componentReference == null || !componentReference.Component || string.IsNullOrEmpty(componentReference.Name))
                {
                    ComponentReferences.Remove(componentReference);
                }
            }
        }

        private static IEnumerable<MonoBehaviour> GetGenericMonoBehaviours(GameObject gameObject)
        {
            return gameObject.GetComponentsInChildren<MonoBehaviour>(true)
                .Where(x => x && x.GetType() != typeof(VarwinObjectDescriptor) && x.GetType() != typeof(ObjectId));
        }

        private HashSet<string> GetUniqueComponentReferencesNames()
        {
            var componentReferenceUniqueNames = new HashSet<string>();

            foreach (var componentReference in ComponentReferences)
            {
                if (componentReferenceUniqueNames.Contains(componentReference.Name))
                {
                    componentReference.Name = GenerateUniqueComponentReferenceName(componentReference.Component, componentReferenceUniqueNames);
                }
                componentReferenceUniqueNames.Add(componentReference.Name);
            }

            return componentReferenceUniqueNames;
        }
        
        private static string GenerateUniqueComponentReferenceName(MonoBehaviour monoBehaviour, ICollection<string> uniqueNames)
        {
            int index = 0;
            string baseName = $"{monoBehaviour.GetType().Name}_";
            
            if (uniqueNames != null)
            {
                while (uniqueNames.Contains(baseName + index))
                {
                    index++;
                }
            }
            
            return baseName + index;
        }
    }
}