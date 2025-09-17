using System;
using System.IO;
using Ionic.Zip;
using UnityEngine;
using Varwin.Editor;

namespace Varwin.SceneTemplateBuilding
{
    public class ZipFilesStep : BaseSceneTemplateBuildStep
    {
        public ZipFilesStep(SceneTemplateBuilder builder) : base(builder)
        {
        }

        public override void Update()
        {
            try
            {
                using (var loanZip = new ZipFile())
                {
                    loanZip.AddFiles(Builder.SceneTemplatePackingPaths, false, string.Empty);
                    loanZip.Save(Builder.DestinationFilePath);
                }

                Debug.Log(SdkTexts.ZipCreateSuccessMessage);
            }
            catch (Exception e)
            {
                Debug.LogError($"{SdkTexts.ZipCreateFailMessage}\n{e}");
            }
            finally
            {
                foreach (var file in Builder.SceneTemplatePackingPaths)
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
            }
        }
    }
}