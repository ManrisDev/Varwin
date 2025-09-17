using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Varwin.Editor;

namespace Varwin.SceneTemplateBuilding
{
    public class SourcePackingStep : BaseSceneTemplateBuildStep
    {
        private readonly string _sourcesDirectory;
        
        public SourcePackingStep(SceneTemplateBuilder builder) : base(builder)
        {
            _sourcesDirectory = $"{UnityProject.Path}/SourcePackages";
        }
        
        public override void Update()
        {
            base.Update();
            
            if (!Directory.Exists(_sourcesDirectory))
            {
                try
                {
                    Directory.CreateDirectory(_sourcesDirectory);
                }
                catch
                {
                    var message = string.Format(SdkTexts.CannotCreateDirectoryFormat, _sourcesDirectory);
                    Debug.LogError(message);
                    EditorUtility.DisplayDialog(SdkTexts.CannotCreateDirectoryTitle, message, "OK");
                    throw;
                }
            }
            
            var dependenciesCollector = new VarwinDependenciesCollector();
            var dependenciesPaths = dependenciesCollector.CollectPathsForScene(Builder.ScenePath);
            
            var originSourcesPath = $"{_sourcesDirectory}/{Builder.SceneName}.unitypackage";
            var destinationSourcesPath = $"{Builder.DestinationFolder}/sources.unitypackage";
            
            AssetDatabase.ExportPackage(dependenciesPaths.ToArray(), originSourcesPath, ExportPackageOptions.Default);
            
            File.Copy(originSourcesPath, destinationSourcesPath, true);
            Builder.SceneTemplatePackingPaths.Add(destinationSourcesPath);
        }
    }
}