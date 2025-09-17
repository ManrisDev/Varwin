using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

using UnityEditorInternal;
using UnityEngine;
using Varwin.Public;

namespace Varwin.Editor
{
    public static class VarwinBuilderController
    {
        #region BUILD SELECTED OBJECTS

        public static void BuildSelectedObjects()
        {
            if (!VarwinBuilderWindow.VersionExists() || !CanBuildSelectedObjects())
            {
                return;
            }

            VarwinBuilderWindow window = VarwinBuilderWindow.GetWindow("Building selected objects");
            window.Build(VarwinObjectUtils.GetSelected());
        }

        public static void BuildSelectedObjectsLogicOnly()
        {
            if (!VarwinBuilderWindow.VersionExists() || !CanBuildSelectedObjects())
            {
                return;
            }

            VarwinBuilderWindow window = VarwinBuilderWindow.GetWindow("Building selected objects logic");
            window.BuildLogic(VarwinObjectUtils.GetSelected());
        }

        public static bool CanBuildSelectedObjects()
        {
            if (!CommonCanBuildCheck())
            {
                return false;
            }

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

                if (selectionFolderPath == "Assets")
                {
                    return false;
                }

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

            foreach (var selectedObject in selectedObjects)
            {
                if (selectedObject is GameObject selectedGameObject && selectedGameObject.GetComponent<VarwinObjectDescriptor>())
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region BUILD SCENE OBJECTS

        public static void BuildAllObjectsOnScene()
        {
            if (!VarwinBuilderWindow.VersionExists() || !CanBuildAllObjectsOnScene())
            {
                return;
            }

            if (!SdkSettings.Features.Changelog)
            {
                if (!EditorUtility.DisplayDialog("Confirm Build All Objects On Scene",
                        "Building all objects on scene might take some time, do you want to proceed?",
                        "Yes",
                        "Cancel"))
                {
                    return;
                }
            }

            VarwinBuilderWindow window = VarwinBuilderWindow.GetWindow("Building all objects on scene");
            window.Build(VarwinObjectUtils.GetAllOnScene());
        }

        public static bool CanBuildAllObjectsOnScene()
        {
            if (!CommonCanBuildCheck())
            {
                return false;
            }

            UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage == null)
            {
                return UnityEngine.Object.FindObjectOfType<VarwinObjectDescriptor>();
            }
            else
            {
                return prefabStage.prefabContentsRoot.GetComponent<VarwinObjectDescriptor>();
            }
        }

        #endregion

        #region BUILD PROJECT OBJECTS

        public static void BuildAllObjectsByAssetBundlePart()
        {
            if (!VarwinBuilderWindow.VersionExists() || !CanBuildSelectedAssetBundlePart())
            {
                return;
            }

            var selectedAssetBundlePart = Selection.activeObject as AssetBundlePart;

            if (!selectedAssetBundlePart)
            {
                return;
            }

            var listObjectsForBuilding = VarwinObjectUtils.GetAllObjectsInProject().ToList().FindAll(a => a.AssetBundleParts.Contains(selectedAssetBundlePart));

            if (listObjectsForBuilding.Count == 0)
            {
                return;
            }

            if (!SdkSettings.Features.Changelog)
            {
                if (!EditorUtility.DisplayDialog("Confirm Build All Objects In Project with selected AssetBundle part ",
                        "Building all objects in project might take some time, do you want to proceed?",
                        "Yes",
                        "Cancel"))
                {
                    return;
                }
            }

            VarwinBuilderWindow window = VarwinBuilderWindow.GetWindow("Building all objects with selected assetbundle part");
            window.Build(listObjectsForBuilding);
        }

        public static bool CanBuildSelectedAssetBundlePart()
        {
            if (!CommonCanBuildCheck())
            {
                return false;
            }
            
            return Selection.activeObject != null && Selection.activeObject is AssetBundlePart;
        }

        public static void BuildAllObjectInProject()
        {
            if (!VarwinBuilderWindow.VersionExists())
            {
                return;
            }

            if (!SdkSettings.Features.Changelog)
            {
                if (!EditorUtility.DisplayDialog("Confirm Build All Objects In Project",
                        "Building all objects in project might take some time, do you want to proceed?",
                        "Yes",
                        "Cancel"))
                {
                    return;
                }
            }

            VarwinBuilderWindow window = VarwinBuilderWindow.GetWindow("Building all objects in project");
            window.Build(VarwinObjectUtils.GetAllObjectsInProject());
        }

        public static bool CanBuildAllObjectInProject()
        {
            if (!CommonCanBuildCheck())
            {
                return false;
            }

            return true;
        }

        #endregion

        #region BUILD PROJECT PACKAGES
 
        public static void BuildAllPackagesInProject()
        {
            if (!VarwinBuilderWindow.VersionExists())
            {
                return;
            }

            if (!SdkSettings.Features.Changelog)
            {
                if (!EditorUtility.DisplayDialog("Confirm Build All Packages In Project",
                        "Building all packages in project might take some time, do you want to proceed?",
                        "Yes",
                        "Cancel"))
                {
                    return;
                }
            }

            VarwinBuilderWindow window = VarwinBuilderWindow.GetWindow("Building all packages in project");
            window.Build(VarwinObjectUtils.GetAllPackagesInProject());
        }

        public static bool CanBuildAllPackagesInProject()
        {
            if (!CommonCanBuildCheck())
            {
                return false;
            }

            return true;
        }

        #endregion

        #region BUILD OBJECTS BY SELECTED ASSEMBLY DEFINITION

        public static void BuildAllObjectsBySelectedAsmdef()
        {
            if (!VarwinBuilderWindow.VersionExists() || !CanBuildAllObjectsBySelectedAsmdef())
            {
                return;
            }

            if (!SdkSettings.Features.Changelog)
            {
                if (!EditorUtility.DisplayDialog("Confirm Build All Objects By Selected Asmdef",
                        "Building all objects on scene might take some time, do you want to proceed?",
                        "Yes",
                        "Cancel"))
                {
                    return;
                }
            }

            var selectedObjects = new List<UnityEngine.Object>();

            if (Selection.objects != null && Selection.objects.Length > 0)
            {
                selectedObjects.AddRange(Selection.objects);
            }
            else
            {
                selectedObjects.AddRange(Selection.GetFiltered(typeof(AssemblyDefinitionAsset), SelectionMode.Unfiltered));
            }

            var asmdefs = new List<AssemblyDefinitionAsset>();
            foreach (var selectedObject in selectedObjects)
            {
                if (!selectedObject)
                {
                    continue;
                }

                if (selectedObject is AssemblyDefinitionAsset assemblyDefinitionAsset)
                {
                    asmdefs.Add(assemblyDefinitionAsset);
                    continue;
                }

                string selectedObjectPath = AssetDatabase.GetAssetPath(selectedObject);
                if (Directory.Exists(selectedObjectPath))
                {
                    var folderAsmdefs = new DirectoryInfo(selectedObjectPath).GetFiles("*.asmdef", SearchOption.AllDirectories);
                    asmdefs.AddRange(folderAsmdefs.Select(AsmdefUtils.LoadAsmdefAsset));
                }
            }

            VarwinBuilderWindow window = VarwinBuilderWindow.GetWindow("Building all objects by selected asmdef");
            window.Build(VarwinObjectUtils.GetByAsmdef(asmdefs));
        }

        public static bool CanBuildAllObjectsBySelectedAsmdef()
        {
            if (!CommonCanBuildCheck())
            {
                return false;
            }

            var selectionObjects = new List<UnityEngine.Object>();

            if (Selection.objects != null && Selection.objects.Length > 0)
            {
                selectionObjects.AddRange(Selection.objects);
            }
            else
            {
                selectionObjects.AddRange(Selection.GetFiltered(typeof(AssemblyDefinitionAsset), SelectionMode.TopLevel));
            }

            if (selectionObjects.Count == 0)
            {
                return false;
            }

            if (selectionObjects.Any(x => x is AssemblyDefinitionAsset))
            {
                return true;
            }

            var folders = selectionObjects.Where(x => x is DefaultAsset);
            foreach (var folder in folders)
            {
                string selectedObjectPath = AssetDatabase.GetAssetPath(folder);

                if (Directory.Exists(selectedObjectPath))
                {
                    var folderAsmdefs = new DirectoryInfo(selectedObjectPath).GetFiles("*.asmdef", SearchOption.AllDirectories);
                    if (folderAsmdefs.Length > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        public static void Build(VarwinObjectDescriptor varwinObjectDescriptor)
        {
            if (!VarwinBuilderWindow.VersionExists())
            {
                return;
            }

            VarwinBuilderWindow window = VarwinBuilderWindow.GetWindow("Building object: " + varwinObjectDescriptor.Name);
            window.Build(varwinObjectDescriptor);
        }

        public static void Build(IEnumerable<VarwinObjectDescriptor> varwinObjectDescriptors)
        {
            if (!VarwinBuilderWindow.VersionExists())
            {
                return;
            }

            VarwinBuilderWindow window = VarwinBuilderWindow.GetWindow("Building objects");
            window.Build(varwinObjectDescriptors);
        }


        public static void Build(VarwinPackageDescriptor varwinPackageDescriptor)
        {
            if (!VarwinBuilderWindow.VersionExists())
            {
                return;
            }

            string name = varwinPackageDescriptor.Name.Get(Language.English)?.value;
            if (string.IsNullOrEmpty(name))
            {
                name = varwinPackageDescriptor.name;
            }

            VarwinBuilderWindow window = VarwinBuilderWindow.GetWindow("Building package: " + name);
            window.Build(varwinPackageDescriptor);
        }


        public static void Build(IEnumerable<VarwinPackageDescriptor> varwinPackageDescriptors)
        {
            if (!VarwinBuilderWindow.VersionExists())
            {
                return;
            }

            VarwinBuilderWindow window = VarwinBuilderWindow.GetWindow("Building packages");
            window.Build(varwinPackageDescriptors);
        }

        private static bool CommonCanBuildCheck()
        {
            return VarwinVersionInfo.Exists && !AnyCompilingOrBuilding();
        }

        private static bool AnyCompilingOrBuilding()
        {
            var buildingInProgress = VarwinBuilderWindow.Instance && !VarwinBuilderWindow.Instance.IsFinished;
            return EditorApplication.isCompiling || EditorUtility.scriptCompilationFailed || buildingInProgress;
        }
    }
}