using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Varwin.Data;
using Varwin.Public;

namespace Varwin.Editor
{
    public class DeprecatedDependenciesCollector
    {
        public IEnumerable<string> GetPaths(Object prefab)
        {
            var result = new List<string>();

            var path = AssetDatabase.GetAssetPath(prefab);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var descriptor = go.GetComponent<VarwinObjectDescriptor>();

            if (descriptor != null)
            {
                var folder = Path.GetDirectoryName(descriptor.Prefab);
                result.Add(folder + "/" + descriptor.Name + "Type.cs");
                result.Add(folder + "/" + descriptor.Name + "Wrapper.cs");
            }

            return result;
        }
    }
}