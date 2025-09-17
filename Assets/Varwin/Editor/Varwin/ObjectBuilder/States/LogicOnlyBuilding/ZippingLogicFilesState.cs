using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;

namespace Varwin.Editor.LogicOnlyBuilding
{
    /// <summary>
    /// Шаг, переопределяющий добавление файлов в финальный архив объекта.
    /// </summary>
    public class ZippingLogicFilesState : ZippingFilesState
    {
        public ZippingLogicFilesState(VarwinBuilder builder) : base(builder) => Label = "Baking objects logic";

        /// <summary>
        /// Переопределение списка файлов, которые будут добавлены к архиву объекта.
        /// </summary>
        /// <param name="currentObjectBuildDescription">Описание билда.</param>
        /// <param name="filesToZip">Список файлов, которые будут добавлены к архиву.</param>
        protected override void AddFiles(ObjectBuildDescription currentObjectBuildDescription, List<string> filesToZip)
        {
            filesToZip.Add(WriteInstallJson(currentObjectBuildDescription));
            filesToZip.AddRange(CollectAssemblies(currentObjectBuildDescription));
        }

        /// <summary>
        /// Переопределение архивирования файлов объекта: использование старых бандов, обновление файлов логики и install.json.
        /// </summary>
        /// <param name="files">Список файлов для добавления.</param>
        /// <param name="zipFilePath">Путь сохранения архива.</param>
        protected override void ZipFiles(List<string> files, string zipFilePath)
        {
            var folderGuild = Guid.NewGuid().ToString();
            var tempDirectoryPath = $"{Path.Combine(UnityProject.Path, VarwinBuildingPath.BakedObjects, folderGuild)}";
            Directory.CreateDirectory(tempDirectoryPath);

            using (ZipFile cachedArchive = ZipFile.Read(zipFilePath))
            {
                cachedArchive.ExtractAll(tempDirectoryPath);
            }

            var cachedFiles = Directory.GetFiles(tempDirectoryPath);
            var filesForCheck = files.Select(Path.GetFileName).ToArray();

            foreach (string cachedFile in cachedFiles)
            {
                if(filesForCheck.Contains(Path.GetFileName(cachedFile)))
                    File.Delete(cachedFile);
            }

            File.Delete(zipFilePath);

            using (ZipFile loanZip = new ZipFile())
            {
                loanZip.AddFiles(files, false, "");
                loanZip.AddFiles(Directory.GetFiles(tempDirectoryPath), false, "");
                loanZip.Save(zipFilePath);
            }

            foreach (string file in files)
            {
                TryDeleteFile(file);
            }

            Directory.Delete(tempDirectoryPath, true);
        }
    }
}