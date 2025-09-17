using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Varwin.Public;

namespace Varwin.Editor
{
    public class AssetBundleBuildingState : BaseObjectBuildingState
    {
        private int _bundlePartIdx = 0;

        private Dictionary<AssetBundlePart, AssetBundleBuild> _buildedBundleParts = new Dictionary<AssetBundlePart, AssetBundleBuild>();

        public AssetBundleBuildingState(VarwinBuilder builder) : base(builder)
        {
            Label = SdkTexts.BuildingAssetBundlesStep;
        }

        protected override void OnEnter()
        {
            if (Directory.Exists(VarwinBuildingPath.AssetBundles))
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(VarwinBuildingPath.AssetBundles);
            }
            catch
            {
                string message = string.Format(SdkTexts.CannotCreateDirectoryFormat, VarwinBuildingPath.AssetBundles);
                Debug.LogError(message);
                EditorUtility.DisplayDialog(SdkTexts.CannotCreateDirectoryTitle,
                    message,
                    "OK");

                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }

            _buildedBundleParts.Clear();
        }

        protected override void Update(ObjectBuildDescription currentObjectBuildDescription)
        {
            try
            {
                Builder.Serialize();

                var buildTargets = new List<BuildTarget>()
                {
                    BuildTarget.StandaloneWindows64,
                    BuildTarget.Android,
                    BuildTarget.WebGL,
                    BuildTarget.StandaloneLinux64
                };
                BuildAssetBundles(buildTargets, VarwinBuildingPath.AssetBundles);

                IsFinished = true;
            }
            catch (Exception e)
            {
                currentObjectBuildDescription.HasError = true;
                string message = string.Format(SdkTexts.ProblemWhenBuildingAssetBundlesFormat, e.Message);
                if (ObjectBuildDescriptions.Count == 1)
                {
                    EditorUtility.DisplayDialog("Error!", message, "OK");
                }

                Debug.LogError($"{message}\n{e}", currentObjectBuildDescription.ContainedObjectDescriptor);
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }
        }

        private void BuildAssetBundles(List<BuildTarget> buildTargets, string folder)
        {
            BuildTarget currentTarget = EditorUserBuildSettings.activeBuildTarget;
            if (buildTargets.Contains(currentTarget))
            {
                Action<string> buildHandler = GetBuildBundlesHandler(currentTarget);
                buildHandler?.Invoke(folder);
                buildTargets.Remove(currentTarget);
            }

            foreach (var target in buildTargets)
            {
                Action<string> buildHandler = GetBuildBundlesHandler(target);
                buildHandler?.Invoke(folder);
            }
        }

        private Action<string> GetBuildBundlesHandler(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows64:
                    return BuildWindowsBundles;
                case BuildTarget.Android:
                    return BuildAndroidBundles;
                case BuildTarget.WebGL:
                    return BuildWebGLBundles;
                case BuildTarget.StandaloneLinux64:
                    return BuildLinuxBundles;
                default:
                    Debug.LogErrorFormat(SdkTexts.BuildTargetNotSupportFormat, buildTarget);
                    return null;
            }
        }

        private AssetBundleBuild BuildAssetBundlePart(AssetBundlePart assetBundlePart, string prefix = null)
        {
            AssetBundleBuild assetBundleBuild = default;

            var assetsIgnoredDuplicates = new HashSet<string>();
            foreach (var asset in assetBundlePart.Assets)
            {
                var assetPath = AssetDatabase.GetAssetPath(asset);

                if (!asset || string.IsNullOrEmpty(assetPath))
                {
                    var path = AssetDatabase.GetAssetPath(assetBundlePart);
                    if (!string.IsNullOrEmpty(path))
                    {
                        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
                    }

                    throw new NullReferenceException($"Some asset bundle part is missing ! AssetBundlePart: {path}");
                }

                assetsIgnoredDuplicates.Add(AssetDatabase.GetAssetPath(asset));
            }

            assetBundleBuild.assetNames = assetsIgnoredDuplicates.ToArray();
            assetBundleBuild.assetBundleName = $"{(!string.IsNullOrEmpty(prefix)? $"{prefix}_" : "")}resources_{_bundlePartIdx++}_{Guid.NewGuid()}";

            return assetBundleBuild;
        }

        private void BuildAndSetAssetBundlePart(ObjectBuildDescription obj, List<AssetBundleBuild> bundles, string prefix = null)
        {
            var assetBundleParts = obj.ContainedObjectDescriptor.AssetBundleParts;

            if (assetBundleParts == null)
            {
                return;
            }
            
            foreach (var assetBundlePart in assetBundleParts)
            {
                if (!assetBundlePart || assetBundlePart.Assets == null)
                {
                    continue;
                }

                if (!_buildedBundleParts.ContainsKey(assetBundlePart))
                {
                    _buildedBundleParts.Add(assetBundlePart, BuildAssetBundlePart(assetBundlePart, prefix));
                    bundles.Add(_buildedBundleParts[assetBundlePart]);
                }

                if (_buildedBundleParts.ContainsKey(assetBundlePart))
                {
                    if (obj.AssetBundleParts == null)
                    {
                        obj.AssetBundleParts = new Dictionary<string, List<string>>();
                    }

                    if (!obj.AssetBundleParts.ContainsKey(prefix ?? ""))
                    {
                        obj.AssetBundleParts.Add(prefix ?? "", new List<string>());
                    }

                    obj.AssetBundleParts[prefix ?? ""].Add(_buildedBundleParts[assetBundlePart].assetBundleName);
                }
            }
        }
        
        private void BuildWindowsBundles(string folder)
        {
            _bundlePartIdx = 0;
            _buildedBundleParts.Clear();

            var bundles = new List<AssetBundleBuild>();

            foreach (ObjectBuildDescription obj in ObjectBuildDescriptions)
            {
                if (obj.HasError)
                {
                    continue;
                }

                BuildAndSetAssetBundlePart(obj, bundles);
                bundles.Add(GetAssetBundleBuild(obj));
            }

            ResourcesExclude.AppendExcludedResources(bundles);

            BuildPipeline.BuildAssetBundles(
                folder,
                bundles.ToArray(),
                BuildAssetBundleOptions.None,
                BuildTarget.StandaloneWindows64);
        }
        
        private void BuildLinuxBundles(string folder)
        {
            _bundlePartIdx = 0;
            _buildedBundleParts.Clear();

            if (!SdkSettings.Features.Linux || !BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64))
            {
                return;
            }

            var bundles = new List<AssetBundleBuild>();

            foreach (ObjectBuildDescription obj in ObjectBuildDescriptions)
            {
                if (obj.HasError)
                {
                    continue;
                }

                BuildAndSetAssetBundlePart(obj, bundles,"linux");

                bundles.Add(GetAssetBundleBuild(obj, "linux"));
            }

            ResourcesExclude.AppendExcludedResources(bundles);

            BuildPipeline.BuildAssetBundles(
                folder,
                bundles.ToArray(),
                BuildAssetBundleOptions.None,
                BuildTarget.StandaloneLinux64);
        }
        
        private void BuildAndroidBundles(string folder)
        {
            _bundlePartIdx = 0;
            _buildedBundleParts.Clear();
            
            if (!SdkSettings.Features.Mobile.Enabled)
            {
                return;
            }

            MobileTextureSubtarget defaultAndroidBuildSubTarget = EditorUserBuildSettings.androidBuildSubtarget;
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;

            var bundles = new List<AssetBundleBuild>();

            foreach (ObjectBuildDescription obj in ObjectBuildDescriptions)
            {
                if (obj.HasError)
                {
                    continue;
                }

                if (!obj.ContainedObjectDescriptor.MobileReady)
                {
                    continue;
                }

                BuildAndSetAssetBundlePart(obj, bundles, $"android");

                bundles.Add(GetAssetBundleBuild(obj, "android"));
                AndroidTextureOverrider.OverrideTextures(obj.PrefabPath);
            }
            
            ResourcesExclude.AppendExcludedResources(bundles);

            BuildPipeline.BuildAssetBundles(
                folder,
                bundles.ToArray(),
                BuildAssetBundleOptions.None,
                BuildTarget.Android);

            EditorUserBuildSettings.androidBuildSubtarget = defaultAndroidBuildSubTarget;
        }

        private void BuildWebGLBundles(string folder)
        {
            _bundlePartIdx = 0;
            _buildedBundleParts.Clear();
            
            if (!SdkSettings.Features.WebGL.Enabled)
            {
                return;
            }

            var bundles = new List<AssetBundleBuild>();

            foreach (ObjectBuildDescription obj in ObjectBuildDescriptions)
            {
                if (obj.HasError)
                {
                    continue;
                }

                BuildAndSetAssetBundlePart(obj, bundles, $"webgl");

                bundles.Add(GetAssetBundleBuild(obj, "webgl"));
            }
            
            ResourcesExclude.AppendExcludedResources(bundles);

            BuildPipeline.BuildAssetBundles(
                folder,
                bundles.ToArray(),
                BuildAssetBundleOptions.None,
                BuildTarget.WebGL);
        }

        private AssetBundleBuild GetAssetBundleBuild(ObjectBuildDescription objectBuildDescription, string prefix = null)
        {
            AssetBundleBuild assetBundleBuild = default;

            var directory = new DirectoryInfo(objectBuildDescription.FolderPath);

            var files = directory.GetFiles("*.asset", SearchOption.AllDirectories);
            var assets = files.Select(file => file.GetAssetPath()).ToList();

            var resources = directory.GetDirectories("Resources", SearchOption.AllDirectories);
            foreach (var resource in resources)
            {
                var resourceFiles = resource.GetFiles("*.*", SearchOption.AllDirectories).ToList();
                resourceFiles = resourceFiles.Where(x => !x.FullName.EndsWith(".meta")).Where(x => !x.FullName.EndsWith(".cs")).Where(x => !x.FullName.EndsWith(".asmdef")).ToList();

                foreach (var resourceFile in resourceFiles)
                {
                    if (!assets.Contains(resourceFile.GetAssetPath()))
                    {
                        assets.Add(resourceFile.GetAssetPath());
                    }
                }
            }

            var prefabFile = new FileInfo(objectBuildDescription.PrefabPath);
            if (!assets.Contains(prefabFile.GetAssetPath()))
            {
                assets.Add(prefabFile.GetAssetPath());
            }

            var assetsArray = assets.ToArray();
            if (assetsArray.Length == 0)
            {
                throw new Exception($"No assets found to create asset bundle for object {objectBuildDescription.ObjectName}");
            }

            assetBundleBuild.assetNames = assetsArray;

            string assetBundleNamePrefix = string.IsNullOrEmpty(prefix) ? "" : $"{prefix}_";
            string guid = objectBuildDescription.ContainedObjectDescriptor.RootGuid.Replace("-", "");
            assetBundleBuild.assetBundleName = $"{assetBundleNamePrefix}{objectBuildDescription.ObjectName}_{guid}";

            return assetBundleBuild;
        }
    }
}