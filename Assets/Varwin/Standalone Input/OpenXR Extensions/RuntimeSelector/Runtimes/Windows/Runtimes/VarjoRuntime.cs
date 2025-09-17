using System.IO;
using Microsoft.Win32;

namespace OpenXR.Extensions
{
    /// <summary>
    /// Описание Varjo.
    /// </summary>
    public class VarjoRuntime : OpenXRWindowsRuntimeBase
    {
        /// <summary>
        /// Путь в реестре до установленного Varjo.
        /// </summary>
        private const string InstallRegeditPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Varjo\Runtime";
        
        /// <summary>
        /// Путь к значению пути установленного Varjo.
        /// </summary>
        private const string InstallRegeditKey = "InstallDir";
        
        /// <summary>
        /// Относительный путь до Json.
        /// </summary>
        private const string JsonRelativePath = @"varjo-openxr/VarjoOpenXR.json";

        /// <summary>
        /// Название подсистемы.
        /// </summary>
        public override string Name => "Varjo";

        /// <summary>
        /// Инициализация подсистемы.
        /// </summary>
        public VarjoRuntime()
        {
            var value = Registry.GetValue(InstallRegeditPath, InstallRegeditKey, null);
            if (value is string installedPath)
            {
                var fileInfo = new FileInfo(Path.Combine(installedPath, JsonRelativePath));
                JsonPath = Path.Combine(fileInfo.DirectoryName, fileInfo.Name);
            }
        }
    }
}