using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Varwin.Data;
using Varwin.Public;

namespace Varwin.Editor
{
    public static class AsmdefUtils
    {
        public static Dictionary<string, AssemblyDefinitionData> AllAsmdefInProject { get; private set; }
        public static Dictionary<AssemblyDefinitionData, string> AllAsmdefInProjectReverse { get; private set; }
        public static Dictionary<string, AssemblyDefinitionData> AllAsmdefInProjectByGuid { get; private set; }

        public static AssemblyDefinitionAsset LoadAsmdefAsset(FileInfo fileInfo)
        {
            return LoadAsmdefAsset(fileInfo.FullName);
        }

        public static AssemblyDefinitionAsset LoadAsmdefAsset(string path)
        {
            var rawAsset =  AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(path);
            if (rawAsset == null)
            { 
               var localPath = Path.GetRelativePath(Application.dataPath.Replace("Assets", ""), path);
               rawAsset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(localPath);
            }

            return rawAsset;
        }

        public static AssemblyDefinitionData LoadAsmdefData(FileInfo fileInfo)
        {
            return LoadAsmdefData(fileInfo.FullName);
        }

        public static AssemblyDefinitionData LoadAsmdefData(string path)
        {
            string asmdefJson = File.ReadAllText(path);
            return asmdefJson.JsonDeserialize<AssemblyDefinitionData>();
        }

        public static FileInfo FindAsmdefByName(string asmdefName)
        {
            FindAllAsmdefInProject();

            const string guidReference = "GUID:";

            if (asmdefName.Contains(guidReference))
            {
                var guid = asmdefName.Replace(guidReference, "");

                var guidedAsmdef = AllAsmdefInProjectByGuid.FirstOrDefault(x => x.Key == guid);
                if (!string.IsNullOrEmpty(guidedAsmdef.Key))
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guidedAsmdef.Key);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        return new FileInfo(assetPath);
                    }
                }
            }

            var targetAsmdef = AllAsmdefInProject.FirstOrDefault(x => x.Value.name == asmdefName);

            if (!string.IsNullOrEmpty(targetAsmdef.Key))
            {
                return new FileInfo(targetAsmdef.Key);
            }

            return null;
        }

        public static FileInfo FindAsmdef(Type searchType, bool ignoreVarwin = true)
        {
            string monoScriptPath = TypeUtils.FindScript(searchType, ignoreVarwin);

            if (!string.IsNullOrEmpty(monoScriptPath))
            {
                return FindAsmdef(monoScriptPath);
            }

            return null;
        }

        public static FileInfo FindAsmdef(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return FindAsmdef(File.Exists(path) ? new FileInfo(path).Directory : new DirectoryInfo(path));
        }

        public static FileInfo FindAsmdef(FileInfo fileInfo)
        {
            return FindAsmdef(fileInfo.Directory);
        }

        public static FileInfo FindAsmdef(DirectoryInfo directoryInfo)
        {
            var root = new DirectoryInfo("Assets");

            FileInfo targetAsmdef = null;
            while (targetAsmdef == null)
            {
                var asmdefs = directoryInfo.GetFiles("*.asmdef");
                if (asmdefs.Length > 0)
                {
                    targetAsmdef = asmdefs[0];
                }
                else
                {
                    directoryInfo = directoryInfo.Parent;
                    if (string.Equals(root.FullName, directoryInfo?.FullName, StringComparison.OrdinalIgnoreCase) || directoryInfo == null)
                    {
                        return null;
                    }
                }
            }

            return targetAsmdef;
        }

        public static void CollectReferences(AssemblyDefinitionData asmdef)
        {
            var references = asmdef.references;
            var dlls = asmdef.precompiledReferences;

            bool referencesIsChanged = false;

            if (references == null)
            {
                return;
            }

            var linkedReferences = new LinkedList<string>(references);
            var linkedPrecompiledReferences = new LinkedList<string>();
            if (dlls != null)
            {
                foreach (string dll in dlls)
                {
                    linkedPrecompiledReferences.AddLast(dll);
                }
            }

            foreach (string reference in references)
            {
                if (reference == "VarwinCore")
                {
                    continue;
                }
                
                if (reference == "Unity.Netcode.Runtime")
                {
                    continue;
                }

                FileInfo referencedAsmdefFile = FindAsmdefByName(reference);

                if (referencedAsmdefFile == null)
                {
                    throw new NullReferenceException("Missing assembly definition reference: " + reference);
                }

                if (referencedAsmdefFile.GetAssetPath().StartsWith("Library/"))
                {
                    continue;
                }

                AssemblyDefinitionData referencedAsmdefData = LoadAsmdefData(referencedAsmdefFile);
                CollectReferences(referencedAsmdefData);

                if (referencedAsmdefData.references != null)
                {
                    foreach (string innerReference in referencedAsmdefData.references)
                    {
                        if (!linkedReferences.Contains(innerReference))
                        {
                            linkedReferences.AddFirst(innerReference);
                            referencesIsChanged = true;
                        }
                    }
                }

                if (referencedAsmdefData.precompiledReferences != null)
                {
                    foreach (string innerPrecompiledReference in referencedAsmdefData.precompiledReferences)
                    {
                        if (!linkedPrecompiledReferences.Contains(innerPrecompiledReference))
                        {
                            linkedPrecompiledReferences.AddFirst(innerPrecompiledReference);
                            referencesIsChanged = true;
                        }
                    }
                }
            }

            if (referencesIsChanged)
            {
                asmdef.references = linkedReferences.ToArray();
                if (linkedPrecompiledReferences.Count > 0)
                {
                    asmdef.precompiledReferences = linkedPrecompiledReferences.ToArray();
                    asmdef.overrideReferences = true;
                }
                else
                {
                    asmdef.overrideReferences = false;
                }

                FileInfo file = FindAsmdefByName(asmdef.name);
                if (file != null)
                {
                    asmdef.Save(file.FullName);
                }
            }
        }

        public static bool HasMissingReferences(VarwinObjectDescriptor descriptor)
        {
            var data = GetAssemblyDefinitionData(descriptor);
            return data.references.Any(string.IsNullOrEmpty) || data.references.Any(x => FindAsmdefByName(x) == null);
        }
        
        public static AssemblyDefinitionData GetAssemblyDefinitionData(VarwinObjectDescriptor varwinObjectDescriptor)
        {
            string prefabPath = varwinObjectDescriptor.Prefab;
            if (!string.IsNullOrEmpty(varwinObjectDescriptor.PrefabGuid))
            {
                prefabPath = AssetDatabase.GUIDToAssetPath(varwinObjectDescriptor.PrefabGuid);
                varwinObjectDescriptor.Prefab = prefabPath;
            }

            string asmdefPath = FindAsmdef(prefabPath)?.FullName;

            if (string.IsNullOrEmpty(asmdefPath))
            {
                return null;
            }

            AssemblyDefinitionData asmdefData = LoadAsmdefData(asmdefPath);

            return asmdefData;
        }

        public static void Refresh()
        {
            AllAsmdefInProject = null;
            AllAsmdefInProjectReverse = null;
            FindAllAsmdefInProject();
        }

        public static void FindAllAsmdefInProject()
        {
            if (AllAsmdefInProject != null)
            {
                return;
            }

            AssetDatabase.Refresh();
            var asmdefPaths = TypeUtils.FindAllObjectsOfType<AssemblyDefinitionAsset>(SearchResultType.Path, false);
            AllAsmdefInProject = new();
            AllAsmdefInProjectReverse = new();
            AllAsmdefInProjectByGuid = new ();

            foreach (var asmdefPath in asmdefPaths)
            {
                var asmdefData = LoadAsmdefData(asmdefPath);
                AllAsmdefInProject.Add(asmdefPath, asmdefData);
                AllAsmdefInProjectReverse.Add(asmdefData, asmdefPath);
                AllAsmdefInProjectByGuid.Add(AssetDatabase.GUIDFromAssetPath(asmdefPath).ToString(), asmdefData);
            }
        }
    }
}