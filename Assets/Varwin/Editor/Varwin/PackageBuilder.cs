using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Varwin.Editor
{
    public static class PackageBuilder
    {
        private const string VarwinOpenXRGlobalDefine = "VARWIN_OPENXR";
        private const string VarwinClientCoreGlobalDefine = "VARWIN_CLIENTCORE";
        private const bool IncludeProjectSettings = true;

        private static readonly string InternalPackageDirectory = Path.Combine(UnityProject.Path, "Assets/InternalPackage");
        private static readonly string ExternalPackageDirectory = Path.Combine(UnityProject.Path, "Assets/SdkSetuppers");
        private static readonly string TempExternalDirectory = Path.Combine(UnityProject.Path, "Temp/ExternalPackage");

        private static readonly string[] ExcludedPaths = new[]
        {
            "Assets/InternalPackage",
            "Assets/MethodInvoker",
            "Assets/Varwin/Core/Client"
        };

        public static void BuildVarwinSdk()
        {                    
            var standaloneDefines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
            
            standaloneDefines = standaloneDefines.Replace(VarwinOpenXRGlobalDefine, "");
            standaloneDefines = standaloneDefines.Replace(VarwinClientCoreGlobalDefine, "");

            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, standaloneDefines);
            
            var androidDefines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android);
            
            androidDefines = androidDefines.Replace(VarwinOpenXRGlobalDefine, "");
            androidDefines = androidDefines.Replace(VarwinClientCoreGlobalDefine, "");

            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android, androidDefines);
            
            var assets = GetPackingAssets("Assets").ToArray();

            if (IncludeProjectSettings)
            {
                assets = assets.Concat(GetProjectSettingsAssets()).ToArray();
            }

            DeleteDirectories(TempExternalDirectory, InternalPackageDirectory);
            Directory.Move(ExternalPackageDirectory, TempExternalDirectory);

            CreateDirectories(InternalPackageDirectory);
            AssetDatabase.ExportPackage(assets.ToArray(), $"{InternalPackageDirectory}/VarwinSDK.unitypackage", ExportPackageOptions.Default);

            PackExternalPackage();
            
            EditorApplication.Exit(0);
        }

        private static List<string> GetPackingAssets(string root)
        {
            var result = new List<string>();
            var directoryInfo = new DirectoryInfo(root);
            
            foreach (var excludedPath in ExcludedPaths)
            {
                var excludedDirectory = new DirectoryInfo(excludedPath);

                if (excludedDirectory.FullName == directoryInfo.FullName)
                {
                    Debug.LogWarning($"Ignoring {excludedDirectory.FullName}");
                    return result;
                }
            }

            foreach (var fileInfo in directoryInfo.GetFiles())
            {
                var relativePath = Path.GetRelativePath(Path.GetDirectoryName(Application.dataPath), fileInfo.FullName);
                result.Add(relativePath);
            }
            
            foreach (var directory in directoryInfo.GetDirectories())
            {
                result.AddRange(GetPackingAssets(directory.FullName));
            }

            return result;
        }

        public static void BuildVarwinSdkWithOpenXR()
        {
            var standaloneDefines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
        
            if (!standaloneDefines.Contains(VarwinOpenXRGlobalDefine))
            {
                standaloneDefines = $"{standaloneDefines}; {VarwinOpenXRGlobalDefine}";
            }
            
            if (!standaloneDefines.Contains(VarwinClientCoreGlobalDefine))
            {
                standaloneDefines = $"{standaloneDefines}; {VarwinClientCoreGlobalDefine}";
            }            

            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, standaloneDefines);
            
            var androidDefines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android);
        
            if (!androidDefines.Contains(VarwinOpenXRGlobalDefine))
            {
                androidDefines = $"{androidDefines}; {VarwinOpenXRGlobalDefine}";
            }
            
            if (!androidDefines.Contains(VarwinClientCoreGlobalDefine))
            {
                androidDefines = $"{androidDefines}; {VarwinClientCoreGlobalDefine}";
            }            

            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android, androidDefines);

            var sourcePath = Application.dataPath.Replace("Assets", "");
            sourcePath = Path.Combine(sourcePath, "com.varwin.xr.openxr.tar.gz");

            const string openXrPackageName = "com.varwin.xr.openxr.tar.gz";

            if (!File.Exists(sourcePath))
            {
                sourcePath = Path.Combine(Application.dataPath.Replace("Assets",""), "Packages", openXrPackageName);
            }

            var targetPath = Path.Combine(Application.dataPath, "SdkSetuppers", "Packages", openXrPackageName);

            if (!File.Exists(sourcePath))
            {
                Debug.LogError($"Can't build source package. File {sourcePath} doesn't exists.");
                return;
            }

            File.Copy(sourcePath, targetPath);
            AssetDatabase.Refresh();
            
            var assets = new[]
            {
                "Assets"
            };

            if (IncludeProjectSettings)
            {
                assets = assets.Concat(GetProjectSettingsAssets()).ToArray();
            }

            DeleteDirectories(TempExternalDirectory, InternalPackageDirectory);
            Directory.Move(ExternalPackageDirectory, TempExternalDirectory);

            CreateDirectories(InternalPackageDirectory);
            AssetDatabase.ExportPackage(assets.ToArray(), $"{InternalPackageDirectory}/VarwinSDK.unitypackage", ExportPackageOptions.Recurse);

            PackExternalPackage(true);

            File.Delete(targetPath);

            AssetDatabase.Refresh();
            
            EditorApplication.Exit(0);
        }

        private static void PackExternalPackage(bool withOpenXR = false)
        {
            var outputPackagePath = withOpenXR? $"{GetOutputPath()}/VarwinSDKWithOpenXR.unitypackage" : $"{GetOutputPath()}/VarwinSDK.unitypackage";
            Directory.Move(TempExternalDirectory, ExternalPackageDirectory);
            AssetDatabase.Refresh();
            
            var externalPackageContent = new[]
            {
                "Assets/InternalPackage/VarwinSDK.unitypackage",
                "Assets/SdkSetuppers"
            };

            if (File.Exists(outputPackagePath))
            {
                File.Delete(outputPackagePath);                
            }

            AssetDatabase.ExportPackage(externalPackageContent.ToArray(), outputPackagePath, ExportPackageOptions.Recurse);
            DeleteDirectories(InternalPackageDirectory);
        }

        private static void CreateDirectories(params string[] directories)
        {
            foreach (var directory in directories)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
        }

        private static void DeleteDirectories(params string[] directories)
        {
            foreach (var directory in directories)
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        public static void BuildVarwinSdkAssetStore()
        {
            var assets = AssetDatabase.GetAllAssetPaths()
                .Where(x => !x.StartsWith("Assets/Varwin/Core/Resources/PlayerRig"))
                .Where(x => !x.StartsWith("Assets/Varwin/Scenes"))
                .Where(x => !x.StartsWith("Assets/Varwin/Standalone Input"))
                .Where(x => !x.StartsWith("Assets/SteamVR_Input"))
                .ToArray();

            if (IncludeProjectSettings)
            {
                assets = assets.Concat(GetProjectSettingsAssets()).ToArray();
            }

            AssetDatabase.ExportPackage(assets, $"{GetOutputPath()}/VarwinSDK Assetstore.unitypackage", ExportPackageOptions.Default);
            EditorApplication.Exit(0);
        }

        private static string GetOutputPath()
        {
            var output = $"{UnityProject.Path}/.output";
            if (!Directory.Exists(output))
            {
                Directory.CreateDirectory(output);
            }

            return output;
        }

        private static IEnumerable<string> GetProjectSettingsAssets()
        {
            return Directory.GetFiles("ProjectSettings", "*.asset");
        }

        public static void BuildAllPackages()
        {
            SdkSettings.Features.Changelog.Enabled = false;
            var window = VarwinBuilderWindow.GetWindow("Building all packages in project");
            window.Build(VarwinObjectUtils.GetAllPackagesInProject());
        }
    }
}