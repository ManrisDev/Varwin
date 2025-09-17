using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Varwin.Editor
{
    public class VarwinDependenciesCollector
    {
        public IEnumerable<string> CollectPathsForObject(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);
            IEnumerable<string> resultPaths = new List<string>() {prefabPath};
            
            var standardCollector = new StandardDependenciesCollector();
            var standardPaths = standardCollector.GetPaths(prefab);
            resultPaths = resultPaths.Union(standardPaths);
        
            var asmDefsCollector = new ScriptsDependenciesCollector();
            var asmDefsPaths = asmDefsCollector.GetPaths(resultPaths);
            resultPaths = resultPaths.Union(asmDefsPaths);
            
            var deprecatedCollector = new DeprecatedDependenciesCollector();
            var deprecatedPaths = deprecatedCollector.GetPaths(prefab);
            resultPaths = resultPaths.Union(deprecatedPaths);
            
            var shadersCollector = new ShadersDependenciesCollector();
            var shadersPaths = shadersCollector.GetPaths(resultPaths);
            resultPaths = resultPaths.Union(shadersPaths);
            
            return resultPaths;
        }
        
        public IEnumerable<string> CollectPathsForScene(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);
            IEnumerable<string> resultPaths = new List<string>() {prefabPath};
            
            var standardCollector = new StandardDependenciesCollector();
            var standardPaths = standardCollector.GetPaths(prefab);
            resultPaths = resultPaths.Union(standardPaths);
            
            return resultPaths;
        }
    }
}