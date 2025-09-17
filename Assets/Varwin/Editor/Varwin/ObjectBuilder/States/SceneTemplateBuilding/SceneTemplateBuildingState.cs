using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using Varwin.SceneTemplateBuilding;
using Object = UnityEngine.Object;

namespace Varwin.Editor
{
    public class SceneTemplateBuildingState : BaseSceneTemplateBuildingState
    {
        private SceneTemplateBuilder _sceneTemplateBuilder;

        public override bool IsSceneBuildFinished => _sceneTemplateBuilder != null && (_sceneTemplateBuilder.IsFinished || _sceneTemplateBuilder.HasErrors);

        public SceneTemplateBuildingState(VarwinBuilder builder) : base(builder)
        {
            Label = "Scene template building";
        }

        protected override void OnInitialize()
        {
            SceneTemplateBuilder.Deserialize();
        }

        protected override void OnEnter()
        {
            Builder.Serialize();
        }

        protected override void Update(SceneTemplateBuildDescription sceneTemplateBuildDescription)
        {
            try
            {
                if (CurrentIndex < Builder.CurrentBuildingSceneIndex)
                {
                    CurrentIndex++;
                    return;
                }

                _sceneTemplateBuilder = SceneTemplateBuilder.Deserialize(); 
                if (_sceneTemplateBuilder == null)
                {
                    EditorSceneManager.OpenScene(sceneTemplateBuildDescription.Path);
                    sceneTemplateBuildDescription.Name = Path.GetFileNameWithoutExtension(sceneTemplateBuildDescription.Path);
                    sceneTemplateBuildDescription.RootGuid = Object.FindObjectOfType<WorldDescriptor>().RootGuid;
                    CreateSceneTemplateWindow.PrepareSceneForBuild();
                    sceneTemplateBuildDescription.NewGuid = Object.FindObjectOfType<WorldDescriptor>().Guid;
                    _sceneTemplateBuilder = CreateSceneTemplateWindow.GetSceneTemplateBuilder();
                    _sceneTemplateBuilder.PrepareBuildSteps();
                }

                _sceneTemplateBuilder.PreviewCamera = Object.FindObjectOfType<SceneCamera>();
                
                _sceneTemplateBuilder.Update();

                if (_sceneTemplateBuilder.IsFinished || _sceneTemplateBuilder.HasErrors)
                {
                    Builder.CurrentBuildingSceneIndex++;
                    sceneTemplateBuildDescription.HasError = _sceneTemplateBuilder.HasErrors;
                    
                    CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.None);
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                }
                else
                {
                    _sceneTemplateBuilder.Serialize();
                }
                
                Builder.Serialize();
            }
            catch (Exception e)
            {
                sceneTemplateBuildDescription.HasError = true;
                Debug.LogError($"Scene Template Building Error:\n{e}");
            }
        }
    }
}