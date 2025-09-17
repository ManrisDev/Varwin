using System.IO;
using UnityEditor;
using UnityEngine;
using Varwin.Editor;

namespace Varwin.SceneTemplateBuilding
{
    public class CheckSceneCacheStep : BaseSceneTemplateBuildStep
    {
        public CheckSceneCacheStep(SceneTemplateLogicOnlyBuilder sceneTemplateLogicOnlyBuilder) : base(sceneTemplateLogicOnlyBuilder) { }

        public override void Update()
        {
            if (!Directory.Exists(SdkSettings.SceneTemplateBuildingFolderPath))
            {
                var errorMessage = $"Can't build scene template logic. Folder witch cache {SdkSettings.SceneTemplateBuildingFolderPath} doesn't exist.";
                Debug.LogError(errorMessage);
                EditorUtility.DisplayDialog("Build logic error", errorMessage, "OK");

                Builder.IsFinished = true;
                return;                
            }

            if (!File.Exists(Builder.DestinationFilePath))
            {
                var errorMessage = $"Can't build scene logic. File with cache {Builder.DestinationFilePath} is doesn't exists";
                Debug.LogError(errorMessage);
                EditorUtility.DisplayDialog("Build logic error", errorMessage, "OK");

                Builder.IsFinished = true;
                return;
            }
        }
    }
}