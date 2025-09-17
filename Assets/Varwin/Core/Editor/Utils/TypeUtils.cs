using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public static class TypeUtils
    {
        private static string[] _allMonoScriptsInProject;

        public static List<string> FindAllObjectsOfType<T>(SearchResultType searchResultType, bool ignoreVarwin = true) where T : UnityEngine.Object
        {
            var allObjectGuids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            var result = new List<string>();
            
            foreach (string guid in allObjectGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                if (ignoreVarwin && prefabPath.StartsWith("Assets/Varwin/"))
                {
                    continue;
                }
                
                result.Add(searchResultType == SearchResultType.GUID ? guid : prefabPath);
            }
            
            return result;
        }
        
        public static List<string> FindAllPrefabsOfType<T>(SearchResultType searchResultType, bool ignoreVarwin = true) where T : Component
        {
            var allPrefabsGuids = AssetDatabase.FindAssets("t:Prefab");
            var result = new List<string>();
            
            foreach (string guid in allPrefabsGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                if (ignoreVarwin && prefabPath.StartsWith("Assets/Varwin/"))
                {
                    continue;
                }
                
                var prefabGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefabGameObject && prefabGameObject.GetComponentsInChildren<T>().Length > 0)
                {
                    result.Add(searchResultType == SearchResultType.GUID ? guid : prefabPath);
                }
            }
            
            return result;
        }

        public static T GetPrefabComponentAtPath<T>(string path) where T : Component
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return prefab ? prefab.GetComponent<T>() : null;
        }
        
        public static bool IsImplementsInterface(this Type type, Type interfaceType)
        {
            var interfaces = type.GetInterfaces();
            return interfaces.Contains(interfaceType);
        }
        
        public static bool IsInheritorOf(this Type type, Type other)
        {
            if (other == null)
            {
                return false;
            }

            if (type == other)
            {
                return true;
            }
            
            return type.IsSubclassOf(other) || type.IsAssignableFrom(other) || type.BaseType != null && type.BaseType == other;
        }

        public static string FindScript(Type type, bool ignoreVarwin = true)
        {
            if (_allMonoScriptsInProject == null)
            {
                _allMonoScriptsInProject = AssetDatabase.FindAssets("t:MonoScript");
            }

            foreach (string monoScriptGuid in _allMonoScriptsInProject)
            {
                string monoScriptPath = AssetDatabase.GUIDToAssetPath(monoScriptGuid);
                if (ignoreVarwin && monoScriptPath.StartsWith("Assets/Varwin/"))
                {
                    continue;
                }

                var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(monoScriptPath);
                if (monoScript.GetClass() == type)
                {
                    return monoScriptPath;
                }
            }

            return null;
        }
    }

    public enum SearchResultType
    {
        Path,
        GUID
    };
}