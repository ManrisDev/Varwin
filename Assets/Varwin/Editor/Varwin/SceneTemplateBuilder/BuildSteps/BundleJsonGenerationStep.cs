using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Varwin.SceneTemplateBuilding
{
    public class BundleJsonGenerationStep : BaseSceneTemplateBuildStep
    {
        public BundleJsonGenerationStep(SceneTemplateBuilder builder) : base(builder)
        {
        }
        
        public override void Update()
        {
            base.Update();

            var descriptor = Builder.WorldDescriptor;

            var localizedName = descriptor.LocalizedName;
            var defaultName = localizedName.Get(Varwin.Language.English)?.value ?? localizedName.FirstOrDefault(x => !string.IsNullOrEmpty(x.value))?.value;
            
            var localizedDescription = descriptor.LocalizedDescription;
            var defaultDescription = localizedDescription.Get(Varwin.Language.English)?.value ?? localizedDescription.FirstOrDefault(x => !string.IsNullOrEmpty(x.value))?.value;

            var sceneConfig = new SceneTemplateBundleJson
            {
                name = defaultName,
                description = defaultDescription,
                image = descriptor.Image,
                assetBundleLabel = descriptor.AssetBundleLabel,
                dllNames = Builder.NewAssemblyNames.Select(a=> $"{a}.dll").ToArray()
            };
            
            var bundleJson = JsonConvert.SerializeObject(sceneConfig, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore
            });
            
            var bundleJsonPath = $"{Builder.DestinationFolder}/bundle.json";
            File.WriteAllText(bundleJsonPath, bundleJson);
            Builder.SceneTemplatePackingPaths.Add(bundleJsonPath);
        }
    }
}