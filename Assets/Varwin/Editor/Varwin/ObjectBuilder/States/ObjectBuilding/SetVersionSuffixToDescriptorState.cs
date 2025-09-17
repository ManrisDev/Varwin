using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public class SetVersionSuffixToDescriptorState : BaseObjectBuildingState
    {
        private Dictionary<string, string> _newNamesToOldNames;

        public SetVersionSuffixToDescriptorState(VarwinBuilder builder) : base(builder)
        {
            Label = "Set Version Suffix";
        }

        protected override void OnEnter()
        {
            _newNamesToOldNames = new Dictionary<string, string>();

            for (var i = 0; i < VarwinBuildingData.OldAssemblyNames.Length; i++)
            {
                string oldAsmdefName = VarwinBuildingData.OldAssemblyNames[i];
                string newAsmdefName = VarwinBuildingData.NewAssemblyNames[i];
                _newNamesToOldNames.Add(newAsmdefName, oldAsmdefName);
            }
        }

        protected override void Update(ObjectBuildDescription currentObjectBuildDescription)
        {
            Label = $"Set {currentObjectBuildDescription.ObjectName} Assembly Suffix";

            try
            {
                AssemblyDefinitionData asmdefData = AsmdefUtils.GetAssemblyDefinitionData(currentObjectBuildDescription.ContainedObjectDescriptor);
                var oldName = asmdefData.name;
                if (_newNamesToOldNames.ContainsKey(oldName))
                {
                    var suffix = asmdefData.name.SubstringAfterLast("_");
                    currentObjectBuildDescription.ContainedObjectDescriptor.AssemblySuffix = suffix;
                }
            }
            catch (Exception e)
            {
                var message = $"{currentObjectBuildDescription.ObjectName} error: Problem when setting suffix to Varwin Object Descriptor";
                Debug.LogError($"{message}\n{e}", currentObjectBuildDescription.ContainedObjectDescriptor);
                currentObjectBuildDescription.HasError = true;
                if (ObjectBuildDescriptions.Count == 1)
                {
                    EditorUtility.DisplayDialog("Error!", message, "OK");
                }
            }
        }
    }
}