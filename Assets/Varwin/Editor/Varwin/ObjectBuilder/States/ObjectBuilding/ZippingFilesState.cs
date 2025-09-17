using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Varwin.Data;

namespace Varwin.Editor
{
    public class ZippingFilesState : BaseObjectBuildingState
    {
        public ZippingFilesState(VarwinBuilder builder) : base(builder)
        {
            Label = $"Baking objects";
        }

        protected override void OnEnter()
        {
            if (Directory.Exists(VarwinBuildingPath.BakedObjects))
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(VarwinBuildingPath.BakedObjects);
            }
            catch
            {
                string message = string.Format(SdkTexts.CannotCreateDirectoryFormat, VarwinBuildingPath.BakedObjects);
                Debug.LogError(message);
                EditorUtility.DisplayDialog(SdkTexts.CannotCreateDirectoryTitle, message, "OK");
            
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }
        }

        protected override void OnExit()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        }

        protected override void Update(ObjectBuildDescription currentObjectBuildDescription)
        {
            Label = $"Baking {currentObjectBuildDescription.ObjectName}";
            
            var filesToZip = new List<string>();

            try
            {
                AddFiles(currentObjectBuildDescription, filesToZip);

                ZipFiles(filesToZip, $"{VarwinBuildingPath.BakedObjects}/{currentObjectBuildDescription.ObjectName}.vwo");
            }
            catch (IOException e)
            {
                if (EditorUtility.DisplayDialog("Error!", $"{e.Message}\n", "Retry", "Continue"))
                {
                    ZipFiles(filesToZip, $"{VarwinBuildingPath.BakedObjects}/{currentObjectBuildDescription.ObjectName}.vwo");
                }
                else
                {
                    currentObjectBuildDescription.HasError = true;
                    var message = $"{e.Message}:\nProblem when creating files";
                    Debug.LogError($"{message}\n{e}", currentObjectBuildDescription.ContainedObjectDescriptor);
                }
            }
            catch (Exception e)
            {
                currentObjectBuildDescription.HasError = true;
                var message = $"{e.Message}:\nProblem when creating files";
                if (ObjectBuildDescriptions.Count == 1)
                {
                    EditorUtility.DisplayDialog("Error!", message, "OK");
                }
                Debug.LogError($"{message}\n{e}", currentObjectBuildDescription.ContainedObjectDescriptor);
            }
            finally
            {
                foreach (string file in filesToZip)
                {
                    TryDeleteFile(file);
                }

                if (currentObjectBuildDescription.ContainedObjectDescriptor)
                {
                    CreateObjectUtils.RemoveBackupObjectIfExists(currentObjectBuildDescription.ContainedObjectDescriptor.Prefab);
                }
            }
        }

        protected virtual void AddFiles(ObjectBuildDescription currentObjectBuildDescription, List<string> filesToZip)
        {
            filesToZip.Add(WriteInstallJson(currentObjectBuildDescription));
            filesToZip.Add(WriteBundleJson(currentObjectBuildDescription));

            filesToZip.AddRange(CollectStandaloneBundle(currentObjectBuildDescription));
            filesToZip.AddRange(CollectWebglBundle(currentObjectBuildDescription));
            filesToZip.AddRange(CollectAndroidBundle(currentObjectBuildDescription));

            filesToZip.AddRange(CollectSourcePackage(currentObjectBuildDescription));
            filesToZip.AddRange(CollectIcons(currentObjectBuildDescription));
            filesToZip.AddRange(CollectAssemblies(currentObjectBuildDescription));
            filesToZip.AddRange(CollectLinuxBundle(currentObjectBuildDescription));
        }

        protected string WriteInstallJson(ObjectBuildDescription objectBuildDescription)
        {
            string targetInstallJsonFile = $"{VarwinBuildingPath.BakedObjects}/install.json";
            File.WriteAllText(targetInstallJsonFile, objectBuildDescription.ConfigBlockly);
            return targetInstallJsonFile;
        }

        private string WriteBundleJson(ObjectBuildDescription objectBuildDescription)
        {
            string targetBundleJsonFile = $"{VarwinBuildingPath.BakedObjects}/bundle.json";
            File.WriteAllText(targetBundleJsonFile, objectBuildDescription.ConfigAssetBundle);
            return targetBundleJsonFile;
        }

        private List<string> CollectStandaloneBundle(ObjectBuildDescription objectBuildDescription)
        {
            var filesToZip = new List<string>();
            
            string sourceBundlePath = $"{VarwinBuildingPath.AssetBundles}/{objectBuildDescription.BundleName}";
            string targetBundlePath = $"{VarwinBuildingPath.BakedObjects}/bundle";
            File.Copy(sourceBundlePath, targetBundlePath, true);
            filesToZip.Add(targetBundlePath);
                
            string sourceBundleManifestPath = $"{VarwinBuildingPath.AssetBundles}/{objectBuildDescription.BundleName}.manifest";
            string targetBundleManifestPath = $"{VarwinBuildingPath.BakedObjects}/bundle.manifest";
            File.Copy(sourceBundleManifestPath, targetBundleManifestPath, true);
            filesToZip.Add(targetBundleManifestPath);

            var assetBundleParts = objectBuildDescription.AssetBundleParts;
            
            if (assetBundleParts != null && assetBundleParts.Count != 0 && assetBundleParts.ContainsKey(""))
            {
                var assetInfo = JsonConvert.DeserializeObject<AssetInfo>(objectBuildDescription.ConfigAssetBundle);
                for (var index = 0; index < assetBundleParts[""].Count; index++)
                {
                    var assetBundlePartName = assetBundleParts[""][index];
                    var assetBundlePartFileName = assetInfo.AssetBundleParts[index];

                    var sourceResourcesPath = $"{VarwinBuildingPath.AssetBundles}/{assetBundlePartName}";
                    var targetResourcesPath = $"{VarwinBuildingPath.BakedObjects}/{assetBundlePartFileName}";

                    File.Copy(sourceResourcesPath, targetResourcesPath, true);
                    filesToZip.Add(targetResourcesPath);

                    var sourceBundlePartPath = $"{VarwinBuildingPath.AssetBundles}/{assetBundlePartName}.manifest";
                    var targetBundlePartPath = $"{VarwinBuildingPath.BakedObjects}/{assetBundlePartFileName}.manifest";
                    File.Copy(sourceBundlePartPath, targetBundlePartPath, true);
                    filesToZip.Add(targetBundlePartPath);
                }
            }

            return filesToZip;
        }
        
        private List<string> CollectAndroidBundle(ObjectBuildDescription objectBuildDescription)
        {
            var filesToZip = new List<string>();
            
            if (!SdkSettings.Features.Mobile.Enabled || !objectBuildDescription.ContainedObjectDescriptor.MobileReady)
            {
                return filesToZip;
            }

            string sourceAndroidBundlePath = $"{VarwinBuildingPath.AssetBundles}/android_{objectBuildDescription.BundleName}";
            string targetAndroidBundleFile = $"{VarwinBuildingPath.BakedObjects}/android_bundle";
            File.Copy(sourceAndroidBundlePath, targetAndroidBundleFile, true);
            filesToZip.Add(targetAndroidBundleFile);
                
            string sourceAndroidBundleManifestPath = $"{VarwinBuildingPath.AssetBundles}/android_{objectBuildDescription.BundleName}.manifest";
            string targetAndroidBundleManifestFile = $"{VarwinBuildingPath.BakedObjects}/android_bundle.manifest";
            File.Copy(sourceAndroidBundleManifestPath, targetAndroidBundleManifestFile, true);
            filesToZip.Add(targetAndroidBundleManifestFile);

            var assetBundleParts = objectBuildDescription.AssetBundleParts;
            
            if (assetBundleParts != null && assetBundleParts.Count != 0 && assetBundleParts.ContainsKey("android"))
            {
                var assetInfo = JsonConvert.DeserializeObject<AssetInfo>(objectBuildDescription.ConfigAssetBundle);
                for (var index = 0; index < assetBundleParts["android"].Count; index++)
                {
                    var assetBundlePartName = assetBundleParts["android"][index];
                    var assetBundlePartFileName = assetInfo.AssetBundleParts[index];

                    var sourceResourcesPath = $"{VarwinBuildingPath.AssetBundles}/{assetBundlePartName}";
                    var targetResourcesPath = $"{VarwinBuildingPath.BakedObjects}/android_{assetBundlePartFileName}";

                    File.Copy(sourceResourcesPath, targetResourcesPath, true);
                    filesToZip.Add(targetResourcesPath);

                    var sourceBundlePartPath = $"{VarwinBuildingPath.AssetBundles}/{assetBundlePartName}.manifest";
                    var targetBundlePartPath = $"{VarwinBuildingPath.BakedObjects}/android_{assetBundlePartFileName}.manifest";
                    File.Copy(sourceBundlePartPath, targetBundlePartPath, true);
                    filesToZip.Add(targetBundlePartPath);
                }
            }
            
            return filesToZip;
        }
        
        private List<string> CollectLinuxBundle(ObjectBuildDescription objectBuildDescription)
        {
            var filesToZip = new List<string>();
            
            if (!SdkSettings.Features.Linux.Enabled)
            {
                return filesToZip;
            }

            string sourceAndroidBundlePath = $"{VarwinBuildingPath.AssetBundles}/linux_{objectBuildDescription.BundleName}";
            string targetAndroidBundleFile = $"{VarwinBuildingPath.BakedObjects}/linux_bundle";
            File.Copy(sourceAndroidBundlePath, targetAndroidBundleFile, true);
            filesToZip.Add(targetAndroidBundleFile);
                
            string sourceAndroidBundleManifestPath = $"{VarwinBuildingPath.AssetBundles}/linux_{objectBuildDescription.BundleName}.manifest";
            string targetAndroidBundleManifestFile = $"{VarwinBuildingPath.BakedObjects}/linux_bundle.manifest";
            File.Copy(sourceAndroidBundleManifestPath, targetAndroidBundleManifestFile, true);
            filesToZip.Add(targetAndroidBundleManifestFile);

            var assetBundleParts = objectBuildDescription.AssetBundleParts;
            
            if (assetBundleParts != null && assetBundleParts.Count != 0 && assetBundleParts.ContainsKey("linux"))
            {
                var assetInfo = JsonConvert.DeserializeObject<AssetInfo>(objectBuildDescription.ConfigAssetBundle);
                for (var index = 0; index < assetBundleParts["linux"].Count; index++)
                {
                    var assetBundlePartName = assetBundleParts["linux"][index];
                    var assetBundlePartFileName = assetInfo.AssetBundleParts[index];

                    var sourceResourcesPath = $"{VarwinBuildingPath.AssetBundles}/{assetBundlePartName}";
                    var targetResourcesPath = $"{VarwinBuildingPath.BakedObjects}/linux_{assetBundlePartFileName}";

                    File.Copy(sourceResourcesPath, targetResourcesPath, true);
                    filesToZip.Add(targetResourcesPath);

                    var sourceBundlePartPath = $"{VarwinBuildingPath.AssetBundles}/{assetBundlePartName}.manifest";
                    var targetBundlePartPath = $"{VarwinBuildingPath.BakedObjects}/linux_{assetBundlePartFileName}.manifest";
                    File.Copy(sourceBundlePartPath, targetBundlePartPath, true);
                    filesToZip.Add(targetBundlePartPath);
                }
            }
            
            return filesToZip;
        }

        private List<string> CollectWebglBundle(ObjectBuildDescription objectBuildDescription)
        {
            var filesToZip = new List<string>();
            
            if (!SdkSettings.Features.WebGL.Enabled)
            {
                return filesToZip;
            }

            string sourceWebglBundlePath = $"{VarwinBuildingPath.AssetBundles}/webgl_{objectBuildDescription.BundleName}";
            string targetWebglBundleFile = $"{VarwinBuildingPath.BakedObjects}/webgl_bundle";
            File.Copy(sourceWebglBundlePath, targetWebglBundleFile, true);
            filesToZip.Add(targetWebglBundleFile);
                
            string sourceWebglBundleManifestPath = $"{VarwinBuildingPath.AssetBundles}/webgl_{objectBuildDescription.BundleName}.manifest";
            string targetWebglBundleManifestFile = $"{VarwinBuildingPath.BakedObjects}/webgl_bundle.manifest";
            File.Copy(sourceWebglBundleManifestPath, targetWebglBundleManifestFile, true);
            filesToZip.Add(targetWebglBundleManifestFile);

            var assetBundleParts = objectBuildDescription.AssetBundleParts;
            
            if (assetBundleParts != null && assetBundleParts.Count != 0 && assetBundleParts.ContainsKey("webgl"))
            {
                var assetInfo = JsonConvert.DeserializeObject<AssetInfo>(objectBuildDescription.ConfigAssetBundle);
                for (var index = 0; index < assetBundleParts["webgl"].Count; index++)
                {
                    var assetBundlePartName = assetBundleParts["webgl"][index];
                    var assetBundlePartFileName = assetInfo.AssetBundleParts[index];

                    var sourceResourcesPath = $"{VarwinBuildingPath.AssetBundles}/{assetBundlePartName}";
                    var targetResourcesPath = $"{VarwinBuildingPath.BakedObjects}/webgl_{assetBundlePartFileName}";

                    File.Copy(sourceResourcesPath, targetResourcesPath, true);
                    filesToZip.Add(targetResourcesPath);

                    var sourceBundlePartPath = $"{VarwinBuildingPath.AssetBundles}/{assetBundlePartName}.manifest";
                    var targetBundlePartPath = $"{VarwinBuildingPath.BakedObjects}/webgl_{assetBundlePartFileName}.manifest";
                    File.Copy(sourceBundlePartPath, targetBundlePartPath, true);
                    filesToZip.Add(targetBundlePartPath);
                }
            }
            return filesToZip;
        }

        private List<string> CollectSourcePackage(ObjectBuildDescription objectBuildDescription)
        {
            var filesToZip = new List<string>();
            
            if (!objectBuildDescription.ContainedObjectDescriptor.SourcesIncluded)
            {
                return filesToZip;
            }
            
            string sourcePath = $"{VarwinBuildingPath.SourcePackages}/{objectBuildDescription.ContainedObjectDescriptor.Name}.unitypackage";
            string targetSourceFile = $"{VarwinBuildingPath.SourcePackages}/sources.unitypackage";
            File.Copy(sourcePath, targetSourceFile, true);
            filesToZip.Add(targetSourceFile);
            
            return filesToZip;
        }
        
        private List<string> CollectIcons(ObjectBuildDescription objectBuildDescription)
        {
            var filesToZip = new List<string>();
            
            string sourceSpritesheetPath = $"{VarwinBuildingPath.ObjectPreviews}/spritesheet_{objectBuildDescription.ObjectGuid}.jpg";
            string targetSpritesheetPath = $"{VarwinBuildingPath.BakedObjects}/spritesheet.jpg";
            if (File.Exists(sourceSpritesheetPath))
            {
                File.Copy(sourceSpritesheetPath, targetSpritesheetPath, true);
            }
            filesToZip.Add(targetSpritesheetPath);
            TryDeleteFile(sourceSpritesheetPath);

            string sourceViewPath = $"{VarwinBuildingPath.ObjectPreviews}/view_{objectBuildDescription.ObjectGuid}.jpg";
            string targetViewPath = $"{VarwinBuildingPath.BakedObjects}/view.jpg";
            if (File.Exists(sourceViewPath))
            {
                File.Copy(sourceViewPath, targetViewPath, true);
            }
            filesToZip.Add(targetViewPath);
            TryDeleteFile(sourceViewPath);
            
            string sourceThumbnailPath = $"{VarwinBuildingPath.ObjectPreviews}/thumbnail_{objectBuildDescription.ObjectGuid}.jpg";
            string targetThumbnailPath = $"{VarwinBuildingPath.BakedObjects}/thumbnail.jpg";
            if (File.Exists(sourceThumbnailPath))
            {
                File.Copy(sourceThumbnailPath, targetThumbnailPath, true);
            }
            filesToZip.Add(targetThumbnailPath);
            TryDeleteFile(sourceThumbnailPath);

            string sourceBundleIconPath = objectBuildDescription.IconPath;
            string targetBundleIconPath = $"{VarwinBuildingPath.BakedObjects}/bundle.png";
            if (File.Exists(sourceBundleIconPath))
            {
                File.Copy(sourceBundleIconPath, targetBundleIconPath);
            }
            else if (objectBuildDescription.ContainedObjectDescriptor.Icon)
            {
                sourceBundleIconPath = AssetDatabase.GetAssetPath(objectBuildDescription.ContainedObjectDescriptor.Icon);
                if (File.Exists(sourceBundleIconPath))
                {
                    File.Copy(sourceBundleIconPath, targetBundleIconPath);
                }
            }
            filesToZip.Add(targetBundleIconPath);
            
            return filesToZip;
        }

        protected void TryDeleteFile(string path)
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
        
        protected List<string> CollectAssemblies(ObjectBuildDescription currentObjectBuildDescription)
        {
            var objectAssemblies = new List<string>();

            foreach (string dllPath in currentObjectBuildDescription.Assemblies)
            {
                string newDllPath = $"{VarwinBuildingPath.BakedObjects}/{Path.GetFileName(dllPath)}";
                File.Copy(dllPath, newDllPath, true);
                objectAssemblies.Add(newDllPath);
            }

            return objectAssemblies;
        }
        
        protected virtual void ZipFiles(List<string> files, string zipFilePath)
        {
            using (ZipFile loanZip = new ZipFile())
            {
                loanZip.AddFiles(files, false, "");
                loanZip.Save(zipFilePath);
            }

            foreach (string file in files)
            {
                TryDeleteFile(file);
            }
        }
    }
}