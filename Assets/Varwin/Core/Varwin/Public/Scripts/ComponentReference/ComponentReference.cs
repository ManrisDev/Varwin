using System;
using UnityEngine;

namespace Varwin.Public
{
    [Serializable]
    public class ComponentReference
    {
        [SerializeField] public string Name;
        [SerializeField] public MonoBehaviour Component;

        public Type Type => Component ? Component.GetType() : null;

        public bool IsVarwinObject => Component && Component is VarwinObject;
        
        public string PrefixName => IsVarwinObject ? string.Empty : $"{Name}_";
    }
}
