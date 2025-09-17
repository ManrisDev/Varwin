using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public class AsmdefReferencesCollectingState : BaseObjectBuildingState
    {
        private AsmdefDynamicVersioningNamesGenerator _asmdefDynamicVersioningNamesGenerator;
        
        public AsmdefReferencesCollectingState(VarwinBuilder builder) : base(builder)
        {
            Label = $"Collect assembly definition references";
            _asmdefDynamicVersioningNamesGenerator = new();
        }

        protected override void OnEnter()
        {
            DllHelper.ForceUpdate();
        }

        protected override void Update(ObjectBuildDescription currentObjectBuildDescription)
        {
            Label = $"Collect assembly definition references for {currentObjectBuildDescription.ObjectName}";

            try
            {
                DllHelper.ForceUpdate();
                AsmdefUtils.Refresh();
                var asmdefData = AsmdefUtils.GetAssemblyDefinitionData(currentObjectBuildDescription.ContainedObjectDescriptor);
                _asmdefDynamicVersioningNamesGenerator.DeepCollectAsmdefNames(asmdefData);
            }
            catch (Exception e)
            {
                var message = $"{currentObjectBuildDescription.ObjectName} error: Problem when collect assembly definition references";
                Debug.LogError($"{message}\n{e}", currentObjectBuildDescription.ContainedObjectDescriptor);
                currentObjectBuildDescription.HasError = true;
                if (ObjectBuildDescriptions.Count == 1)
                {
                    EditorUtility.DisplayDialog("Error!", message, "OK");
                }

                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }
        }
        
        protected override void OnExit()
        {
            VarwinBuildingData.OldAssemblyNames = _asmdefDynamicVersioningNamesGenerator.AsmdefsOldToNewName.Keys.ToArray();
            VarwinBuildingData.NewAssemblyNames = _asmdefDynamicVersioningNamesGenerator.AsmdefsOldToNewName.Values.ToArray();
        }
    }
}