using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;

using UnityEditorInternal;
using UnityEngine;
using Varwin.Public;

namespace Varwin.Editor
{
    public static class VarwinObjectUtils
    {
        public static IEnumerable<VarwinObjectDescriptor> GetSelected()
        {
            var selection = new List<UnityEngine.Object>();
            if (Selection.objects != null && Selection.objects.Length > 0)
            {
                selection.AddRange(Selection.objects);
            }
            else
            {
                selection.AddRange(Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets));
            }
            
            var selectedObjects = new List<UnityEngine.Object>();
            foreach (var selected in selection)
            {
                string selectionFolderPath = AssetDatabase.GetAssetPath(selected);
                    
                if (Directory.Exists(selectionFolderPath))
                {
                    var prefabs = new DirectoryInfo(selectionFolderPath).GetFiles("*.prefab", SearchOption.AllDirectories);
                    selectedObjects.AddRange(prefabs.Select(x => AssetDatabase.LoadAssetAtPath<GameObject>(x.GetAssetPath())));
                }
                else
                {
                    selectedObjects.Add(selected);
                }
            }

            var varwinObjects = new List<VarwinObjectDescriptor>();
            foreach (var selectedObject in selectedObjects)
            {
                if (selectedObject is GameObject selectedGameObject)
                {
                    var varwinObjectDescriptor = selectedGameObject.GetComponent<VarwinObjectDescriptor>();
                    if (varwinObjectDescriptor)
                    {
                        varwinObjects.Add(varwinObjectDescriptor);
                    }
                }
            }

            return varwinObjects;
        }
        
        public static IEnumerable<VarwinObjectDescriptor> GetAllOnScene()
        {
            var varwinObjectDescriptors = new List<VarwinObjectDescriptor>();
            
            UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage == null)
            {
                varwinObjectDescriptors.AddRange(UnityEngine.Object.FindObjectsOfType<VarwinObjectDescriptor>().Where(x => x.gameObject.activeInHierarchy));
            }
            else
            {
                var varwinObjectDescriptor = prefabStage.prefabContentsRoot.GetComponent<VarwinObjectDescriptor>();
                if (varwinObjectDescriptor)
                {
                    varwinObjectDescriptors.Add(varwinObjectDescriptor);
                }
            }

            return varwinObjectDescriptors;
        }

        public static IEnumerable<VarwinObjectDescriptor> GetAllObjectsInProject()
        {
            var allVarwinObjectPaths = TypeUtils.FindAllPrefabsOfType<VarwinObjectDescriptor>(SearchResultType.Path);
            return allVarwinObjectPaths.Select(AssetDatabase.LoadAssetAtPath<VarwinObjectDescriptor>);
        }

        public static IEnumerable<VarwinPackageDescriptor> GetAllPackagesInProject()
        {
            var allVarwinObjectPaths = TypeUtils.FindAllObjectsOfType<VarwinPackageDescriptor>(SearchResultType.Path);
            return allVarwinObjectPaths.Select(AssetDatabase.LoadAssetAtPath<VarwinPackageDescriptor>);
        }
        
        public static VarwinPackageDescriptor GetPackageDescriptorByName(string packageName)
        {
            var allVarwinPackagePaths = TypeUtils.FindAllObjectsOfType<VarwinPackageDescriptor>(SearchResultType.Path);
            foreach (var varwinPackagePath in allVarwinPackagePaths)
            {
                if (Path.GetFileNameWithoutExtension(varwinPackagePath).ToLower(CultureInfo.InvariantCulture).Equals(packageName.ToLower(CultureInfo.InvariantCulture)))
                {
                    return AssetDatabase.LoadAssetAtPath<VarwinPackageDescriptor>(varwinPackagePath);
                }
            }

            return null;
        }

        
        public static IEnumerable<VarwinObjectDescriptor> GetByAsmdef(IEnumerable<AssemblyDefinitionAsset> asmdefs)
        {
            return GetByAsmdef(asmdefs.Select(asmdef => asmdef.GetData()));
        }

        public static IEnumerable<VarwinObjectDescriptor> GetByAsmdef(IEnumerable<AssemblyDefinitionData> asmdefs)
        {
            var assemblyDefinitionDatas = asmdefs as AssemblyDefinitionData[] ?? asmdefs.ToArray();
            var varwinObjectDescriptors = new List<VarwinObjectDescriptor>();
            
            var allVarwinObjectPaths = TypeUtils.FindAllPrefabsOfType<VarwinObjectDescriptor>(SearchResultType.Path);
            
            foreach (string varwinObjectPath in allVarwinObjectPaths)
            {
                var varwinObjectDescriptor = AssetDatabase.LoadAssetAtPath<GameObject>(varwinObjectPath).GetComponent<VarwinObjectDescriptor>();

                var varwinObjectDescriptorIsAdded = false;
                
                var references = CreateObjectUtils.GetAssembliesReferences(varwinObjectDescriptor).ToList();

                foreach (var assemblyDefinitionData in assemblyDefinitionDatas)
                {
                    if (references.Any(x => string.Equals(x, assemblyDefinitionData.name, StringComparison.InvariantCulture)))
                    {
                        varwinObjectDescriptors.Add(varwinObjectDescriptor);
                        varwinObjectDescriptorIsAdded = true;
                        break;
                    }
                }

                if (varwinObjectDescriptorIsAdded)
                {
                    continue;
                }
                
                FileInfo varwinObjectAsmdef = AsmdefUtils.FindAsmdef(varwinObjectPath);
                if (varwinObjectAsmdef != null)
                {
                    AssemblyDefinitionData asmdef = AsmdefUtils.LoadAsmdefData(varwinObjectAsmdef);
                    foreach (string asmdefReference in asmdef.references)
                    {
                        if (assemblyDefinitionDatas.Any(x => string.Equals(x.name, asmdefReference)))
                        {
                            varwinObjectDescriptors.Add(varwinObjectDescriptor);
                            varwinObjectDescriptorIsAdded = true;
                            break;
                        }
                    }
                }
            }

            return varwinObjectDescriptors;
        }
    }
}