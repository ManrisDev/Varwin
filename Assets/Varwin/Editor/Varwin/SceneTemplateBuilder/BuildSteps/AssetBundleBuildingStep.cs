using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Varwin.Editor;

namespace Varwin.SceneTemplateBuilding
{
    public class AssetBundleBuildingStep : BaseSceneTemplateBuildStep
    {
        private readonly string _bundleDirectory;
        
        private bool _buildAndroidAssetBundles;
        private bool _buildLinuxAssetBundles;
        
        public AssetBundleBuildingStep(SceneTemplateBuilder builder) : base(builder)
        {
            _bundleDirectory = $"{UnityProject.Path}/AssetBundles";
        }
        
        public override void Update()
        {
            base.Update();
            
            _buildAndroidAssetBundles = SdkSettings.Features.Mobile && Builder.WorldDescriptor.MobileReady;
            _buildLinuxAssetBundles = SdkSettings.Features.Linux && BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);

            if (!Directory.Exists(_bundleDirectory))
            {
                try
                {
                    Directory.CreateDirectory(_bundleDirectory);
                }
                catch
                {
                    var message = string.Format(SdkTexts.CannotCreateDirectoryFormat, _bundleDirectory);
                    Debug.LogError(message);
                    
                    EditorUtility.DisplayDialog(SdkTexts.CannotCreateDirectoryTitle, message, "OK");
                    throw;
                }
            }
            
            BuildAssetBundles(new()
            {
                BuildTarget.StandaloneWindows64, 
                BuildTarget.Android,
                BuildTarget.StandaloneLinux64
            });
            
            var originAssetBundle = $"{_bundleDirectory}/{Builder.SceneName.ToLowerInvariant()}";
            var destinationAssetBundle = $"{Builder.DestinationFolder}/bundle";
            CopyAssetBundle(originAssetBundle, destinationAssetBundle);
            Builder.SceneTemplatePackingPaths.Add(destinationAssetBundle);
            Builder.SceneTemplatePackingPaths.Add(destinationAssetBundle + ".manifest");
            
            if (_buildAndroidAssetBundles)
            {   
                var originAndroidAssetBundle = $"{_bundleDirectory}/android_{Builder.SceneName.ToLowerInvariant()}";
                var destinationAndroidAssetBundle = $"{Builder.DestinationFolder}/android_bundle";
                CopyAssetBundle(originAndroidAssetBundle, destinationAndroidAssetBundle);
                Builder.SceneTemplatePackingPaths.Add(destinationAndroidAssetBundle);
                Builder.SceneTemplatePackingPaths.Add(destinationAndroidAssetBundle + ".manifest");
            }
            
            if (_buildLinuxAssetBundles)
            {   
                var originAndroidAssetBundle = $"{_bundleDirectory}/linux_{Builder.SceneName.ToLowerInvariant()}";
                var destinationAndroidAssetBundle = $"{Builder.DestinationFolder}/linux_bundle";
                CopyAssetBundle(originAndroidAssetBundle, destinationAndroidAssetBundle);
                Builder.SceneTemplatePackingPaths.Add(destinationAndroidAssetBundle);
                Builder.SceneTemplatePackingPaths.Add(destinationAndroidAssetBundle + ".manifest");
            }
        }
        
        private void BuildAssetBundles(List<BuildTarget> buildTargets)
        {
            var currentTarget = EditorUserBuildSettings.activeBuildTarget;
            if (buildTargets.Contains(currentTarget))
            {
                var buildHandler = GetBuildBundlesHandler(currentTarget);
                buildHandler?.Invoke();
                buildTargets.Remove(currentTarget);
            }

            foreach (var target in buildTargets)
            {
                var buildHandler = GetBuildBundlesHandler(target);
                buildHandler?.Invoke();
            }
        }
        
        private Action GetBuildBundlesHandler(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows64: 
                    return BuildWindowsBundles;
                case BuildTarget.Android: 
                    return BuildAndroidBundles;
                case BuildTarget.StandaloneLinux64:
                    return BuildLinuxBundles;
                default:
                    Debug.LogError(string.Format(SdkTexts.BuildTargetNoSupportedFormat, buildTarget));
                    return null;
            }
        }
        
        private void BuildWindowsBundles()
        {
            AssetBundleBuild assetBundleBuild = default;

            assetBundleBuild.assetNames = new[] {Builder.ScenePath};
            assetBundleBuild.assetBundleName = Builder.SceneName.ToLowerInvariant();

            BuildPipeline.BuildAssetBundles(
                _bundleDirectory,
                new[] {assetBundleBuild, ResourcesExclude.GetExcludedResourcesBundle()},
                BuildAssetBundleOptions.UncompressedAssetBundle,
                BuildTarget.StandaloneWindows64);
        }
        
        private void BuildLinuxBundles()
        {
            if (!_buildLinuxAssetBundles)
            {
                return;
            }

            AssetBundleBuild assetBundleBuild = default;

            assetBundleBuild.assetNames = new[] {Builder.ScenePath};
            assetBundleBuild.assetBundleName = $"linux_{Builder.SceneName.ToLowerInvariant()}";
            
            BuildPipeline.BuildAssetBundles(
                _bundleDirectory,
                new[] {assetBundleBuild, ResourcesExclude.GetExcludedResourcesBundle()},
                BuildAssetBundleOptions.UncompressedAssetBundle,
                BuildTarget.StandaloneLinux64);
        }
        
        private void BuildAndroidBundles()
        {
            if (!_buildAndroidAssetBundles)
            {
                return;
            }

            AssetBundleBuild assetBundleBuild = default;

            assetBundleBuild.assetNames = new[] {Builder.ScenePath};
            assetBundleBuild.assetBundleName = $"android_{Builder.SceneName.ToLowerInvariant()}";

            AndroidTextureOverrider.OverrideTextures(Builder.ScenePath);
                
            BuildPipeline.BuildAssetBundles(
                _bundleDirectory,
                new[] {assetBundleBuild, ResourcesExclude.GetExcludedResourcesBundle()},
                BuildAssetBundleOptions.UncompressedAssetBundle,
                BuildTarget.Android);
        }

        private void CopyAssetBundle(string origin, string destination)
        {
            File.Copy(origin, destination, true);
            File.Copy($"{origin}.manifest", $"{destination}.manifest", true);
        }

    }
}