using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Varwin.Core;

namespace Varwin
{
    public static class VarwinMembersHelper
    {
        private static Dictionary<Type, MemberInfo[]> _typesMembers = new Dictionary<Type, MemberInfo[]>();
        
        public static bool IsVarwinInspector(MonoBehaviour monoBehaviour)
        {
            return IsVarwinInspector(monoBehaviour.GetType());
        }
        
        public static bool IsVarwinInspector(Type type)
        {
            return GetVarwinInspectorMembers(type).Length > 0;
        }

        public static MemberInfo[] GetVarwinInspectorMembers(MonoBehaviour monoBehaviour)         
        {
            return GetVarwinInspectorMembers(monoBehaviour.GetType());
        }

        public static MemberInfo[] GetVarwinInspectorMembers(Type type)
        {
            if (_typesMembers.ContainsKey(type))
            {
                return _typesMembers[type];
            }
            
            var members = type
                .GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(member => member.FastGetCustomAttribute<VarwinSerializableAttribute>(true) != null || member.FastGetCustomAttribute<VarwinInspectorAttribute>(true) != null)
                .ToArray();

            _typesMembers.Add(type, members);

            return members;
        }
    }
}