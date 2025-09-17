using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using System.IO;

namespace Varwin.Editor
{
    public class ShadersDependenciesCollector 
    {
        public IEnumerable<string> GetPaths(IEnumerable<string> paths)
        {
            var shadersFolders = GetShaderFolders(paths);           
            return GetCgIncPaths(shadersFolders);
        }

        private static IEnumerable<string> GetShaderFolders(IEnumerable<string> paths)
        {
            var result = new List<string>();

            foreach (var path in paths)
            {
                var importer = AssetImporter.GetAtPath(path) as ShaderImporter;
                if (importer != null)
                {
                    result.Add(Path.GetDirectoryName(path));
                }
            }
            
            return result;
        }

        private static IEnumerable<string> GetCgIncPaths(IEnumerable<string> folders)
        {
            var result = new List<string>();

            var array = folders.ToArray();
            if (array.Length == 0)
            {
                return result;
            }

            var assetGuids = AssetDatabase.FindAssets("t:textasset", array);
            foreach (var guid in assetGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains(".cginc"))
                {
                    result.Add(path);
                }
            }
            
            return result;
        }
    }
}