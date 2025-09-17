using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Varwin.Data;

namespace Varwin.Editor
{
    public class ScriptsDependenciesCollector
    {
        public IEnumerable<string> GetPaths(IEnumerable<string> sourcePaths)
        {
            var defs = GetAllDefsInProject();
            var scripts = GetScripts(sourcePaths);
            var assemblies = GetAssemblies(scripts);
            return SelectDefs(assemblies, defs);
        }
        
        private static Dictionary<string, string> GetAllDefsInProject()
        {
            var result = new Dictionary<string, string>();
            
            var assets = AssetDatabase.FindAssets("t: asmdef");           
            foreach (var guid in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                var json = File.ReadAllText(path);
                var asm = json.JsonDeserialize<AssemblyDefinitionData>();
                
                result.Add(asm.name, path);
            }
            
            return result;
        }
        
        private static IEnumerable<MonoImporter> GetScripts(IEnumerable<string> paths)
        {
            var result = new List<MonoImporter>();

            foreach (var path in paths)
            {
                var importer = AssetImporter.GetAtPath(path) as MonoImporter;
                if (importer != null)
                {
                    result.Add(importer);
                }
            }
            
            return result;
        }
        
        private static IEnumerable<string> GetAssemblies(IEnumerable<MonoImporter> importers)
        {
            var result = new List<string>();

            foreach (var item in importers)
            {
                var assembly = item.GetScript().GetClass().Assembly;
                result.Add(assembly.GetName().Name);
            }
            
            return result;
        }
        
        private static IEnumerable<string> SelectDefs(IEnumerable<string> assemblies, IReadOnlyDictionary<string, string> defsDictionary)
        {
            var result = new List<string>();

            foreach (var assembly in assemblies)
            {
                if (defsDictionary.ContainsKey(assembly))
                {
                    result.Add(defsDictionary[assembly]);
                }
            }
            
            return result;
        }
    }
}