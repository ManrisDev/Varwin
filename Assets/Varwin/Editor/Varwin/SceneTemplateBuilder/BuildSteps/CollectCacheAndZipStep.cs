using System;
using System.IO;
using System.Linq;
using Ionic.Zip;

namespace Varwin.SceneTemplateBuilding
{
    /// <summary>
    /// Шаг билда сцены, отвечающий за сбор кэша и упаковку его, вместе с новой логикой, в *.vwst объект.
    /// </summary>
    public class ZippingLogicFilesStep : BaseSceneTemplateBuildStep
    {
        public ZippingLogicFilesStep(SceneTemplateBuilder builder) : base(builder) { }

        public override void Update()
        {
            var tempFolder = Path.Combine(Builder.DestinationFolder,$"scene_template_temp_{Guid.NewGuid().ToString()}");
            try
            {
                using (var loanZip = ZipFile.Read(Builder.DestinationFilePath))
                {
                    loanZip.ExtractAll(tempFolder);
                }

                var newFiles = Builder.SceneTemplatePackingPaths;
                var cachedFiles = Directory.GetFiles(tempFolder);

                var filesToDelete = cachedFiles.Select(Path.GetFileName).Intersect(newFiles.Select(Path.GetFileName));

                foreach (var file in filesToDelete)
                {
                    var path = Path.Combine(tempFolder, file);
                    File.Delete(path);
                }

                newFiles.ForEach(file => File.Move(file, Path.Combine(tempFolder, Path.GetFileName(file))));

                File.Delete(Builder.DestinationFilePath);

                using (var loanZip = new ZipFile())
                {
                    loanZip.AddFiles(Directory.GetFiles(tempFolder), false, string.Empty);
                    loanZip.Save(Builder.DestinationFilePath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
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

                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true);
                }
            }
        }
    }
}