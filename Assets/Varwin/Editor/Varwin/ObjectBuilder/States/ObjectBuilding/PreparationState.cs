using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Varwin.Public;

namespace Varwin.Editor
{
    public class PreparationState : BaseObjectBuildingState
    {
        public PreparationState(VarwinBuilder builder) : base(builder)
        {
            Label = $"Preparing";
        }

        protected override void Update(ObjectBuildDescription currentObjectBuildDescription)
        {
            Label = $"Prepare {currentObjectBuildDescription.ObjectName}";

            GameObject go = null;
            try
            {
                go = (GameObject) PrefabUtility.InstantiatePrefab(currentObjectBuildDescription.GameObject);
                PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

                if (go.GetComponentsInChildren<VarwinObject>().Length > 1)
                {
                    throw new Exception("More than one varwin object added!");
                }

                var varwinObjectDescriptor = go.GetComponent<VarwinObjectDescriptor>();
                varwinObjectDescriptor.PreBuild();

                currentObjectBuildDescription.RootGuid = varwinObjectDescriptor.RootGuid;
                SetupNewVersion(varwinObjectDescriptor);
                currentObjectBuildDescription.NewGuid = varwinObjectDescriptor.Guid;
                
                CreateObjectUtils.SetupAsmdef(varwinObjectDescriptor);
                CreateObjectUtils.SetupComponentReferences(varwinObjectDescriptor);
                SignatureUtils.SetupObjectSignatures(varwinObjectDescriptor);
                go.SetActive(true);
                CreateObjectUtils.ApplyPrefabInstanceChanges(go);

                var varwinObjectDescriptorsOnScene = UnityEngine.Object.FindObjectsOfType<VarwinObjectDescriptor>();
                foreach (var varwinObjectDescriptorOnScene in varwinObjectDescriptorsOnScene)
                {
                    if (varwinObjectDescriptorOnScene.Guid == varwinObjectDescriptor.Guid)
                    {
                        CreateObjectUtils.RevertPrefabInstanceChanges(varwinObjectDescriptorOnScene.gameObject);
                    }
                }

                var anyGlobalScript = CreateObjectUtils.GetGlobalScriptsPaths(currentObjectBuildDescription.ContainedObjectDescriptor).FirstOrDefault();
                if (anyGlobalScript != null)
                {
                    throw new Exception(string.Format(SdkTexts.ScriptWithoutAsmdefInAssetsFormat, anyGlobalScript));
                }
            }
            catch (Exception e)
            {
                string message = $"{currentObjectBuildDescription.ObjectName} error: Problem when preparation object {currentObjectBuildDescription.GameObject}";
                Debug.LogError($"{message}\n{e}", currentObjectBuildDescription.ContainedObjectDescriptor);
                currentObjectBuildDescription.HasError = true;
                if (ObjectBuildDescriptions.Count == 1)
                {
                    EditorUtility.DisplayDialog("Error!", message, "OK");
                }

                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }
            finally
            {
                if (go)
                {
                    UnityEngine.Object.DestroyImmediate(go);
                }
            }
        }

        private void SetupNewVersion(VarwinObjectDescriptor varwinObjectDescriptor)
        {
            varwinObjectDescriptor.Guid = Guid.NewGuid().ToString();
            varwinObjectDescriptor.CurrentVersionWasBuilt = false;
            varwinObjectDescriptor.CurrentVersionWasBuiltAsMobileReady = false;
            varwinObjectDescriptor.Signatures = null;
        }

        protected override void OnExit()
        {
            Builder.Serialize();
        }
    }
}