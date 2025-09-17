using System;
using System.IO;
using UnityEngine;
using Varwin.Editor;

namespace Varwin.SceneTemplateBuilding
{
    public class CollectDllFilesStep : BaseSceneTemplateBuildStep
    {
        public CollectDllFilesStep(SceneTemplateBuilder builder) : base(builder)
        {
        }

        public override void Update()
        {
            base.Update();

            AsmdefUtils.Refresh();
            DllHelper.ForceUpdate();

            foreach (var dllName in Builder.NewAssemblyNames)
            {
                try
                {
                    var dllPath = DllHelper.FindInProject($"{dllName}.dll");
                    var outputDllPath = $"{Builder.DestinationFolder}/{Path.GetFileName(dllPath)}";

                    File.Copy(dllPath, outputDllPath);
                    Builder.SceneTemplatePackingPaths.Add(outputDllPath);
                }
                catch (DllNotFoundException e)
                {
                    Debug.LogError($"Not found dll for \"{dllName}\"\n{e}");
                }
            }
        }
    }
}