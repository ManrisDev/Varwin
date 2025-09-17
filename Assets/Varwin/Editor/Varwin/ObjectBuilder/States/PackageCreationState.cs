using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Varwin.Core;
using Varwin.Public;

namespace Varwin.Editor
{
    public class PackageCreationState : BaseCommonBuildingState
    {
        private const string DefaultIconPath = @"Assets/Varwin/Editor/Varwin/Resources/DefaultObjectIcon.png";
        private Dictionary<string, string> _objectFilesToZip;
        private Dictionary<string, string> _sceneFilesToZip;
        private Dictionary<string, string> _resourceFilesToZip;
        
        public PackageCreationState(VarwinBuilder builder) : base(builder)
        {
            Label = "Packages Creation";
        }

        protected override void OnEnter()
        {
            _objectFilesToZip = new Dictionary<string, string>();
            _sceneFilesToZip = new Dictionary<string, string>();
            _resourceFilesToZip = new Dictionary<string, string>();
        }
        
        protected override void Update(ObjectBuildDescription currentObjectBuildDescription)
        {
            Label = "Packages Creation";
            
            try
            {
                _objectFilesToZip.Add(currentObjectBuildDescription.ObjectName, $"{VarwinBuildingPath.BakedObjects}/{currentObjectBuildDescription.ObjectName}.vwo");
            }
            catch (Exception e)
            {
                var message = $"{currentObjectBuildDescription.ObjectName} error: Problem when package creation";
                Debug.LogError($"{message}\n{e}", currentObjectBuildDescription.ContainedObjectDescriptor);
                currentObjectBuildDescription.HasError = true;
                if (ObjectBuildDescriptions.Count == 1)
                {
                    EditorUtility.DisplayDialog("Error!", message, "OK");
                }
                    
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }
        }
        
        protected override void Update(SceneTemplateBuildDescription currentSceneTemplateBuildDescription)
        {
            Label = "Packages Creation";
            
            try
            {
                _sceneFilesToZip.Add(currentSceneTemplateBuildDescription.Name, $"{VarwinBuildingPath.BakedSceneTemplates}/{currentSceneTemplateBuildDescription.Name}.vwst");
            }
            catch (Exception e)
            {
                var message = $"{currentSceneTemplateBuildDescription.Name} error: Problem when package creation";
                Debug.LogError($"{message}\n{e}");
                currentSceneTemplateBuildDescription.HasError = true;
                if (SceneTemplateBuildDescriptions.Count == 1)
                {
                    EditorUtility.DisplayDialog("Error!", message, "OK");
                }
                    
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }
        }

        protected override void Update(ResourceBuildDescription resourceBuildDescription)
        {
            Label = "Packages Creation";
            
            try
            {
                _resourceFilesToZip.Add(resourceBuildDescription.Name, resourceBuildDescription.ResourcePath);
            }
            catch (Exception e)
            {
                var message = $"{resourceBuildDescription.Name} error: Problem when package creation";
                Debug.LogError($"{message}\n{e}");
                resourceBuildDescription.HasError = true;
                if (ResourcesToBuild.Count == 1)
                {
                    EditorUtility.DisplayDialog("Error!", message, "OK");
                }
                    
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }
        }
        
        protected override void OnExit()
        {
            foreach (var packageInfo in Builder.PackageInfos)
            {
                var outputFilename = packageInfo.Name;
                var outputExtension = "vwpkg";
                var outputPath = $"{VarwinBuildingPath.BakedObjects}/{outputFilename}.{outputExtension}";

                var filesToZip = new HashSet<string>();
                
                filesToZip.Add(WriteInstallJson(packageInfo.Config));

                foreach (var locale in packageInfo.ViewPaths)
                {
                    var fileName = locale.key == Language.English ? "view" : $"view_{locale.key.GetCode()}";
                    filesToZip.Add(GenerateImages(locale.value ?? DefaultIconPath, fileName));
                }

                foreach (var locale in packageInfo.ThumbnailPaths)
                {
                    var fileName = locale.key == Language.English ? "thumbnail" : $"thumbnail_{locale.key.GetCode()}";
                    filesToZip.Add(GenerateImages(locale.value ?? DefaultIconPath, fileName));
                }
                
                filesToZip.Add(GenerateGuidMigration(packageInfo));

                foreach (var fileToZip in _objectFilesToZip)
                {
                    if (packageInfo.VarwinObjects?.Contains(fileToZip.Key) ?? false)
                    {
                        filesToZip.Add(fileToZip.Value);
                    }
                }

                foreach (var fileToZip in _sceneFilesToZip)
                {
                    if (packageInfo.VarwinScenes?.Contains(fileToZip.Key) ?? false)
                    {
                        filesToZip.Add(fileToZip.Value);
                    }
                }
                
                foreach (var fileToZip in _resourceFilesToZip)
                {
                    if (packageInfo.VarwinResources?.Contains(fileToZip.Value) ?? false)
                    {
                        filesToZip.Add(fileToZip.Value);
                    }
                }
            
                ZipFiles(filesToZip, outputPath);

                TryDeleteFile($"{VarwinBuildingPath.BakedObjects}/install.json");
                TryDeleteFile($"{VarwinBuildingPath.BakedObjects}/guidMigration.json");
                
                foreach (var locale in packageInfo.ViewPaths)
                {
                    var fileName = locale.key == Language.English ? "view.jpg" : $"view_{locale.key.GetCode()}.jpg";
                    TryDeleteFile($"{VarwinBuildingPath.BakedObjects}/{fileName}");
                }

                foreach (var locale in packageInfo.ThumbnailPaths)
                {
                    var fileName = locale.key == Language.English ? "thumbnail.jpg" : $"thumbnail_{locale.key.GetCode()}.jpg";
                    TryDeleteFile($"{VarwinBuildingPath.BakedObjects}/{fileName}");
                }
            }
            
            foreach (var file in _objectFilesToZip)
            {
                TryDeleteFile(file.Value);
            }

            foreach (var file in _sceneFilesToZip)
            {
                TryDeleteFile(file.Value);
            }
            
            _sceneFilesToZip.Clear();
            _sceneFilesToZip = null;
            _objectFilesToZip.Clear();
            _objectFilesToZip = null;
            _resourceFilesToZip.Clear();
            _resourceFilesToZip = null;
        }

        private string WriteInstallJson(string installJson)
        {
            var targetInstallJsonFile = $"{VarwinBuildingPath.BakedObjects}/install.json";
            File.WriteAllText(targetInstallJsonFile, installJson);
            return targetInstallJsonFile;
        }
        
        private string GenerateImages(string iconPath, string iconName)
        {
            var targetPath = $"{VarwinBuildingPath.BakedObjects}/{iconName}.jpg";
            return GenerateImageFromTexture(iconPath, targetPath, true);
        }

        private string GenerateGuidMigration(VarwinPackageInfo packageInfo)
        {
            var targetPath = $"{VarwinBuildingPath.BakedObjects}/guidMigration.json";

            var guidMigration = new GuidMigration
            {
                Infos = new List<GuidMigrationInfo>()
            };

            if (packageInfo.VarwinObjects != null)
            {
                foreach (var objectBuildDescription in ObjectBuildDescriptions)
                {
                    if (!packageInfo.VarwinObjects.Contains(objectBuildDescription.ObjectName))
                    {
                        continue;
                    }

                    guidMigration.Infos.Add(new GuidMigrationInfo
                    {
                        Name = objectBuildDescription.ObjectName,
                        RootGuid = objectBuildDescription.RootGuid,
                        NewGuid = objectBuildDescription.NewGuid
                    });
                }
            }

            if (packageInfo.VarwinScenes != null)
            {
                foreach (var sceneTemplateBuildDescription in SceneTemplateBuildDescriptions)
                {
                    if (!packageInfo.VarwinScenes.Contains(sceneTemplateBuildDescription.Name))
                    {
                        continue;
                    }

                    guidMigration.Infos.Add(new GuidMigrationInfo
                    {
                        Name = sceneTemplateBuildDescription.Name,
                        RootGuid = sceneTemplateBuildDescription.RootGuid,
                        NewGuid = sceneTemplateBuildDescription.NewGuid
                    });
                }
            }

            var jsonSerializerSettings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};
            string json = JsonConvert.SerializeObject(guidMigration, Formatting.None, jsonSerializerSettings);
            
            File.WriteAllText(targetPath, json);
            return targetPath;
        }

        private string GenerateImageFromTexture(string sourcePath, string targetPath, bool isJpg)
        {
            var image = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
            var generatedImage = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
            if (generatedImage.LoadImage(File.ReadAllBytes(sourcePath)))
            {
                File.WriteAllBytes(targetPath, isJpg ? generatedImage.EncodeToJPG() : generatedImage.EncodeToPNG());
            }
            else
            {
                File.Copy(sourcePath, targetPath);
            }

            return targetPath;
        }
        
        private void ZipFiles(IEnumerable<string> files, string zipFilePath)
        {
            using var loanZip = new ZipFile();
            loanZip.AddFiles(files, false, "");
            loanZip.Save(zipFilePath);
        }

        private void TryDeleteFile(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                File.Delete(path);
            }
            catch
            {
                Debug.LogError(string.Format(SdkTexts.CannotDeleteFileFormat, path));
            }
        }

        private struct GuidMigration
        {
            public List<GuidMigrationInfo> Infos;
        }

        private struct GuidMigrationInfo
        {
            public string Name;
            public string RootGuid;
            public string NewGuid;
        }
    }
}