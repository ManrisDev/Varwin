using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Varwin.Data;

namespace Varwin.Editor
{
    public class BundleJsonGenerationState : BaseObjectBuildingState
    {
        public BundleJsonGenerationState(VarwinBuilder builder) : base(builder)
        {
            Label = $"Creating bundles";
        }

        protected override void Update(ObjectBuildDescription currentObjectBuildDescription)
        {
            Label = $"Creating bundle for {currentObjectBuildDescription.ObjectName}";
            
            try
            {
                var assetInfo = new AssetInfo
                {
                    AssetName = currentObjectBuildDescription.ObjectName,
                    Assembly = DllHelper.GetFromObject(currentObjectBuildDescription).Keys.Select(Path.GetFileName).Reverse().ToList()
                };

                var assetBundleParts = currentObjectBuildDescription.ContainedObjectDescriptor.AssetBundleParts;
                
                if (assetBundleParts != null && assetBundleParts.Length != 0)
                {
                    assetInfo.AssetBundleParts = new List<string>();
                    
                    for (var i = 0; i < assetBundleParts.Length; i++)
                    {
                        assetInfo.AssetBundleParts.Add($"assetbundle_part{i}");                                
                    }
                }

                var jsonSerializerSettings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};
                string jsonConfig = JsonConvert.SerializeObject(assetInfo, Formatting.None, jsonSerializerSettings);
                currentObjectBuildDescription.ContainedObjectDescriptor.ConfigAssetBundle = jsonConfig;
                currentObjectBuildDescription.ConfigAssetBundle = jsonConfig;
            }
            catch (Exception e)
            {
                string message = string.Format(SdkTexts.ProblemWhenRunBuildVarwinObjectFormat, $"Can't build \"{currentObjectBuildDescription.ObjectName}\"", e);
                Debug.LogErrorFormat(currentObjectBuildDescription.ContainedObjectDescriptor, message);
                currentObjectBuildDescription.HasError = true;
            }
        }
    }
}