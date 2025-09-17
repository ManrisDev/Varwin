using System;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public class WrapperGenerationState : BaseObjectBuildingState
    {
        public WrapperGenerationState(VarwinBuilder builder) : base(builder)
        {
            Label = "Generating wrappers";
        }
        
        protected override void Update(ObjectBuildDescription currentObjectBuildDescription)
        {
            Label = "Generating wrapper for " + currentObjectBuildDescription.ObjectName;
            
            try
            {
                WrapperGenerator.GenerateWrapper(currentObjectBuildDescription.ContainedObjectDescriptor);
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat(currentObjectBuildDescription.ContainedObjectDescriptor, SdkTexts.ProblemWhenRunBuildVarwinObjectFormat, "Can't build " + currentObjectBuildDescription.ObjectName, e);
                currentObjectBuildDescription.HasError = true;
            }
        }
        
        protected override void OnExit()
        {
            Builder.Serialize();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
    }
}