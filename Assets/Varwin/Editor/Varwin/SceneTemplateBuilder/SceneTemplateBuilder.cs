using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Varwin.Data;
using Varwin.Editor;

namespace Varwin.SceneTemplateBuilding
{
    public class SceneTemplateBuilder : IJsonSerializable
    {
        public enum BuilderType
        {
            Default,
            LogicOnly
        }

        public const string TempStateFilename = "SceneTemplateBuilding.json";

        public WorldDescriptorData WorldDescriptor;
        [JsonIgnore] public SceneCamera PreviewCamera;
        [JsonIgnore] public UnityEngine.SceneManagement.Scene Scene;

        public string DestinationFolder;
        public string DestinationFilePath;

        public string SceneName;
        public string ScenePath;

        public bool IsStarted;
        public bool IsFinished;
        public bool HasErrors;

        public List<string> SceneTemplatePackingPaths;
        public int CompletedStepsCount;

        public string[] OldAssemblyNames;
        public string[] NewAssemblyNames;

        public virtual BuilderType BuildType { get; set; } = BuilderType.Default;

        [JsonIgnore] public Queue<BaseSceneTemplateBuildStep> BuildSteps;

        public SceneTemplateBuilder()
        {
            
        }
        
        public void Initialize(WorldDescriptor worldDescriptor, SceneCamera previewCamera)
        {
            WorldDescriptor = new(worldDescriptor);
            PreviewCamera = previewCamera;
            Scene = SceneManager.GetActiveScene();
            SceneTemplatePackingPaths = new();

            ScenePath = Scene.path;
            SceneName = Path.GetFileNameWithoutExtension(ScenePath);
            
            DestinationFolder = SdkSettings.SceneTemplateBuildingFolderPath;
            DestinationFilePath = $"{DestinationFolder}/{SceneName}.vwst";
        }

        public virtual void PrepareBuildSteps()
        {
            IsStarted = true;

            BuildSteps = new();
            BuildSteps.Enqueue(new SaveSceneStep(this));
            BuildSteps.Enqueue(new PreparationStep(this));
            BuildSteps.Enqueue(new IconsGenerationStep(this));
            
            BuildSteps.Enqueue(new AsmdefReferencesCollectingStep(this));

            if (SdkSettings.Settings.DynamicVersioningSupport)
            {
                BuildSteps.Enqueue(new RenameAssembliesToNewNamesStep(this));
            }
            
            BuildSteps.Enqueue(new SerializeDataAndRefreshStep(this));
            BuildSteps.Enqueue(new AssetBundleBuildingStep(this));
            
            if (WorldDescriptor.SourcesIncluded)
            {
                BuildSteps.Enqueue(new SourcePackingStep(this));
            }
            
            BuildSteps.Enqueue(new InstallJsonGenerationStep(this));
            BuildSteps.Enqueue(new BundleJsonGenerationStep(this));
            
            BuildSteps.Enqueue(new CollectDllFilesStep(this));

            if (SdkSettings.Settings.DynamicVersioningSupport)
            {
                BuildSteps.Enqueue(new RenameAssembliesToOldNamesStep(this));
            }

            BuildSteps.Enqueue(new ZipFilesStep(this));
        }

        public void Update()
        {
            if (!IsStarted || IsFinished)
            {
                return;
            }

            if (EditorApplication.isCompiling)
            {
                return;
            }
            
            if (BuildSteps.Count > 0)
            {
                CompletedStepsCount++;
                try
                {
                    var buildStep = BuildSteps.Dequeue();
                    buildStep.Update();
                }
                catch (Exception e)
                {
                    HasErrors = true;
                    IsFinished = true;
                    throw;
                }
            }
            else
            {
                IsFinished = true;
            }
        }

        public void BuildImmediate()
        {
            while (BuildSteps.Count > 0)
            {
                CompletedStepsCount++;
                try
                {
                    var buildStep = BuildSteps.Dequeue();
                    buildStep.Update();
                }
                catch (Exception e)
                {
                    HasErrors = true;
                    IsFinished = true;
                    throw;
                }
            }
        }

        public virtual void Serialize()
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            var json = JsonConvert.SerializeObject(this, Formatting.Indented, jsonSerializerSettings);
            File.WriteAllText(TempStateFilename, json);
        }

        public static SceneTemplateBuilder Deserialize()
        {
            if (!File.Exists(TempStateFilename))
            {
                return null;
            }
            
            string json = File.ReadAllText(TempStateFilename);
            File.Delete(TempStateFilename);
            var sceneTemplateBuilder = JsonConvert.DeserializeObject<SceneTemplateBuilder>(json);
            if (sceneTemplateBuilder.BuildType == BuilderType.LogicOnly)
                sceneTemplateBuilder = JsonConvert.DeserializeObject<SceneTemplateLogicOnlyBuilder>(json);

            sceneTemplateBuilder.PrepareBuildSteps();
            for (int i = 0; i < sceneTemplateBuilder.CompletedStepsCount; ++i)
            {
                sceneTemplateBuilder.BuildSteps.Dequeue();
            }

            Debug.LogWarning($"Deserialized {sceneTemplateBuilder.GetType()}");
            return sceneTemplateBuilder;
        }
    }
}
