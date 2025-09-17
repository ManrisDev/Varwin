using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public class SourcePackagingState : BaseObjectBuildingState
    {
        public SourcePackagingState(VarwinBuilder builder) : base(builder)
        {
            Label = "Source packing";
        }

        protected override void OnEnter()
        {
            if (Directory.Exists(VarwinBuildingPath.SourcePackages))
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(VarwinBuildingPath.SourcePackages);
            }
            catch
            {
                string message = string.Format(SdkTexts.CannotCreateDirectoryFormat, VarwinBuildingPath.SourcePackages);
                Debug.LogError(message);
                EditorUtility.DisplayDialog(SdkTexts.CannotCreateDirectoryTitle, message, "OK");
                    
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }
        }

        protected override void Update(ObjectBuildDescription currentObjectBuildDescription)
        {
            Label = string.Format(SdkTexts.PackSourcesStep, currentObjectBuildDescription.ObjectName);
            
            try
            {
                if (!currentObjectBuildDescription.ContainedObjectDescriptor.SourcesIncluded)
                {
                    return;
                }
                
                var collector = new VarwinDependenciesCollector();
                var paths = collector.CollectPathsForObject(currentObjectBuildDescription.ContainedObjectDescriptor.Prefab);

                string filePath = $"{VarwinBuildingPath.SourcePackages}/{currentObjectBuildDescription.ContainedObjectDescriptor.Name}.unitypackage";
                AssetDatabase.ExportPackage(paths.ToArray(), filePath, ExportPackageOptions.Default);
            }
            catch (Exception e)
            {
                currentObjectBuildDescription.HasError = true;
                string message = string.Format(SdkTexts.ProblemWhenPackSourcesFormat, e.Message);
                
                if (ObjectBuildDescriptions.Count == 1)
                {
                    EditorUtility.DisplayDialog("Error!", message, "OK");
                }

                Debug.LogError($"{message}\n{e}", currentObjectBuildDescription.ContainedObjectDescriptor);
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }
        }
    }
}