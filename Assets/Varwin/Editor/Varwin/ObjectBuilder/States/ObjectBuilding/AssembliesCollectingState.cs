using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public class AssembliesCollectingState : BaseObjectBuildingState
    {
        public AssembliesCollectingState(VarwinBuilder builder) : base(builder)
        {
            Label = $"Assemblies collecting";
        }

        protected override void OnEnter()
        {
            DllHelper.ForceUpdate();

            if (!Directory.Exists(VarwinBuildingPath.BakedObjects))
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

        protected override void Update(ObjectBuildDescription currentObjectBuildDescription)
        {
            Label = $"Assemblies collecting for " + currentObjectBuildDescription.ObjectName;
            
            try
            {
                var dllPaths = DllHelper.GetFromObject(currentObjectBuildDescription).Keys;
                
                foreach (string dllPath in dllPaths)
                {
                    if (!File.Exists(dllPath))
                    {
                        string oldSupportedFile = dllPath.Replace("_" + currentObjectBuildDescription.Guid, "");

                        if (!File.Exists(oldSupportedFile))
                        {
                            string message = $"Can't build object {currentObjectBuildDescription.ObjectName}. File hasn't been compiled: {dllPath}. Please check your assembly name.";
                            throw new Exception(message);
                        }
                        else
                        {
                            Debug.LogWarning(
                                "WARNING! Old object detected. Please, change your asmdef to <objectname>_<objectguidwithoutdashes> format! Now using file " +
                                oldSupportedFile);
                            currentObjectBuildDescription.Assemblies.Add(oldSupportedFile);
                            continue;
                        }
                    }
                    
                    currentObjectBuildDescription.Assemblies.Add(dllPath);
                }

                string anyGlobalScript = CreateObjectUtils.GetGlobalScriptsPaths(currentObjectBuildDescription.ContainedObjectDescriptor).FirstOrDefault(); 
                if (anyGlobalScript != null)
                {
                    throw new Exception(string.Format(SdkTexts.ScriptWithoutAsmdefInAssetsFormat, anyGlobalScript));
                }
            }
            catch (Exception e)
            {
                string message = $"{currentObjectBuildDescription.ObjectName} error: Problem when collecting assemblies";
                Debug.LogError($"{message}\n{e}", currentObjectBuildDescription.ContainedObjectDescriptor);
                currentObjectBuildDescription.HasError = true;
                if (ObjectBuildDescriptions.Count == 1)
                {
                    EditorUtility.DisplayDialog("Error!", message, "OK");
                }
                
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }
        }
    }
}