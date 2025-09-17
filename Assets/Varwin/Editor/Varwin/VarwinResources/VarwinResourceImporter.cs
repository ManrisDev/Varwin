using System.IO;
using UnityEditor.AssetImporters;
using Varwin.Public;

namespace Varwin.Editor
{
    /// <summary>
    /// Импортер VarwinResource из файла.
    /// </summary>
    [ScriptedImporter(1, "vwr")]
    public class VarwinResourceImporter : ScriptedImporter
    {
        /// <summary>
        /// Метод, выполняемый при импорте ресурса из файла.
        /// </summary>
        /// <param name="context">Контекст выполнения импорта.</param>
        public override void OnImportAsset(AssetImportContext context)
        {
            var resource = VarwinResource.CreateFromFile(context.assetPath);
            context.AddObjectToAsset(Path.GetFileName(context.assetPath), resource);
            context.SetMainObject(resource);
        }
    }
}