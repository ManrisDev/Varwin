using UnityEditor;
using Varwin.SceneTemplateBuilding;

namespace Varwin.Editor
{
    public static class SdkMenuItems
    {
        #region CREATING

        [MenuItem("Varwin/Create/Object", false, 0)]
        private static void OpenCreateObjectWindow() => CreateObjectWindow.OpenWindow();
        
        [MenuItem("Varwin/Create/Scene Template", false, 1)]
        private static void OpenCreateSceneTemplateWindow() => CreateSceneTemplateWindow.OpenWindow();
        
        [MenuItem("Varwin/Create/Package", false, 2)]
        private static void OpenCreatePackageWindow() => CreatePackageWindow.OpenWindow();
        
        #endregion

        #region OBJECTS BUILDING

        #if VARWIN_DEVELOPER_MODE

        [MenuItem("Varwin/Build/Selected Objects Logic", false,  6)]
        private static void BuildSelectedObjectsLogic() => VarwinBuilderController.BuildSelectedObjectsLogicOnly();

        [MenuItem("Varwin/Build/Selected Objects Logic", true)]
        private static bool CanBuildSelectedObjectsLogic() => VarwinBuilderController.CanBuildSelectedObjects();


        #endif

        [MenuItem("Varwin/Build/Selected Objects", false, 1)]
        private static void BuildSelectedObjects() => VarwinBuilderController.BuildSelectedObjects();

        [MenuItem("Varwin/Build/Selected Objects", true)]
        private static bool CanBuildSelectedObjects() => VarwinBuilderController.CanBuildSelectedObjects();

        
        [MenuItem("Varwin/Build/All Objects On Scene", false, 2)]
        private static void BuildAllObjectsOnScene() => VarwinBuilderController.BuildAllObjectsOnScene();
        
        [MenuItem("Varwin/Build/All Objects On Scene", true)]
        private static bool CanBuildAllObjectsOnScene() => VarwinBuilderController.CanBuildAllObjectsOnScene();
        
        
        [MenuItem("Varwin/Build/All Objects In Project", false, 3)]
        private static void BuildAllObjectsInProject() => VarwinBuilderController.BuildAllObjectInProject();

        [MenuItem("Varwin/Build/All Objects In Project", true)]
        private static bool CanBuildAllObjectsInProject() => VarwinBuilderController.CanBuildAllObjectInProject();
        
        
        [MenuItem("Varwin/Build/All Packages In Project", false, 3)]
        private static void BuildAllPackagesInProject() => VarwinBuilderController.BuildAllPackagesInProject();

        [MenuItem("Varwin/Build/All Packages In Project", true)]
        private static bool CanBuildAllPackagesInProject() => VarwinBuilderController.CanBuildAllPackagesInProject();
        
        
        [MenuItem("Varwin/Build/All Objects By Selected Asmdef", false, 4)]
        private static void BuildAllObjectsByAsmdef() => VarwinBuilderController.BuildAllObjectsBySelectedAsmdef();

        [MenuItem("Varwin/Build/All Objects By Selected Asmdef", true)]
        private static bool CanBuildAllObjectsByAsmdef() => VarwinBuilderController.CanBuildAllObjectsBySelectedAsmdef();
        
        
        [MenuItem("Varwin/Build/All Objects by Selected AssetBundlePart", true)]
        private static bool CanBuildSelectedAssetBundlePart() => VarwinBuilderController.CanBuildSelectedAssetBundlePart();

        [MenuItem("Varwin/Build/All Objects by Selected AssetBundlePart", false, 5)]
        private static void BuildAllObjectsByAssetBundlePart() => VarwinBuilderController.BuildAllObjectsByAssetBundlePart();
        
        #endregion

        #region SETTINGS
        
        [MenuItem("Varwin/Settings/SDK", false, 90)]
        private static void OpenSdkSettingsWindow() => SdkSettingsWindow.OpenWindow();
        
        [MenuItem("Varwin/Settings/Default Author Info", false, 100)]
        private static void OpenAuthorSettingsWindow() => AuthorSettingsWindow.OpenWindow();
        
        [MenuItem("Varwin/Settings/Project Settings", false, 110)]
        private static void OpenVarwinUnitySettingsWindow() => VarwinUnitySettingsWindow.OpenWindow(true);
        
        #endregion

        #region OTHER

        [MenuItem("Varwin/Check for update", false, 910)]
        private static void OpenSdkUpdateWindow() => SdkUpdateWindow.OpenWindow();      

        [MenuItem("Varwin/About", false, 920)]
        private static void OpenVarwinAboutWindow() => VarwinAboutWindow.OpenWinow();
        
        #endregion

#if SKETCHFAB

        [MenuItem("Varwin/Import/Model...", false, 1)]
        private static void RunImportModelsWindow() => ImportModelsWindow.InitModelImportWindow();

        [MenuItem("Varwin/Import/Folder...", false, 1)]
        private static void RunImportFolderWindow() => ImportModelsWindow.InitFolderImportWindow();

#endif
        
    }
}