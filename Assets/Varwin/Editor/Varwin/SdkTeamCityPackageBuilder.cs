using System.IO;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public class SdkTeamCityPackageBuilder : EditorWindow
    {
        private VarwinBuilderWindow _varwinBuilderWindow;

        public static void Build()
        {
            var instance = GetWindow<SdkTeamCityPackageBuilder>();
            instance._varwinBuilderWindow = VarwinBuilderWindow.GetWindow();
            var packages = VarwinObjectUtils.GetAllPackagesInProject();
            
            SdkSettings.Features.Changelog.Enabled = false;
            instance._varwinBuilderWindow.Build(packages);
        }
        
        public static void BuildByName()
        {
            var savedPathFilePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "buildPackageName.txt");

            if (!File.Exists(savedPathFilePath))
            {
                Debug.LogError("Path is not exist!");
                EditorApplication.Exit(-1);
                return;
            }

            var packageName = File.ReadAllText(savedPathFilePath);

            if (string.IsNullOrEmpty(packageName))
            {
                Debug.LogError("Path is null or empty!");
                EditorApplication.Exit(-1);
                return;
            }

            var targetPackage = VarwinObjectUtils.GetPackageDescriptorByName(packageName);

            if (!targetPackage)
            {
                Debug.LogError($"Can't find package with name {packageName}!");
                EditorApplication.Exit(-1);
                return;
            }
            
            var instance = GetWindow<SdkTeamCityPackageBuilder>();
            instance._varwinBuilderWindow = VarwinBuilderWindow.GetWindow();
            
            SdkSettings.Features.Changelog.Enabled = false;
            instance._varwinBuilderWindow.Build(targetPackage);
        }

        private void Update()
        {
            if (EditorApplication.isCompiling)
            {
                return;
            }

            if (_varwinBuilderWindow.IsFinished)
            {
                EditorApplication.Exit(0);
            }
        }
    }
}