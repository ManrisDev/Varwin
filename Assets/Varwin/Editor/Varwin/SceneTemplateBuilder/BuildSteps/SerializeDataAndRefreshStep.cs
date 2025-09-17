using UnityEditor;
using UnityEditor.Compilation;

namespace Varwin.SceneTemplateBuilding
{
    public class SerializeDataAndRefreshStep : BaseSceneTemplateBuildStep
    {
        public SerializeDataAndRefreshStep(SceneTemplateBuilder builder) : base(builder)
        {
        }

        public override void Update()
        {
            base.Update();
            
            Builder.Serialize();
            CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.None);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
    }
}