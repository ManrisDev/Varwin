using System.IO;
using Newtonsoft.Json;

namespace Varwin.SceneTemplateBuilding
{
    public class SceneTemplateLogicOnlyBuilder : SceneTemplateBuilder
    {
        public override BuilderType BuildType { get; set; } = BuilderType.LogicOnly;

        public override void PrepareBuildSteps()
        {
            IsStarted = true;

            BuildSteps = new();

            BuildSteps.Enqueue(new CheckSceneCacheStep(this));
            BuildSteps.Enqueue(new SaveSceneStep(this));
            BuildSteps.Enqueue(new PreparationStep(this));
            BuildSteps.Enqueue(new InstallJsonGenerationStep(this));

            BuildSteps.Enqueue(new AsmdefReferencesCollectingStep(this));
            BuildSteps.Enqueue(new RenameAssembliesToNewNamesStep(this));
            BuildSteps.Enqueue(new SerializeDataAndRefreshStep(this));

            BuildSteps.Enqueue(new CollectDllFilesStep(this));
            BuildSteps.Enqueue(new RenameAssembliesToOldNamesStep(this));
            BuildSteps.Enqueue(new ZippingLogicFilesStep(this));
        }

        public override void Serialize()
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            var json = JsonConvert.SerializeObject(this, Formatting.Indented, jsonSerializerSettings);
            File.WriteAllText(TempStateFilename, json);
        }
    }
}