using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Varwin.Editor
{
    public static class DllHelper
    {
        private readonly static string[] _ignoredAsmdefs = new[]
        {
            "VarwinCore",
            "Antilatency.SDK",
            "Illumetry.Sdk",
            "UnityEngine.UI"
        };
        
        public const string VarwinCoreAsmdefPath = "Assets/Varwin/Core/VarwinCore.asmdef";
        
        private static string[] _allAssetsDll;
        private static string[] _allProjectDll;

        public static Dictionary<string, AssemblyType> GetForScripts(IEnumerable<MonoBehaviour> monoBehaviours)
        {
            var dllPaths = new Dictionary<string, AssemblyType>();
            
            var references = CreateObjectUtils.GetAssembliesReferences(monoBehaviours);

            foreach (string reference in references)
            {
                string dllPath;
                try
                {
                    dllPath = FindInAssets(reference + ".dll");
                    dllPaths.Add(dllPath, AssemblyType.Managed);
                    continue;
                }
                catch (DllNotFoundException e)
                {
                }

                dllPath = FindInProject(reference + ".dll");
                dllPaths.Add(dllPath, AssemblyType.Precompiled);
            }

            return dllPaths;
        }
        
        public static Dictionary<string, AssemblyType> GetFromObject(ObjectBuildDescription buildObject)
        {
            if (AsmdefUtils.FindAsmdef(buildObject.FolderPath) != null)
            {
                return GetFromAsmdef(buildObject);
            }

            var dllPaths = new Dictionary<string, AssemblyType>
            {
                {FindInProject($"{buildObject.ObjectName}_{buildObject.Guid}.dll"), AssemblyType.Managed}
            };
            
            var references = CreateObjectUtils.GetAssembliesReferences(buildObject.ContainedObjectDescriptor);

            foreach (string reference in references)
            {
                string dllPath;
                try
                {
                    dllPath = FindInAssets(reference + ".dll");
                    dllPaths.Add(dllPath, AssemblyType.Managed);
                    continue;
                }
                catch (DllNotFoundException e)
                {
                }

                dllPath = FindInProject(reference + ".dll");
                dllPaths.Add(dllPath, AssemblyType.Precompiled);
            }

            return dllPaths;
        }
        
        public static Dictionary<string, AssemblyType> GetFromAsmdef(ObjectBuildDescription buildObject)
        {
            string buildObjectAsmdefPath = AsmdefUtils.FindAsmdef(buildObject.FolderPath)?.FullName;
            
            var dllNamesList = new Dictionary<string, AssemblyType>();

            if (string.IsNullOrEmpty(buildObjectAsmdefPath))
            {
                Debug.LogError($"No .asmdef file found for {buildObject.ObjectName} at {buildObject.FolderPath}");

                return dllNamesList;
            }
            
            if (!File.Exists(buildObjectAsmdefPath))
            {
                Debug.LogError($"No .asmdef file found for {buildObject.ObjectName} at {buildObjectAsmdefPath}");

                return dllNamesList;
            }

            AssemblyDefinitionData buildObjectAsmdef = AsmdefUtils.LoadAsmdefData(buildObjectAsmdefPath);
            AssemblyDefinitionData varwinCoreAsmdef = AsmdefUtils.LoadAsmdefData(VarwinCoreAsmdefPath);

            dllNamesList = new Dictionary<string, AssemblyType>();

            try
            {
                dllNamesList.Add(FindInProject($"{buildObject.ObjectName}_{buildObject.Guid}.dll"), AssemblyType.Managed);
            }
            catch (DllNotFoundException e)
            {
                dllNamesList.Add(FindInProject($"{buildObjectAsmdef.name}.dll"), AssemblyType.Managed);
            }

            foreach (string asmdefName in buildObjectAsmdef.references.Reverse())
            {
                if (!_ignoredAsmdefs.Contains(asmdefName) && !varwinCoreAsmdef.references.Contains(asmdefName))
                {
                    string dll = FindInProject($"{asmdefName}.dll");
                    if (!dllNamesList.ContainsKey(dll))
                    {
                        dllNamesList.Add(dll, AssemblyType.Managed);
                    }
                }
            }

            if (buildObjectAsmdef.precompiledReferences != null)
            {
                foreach (string dllName in buildObjectAsmdef.precompiledReferences.Reverse())
                {
                    if (!_ignoredAsmdefs.Contains(dllName) && !varwinCoreAsmdef.references.Contains(dllName))
                    {
                        string dll = FindInProject(dllName);
                        if (!dllNamesList.ContainsKey(dll))
                        {
                            dllNamesList.Add(dll, AssemblyType.Precompiled);
                        }
                    }
                }
            }
            
            return dllNamesList;
        }
        
        public static string FindInAssets(string dllNameKey)
        {
            if (_allAssetsDll == null)
            {
                _allAssetsDll = Directory.GetFiles(Application.dataPath, "*.dll", SearchOption.AllDirectories);
            }

            foreach (string foundFile in _allAssetsDll)
            {
                if (foundFile.Contains(dllNameKey))
                {
                    return foundFile;
                }
            }
            
            throw new DllNotFoundException();
        }
        
        public static string FindInProject(string dllNameKey)
        {
            if (_allProjectDll == null)
            {
                if (_allAssetsDll == null)
                {
                    _allAssetsDll = Directory.GetFiles(Application.dataPath, "*.dll", SearchOption.AllDirectories);
                }
                
                var allProjectDll = new List<string>();
                allProjectDll.AddRange(_allAssetsDll);
                allProjectDll.AddRange(Directory.GetFiles(VarwinBuildingPath.ScriptAssemblies, "*.dll", SearchOption.AllDirectories));

                _allProjectDll = allProjectDll.ToArray();
            }

            foreach (string foundFile in _allProjectDll)
            {
                if (foundFile != null && dllNameKey == Path.GetFileName(foundFile))
                {
                    return foundFile.Replace("\\", "/");
                }
            }
            
            throw new DllNotFoundException();
        }

        public static void ForceUpdate()
        {
            _allAssetsDll = Directory.GetFiles(Application.dataPath, "*.dll", SearchOption.AllDirectories);
            
            var allProjectDll = new List<string>();
            allProjectDll.AddRange(_allAssetsDll);
            allProjectDll.AddRange(Directory.GetFiles(VarwinBuildingPath.ScriptAssemblies, "*.dll", SearchOption.AllDirectories));

            _allProjectDll = allProjectDll.ToArray();
        }
    }

    public enum AssemblyType
    {
        Managed,
        Precompiled
    }
}