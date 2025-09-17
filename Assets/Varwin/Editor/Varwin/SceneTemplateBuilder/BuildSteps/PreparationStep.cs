using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Varwin.Editor;
using Object = UnityEngine.Object;

namespace Varwin.SceneTemplateBuilding
{
    public class PreparationStep : BaseSceneTemplateBuildStep
    {
        public PreparationStep(SceneTemplateBuilder builder) : base(builder)
        {
        }
        
        public override void Update()
        {
            base.Update();
            var missingReferences = "";

            foreach (var sceneObject in Object.FindObjectsOfType<GameObject>(true))
            {
                if (!PrefabUtility.IsPrefabAssetMissing(sceneObject))
                {
                    continue;
                }

                missingReferences += $"{sceneObject.name}\n";
            }

            if (!string.IsNullOrWhiteSpace(missingReferences))
            {
                var message = string.Format(SdkTexts.CannotBuildSceneWithMissingReferencesFormat, missingReferences);
                EditorUtility.DisplayDialog(SdkTexts.SceneTemplateBuildErrorMessage, message, "OK");

                HasErrors = true;
                throw new Exception(message);
            }

            if (!Directory.Exists(Builder.DestinationFolder))
            {
                try
                {
                    Directory.CreateDirectory(Builder.DestinationFolder);
                }
                catch
                {
                    var message = string.Format(SdkTexts.CannotCreateDirectoryFormat, Builder.DestinationFolder);
                    Debug.LogError(message);
                    
                    EditorUtility.DisplayDialog(SdkTexts.CannotCreateDirectoryTitle, message, "OK");
                    throw;
                }
            }
        }
    }
}