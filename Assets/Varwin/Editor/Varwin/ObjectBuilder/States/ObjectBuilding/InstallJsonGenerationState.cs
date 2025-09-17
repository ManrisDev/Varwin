using System;
using UnityEditor;
using UnityEngine;
using Varwin.Public;

namespace Varwin.Editor
{
    public class InstallJsonGenerationState : BaseObjectBuildingState
    {
        public InstallJsonGenerationState(VarwinBuilder builder) : base(builder)
        {
            Label = $"Creating configurations";
        }

        protected override void Update(ObjectBuildDescription currentObjectBuildDescription)
        {
            Label = $"Creating configuration for {currentObjectBuildDescription.ObjectName}";
            
            try
            {
                VarwinObjectDescriptor varwinObjectDescriptor = currentObjectBuildDescription.ContainedObjectDescriptor;
                string typeName = $"Varwin.Types.{varwinObjectDescriptor.Namespace}.{varwinObjectDescriptor.Name}Wrapper";
                
                var wrapperType = Type.GetType(varwinObjectDescriptor.WrapperAssemblyQualifiedName);

                if (wrapperType == null)
                {
                    AssemblyDefinitionData asmdef = AsmdefUtils.LoadAsmdefData(AsmdefUtils.FindAsmdef(currentObjectBuildDescription.PrefabPath));
                    string wrapperTypeName = $"Varwin.Types.{varwinObjectDescriptor.Namespace}.{varwinObjectDescriptor.Name}Wrapper, {asmdef.name}, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
                    wrapperType = Type.GetType(wrapperTypeName);
                }

                if (wrapperType == null)
                {
                    string wrapperTypeName = $"Varwin.Types.{varwinObjectDescriptor.Namespace}.{varwinObjectDescriptor.Name}Wrapper, {varwinObjectDescriptor.Name}, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
                    wrapperType = Type.GetType(wrapperTypeName);
                }

                if (wrapperType == null)
                {
                    string message = $"Can't get wrapper for \"{currentObjectBuildDescription.ObjectName}\" ({typeName})";
                    throw new Exception(message);
                }
                
                try
                {
                    currentObjectBuildDescription.ContainedObjectDescriptor.ConfigBlockly = BlocklyBuilder.CreateBlocklyConfig(wrapperType, currentObjectBuildDescription);
                    currentObjectBuildDescription.ConfigBlockly = currentObjectBuildDescription.ContainedObjectDescriptor.ConfigBlockly;
                }
                catch (BlocklyBuilder.BlocklyArgumentsException e)
                {
                    if (ObjectBuildDescriptions.Count == 1)
                    {
                        EditorUtility.DisplayDialog("Message",
                            string.Format(SdkTexts.CannotBuildActionArgumentMismatchFormat, currentObjectBuildDescription.ContainedObjectDescriptor.Name),
                            "OK");
                    }
                    
                    throw;
                }
                catch (BlocklyBuilder.BlocklyArgsFormatIsNotEqualsException e)
                {
                    if (ObjectBuildDescriptions.Count == 1)
                    {
                        EditorUtility.DisplayDialog("Message",
                            string.Format(SdkTexts.ArgsFormatInTypeIsNotEquals, currentObjectBuildDescription.ContainedObjectDescriptor.Name),
                            "OK");
                    }

                    throw;
                }
            }
            catch (Exception e)
            {
                string message = string.Format(SdkTexts.ProblemWhenRunBuildVarwinObjectFormat, $"Can't build \"{currentObjectBuildDescription.ObjectName}\"", e);
                Debug.LogError(message, currentObjectBuildDescription.ContainedObjectDescriptor);
                currentObjectBuildDescription.HasError = true;
            }
        }
    }
}