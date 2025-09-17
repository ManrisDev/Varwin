using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Varwin.Editor
{
    public class StandardDependenciesCollector 
    {
        public IEnumerable<string> GetPaths(Object prefab)
        {
            var objects = GetObjects(prefab);
            var paths = GetPaths(objects);
            
            paths = Exclude(paths, "Library/unity default resources");
            paths = Exclude(paths, "Resources/unity_builtin_extra");
            paths = Exclude(paths, "C:/Program Files/");
            paths = Exclude(paths, "Assets/Varwin");
            
            return paths;
        }

        private static IEnumerable<Object> GetObjects(Object target)
        {           
            return EditorUtility.CollectDependencies(new[] {target});
        }      
        
        private static IEnumerable<string> GetPaths(IEnumerable<Object> objects)
        {
            return objects.Select(AssetDatabase.GetAssetPath);
        }
        
        private static IEnumerable<string> Exclude(IEnumerable<string> paths, string filter)
        {
            return paths.Where(x => !x.Contains(filter));
        }
    }
}
