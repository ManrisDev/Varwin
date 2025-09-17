using System.IO;
using Microsoft.Win32;

namespace OpenXR.Extensions
{
    /// <summary>
    /// Описание HTC Vive Port.
    /// </summary>
    public class VivePortRuntime : OpenXRWindowsRuntimeBase
    {
        /// <summary>
        /// Путь в реестре до установленного HTC Vive Port.
        /// </summary>
        private const string InstallRegeditPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\HtcVive\Updater";
        
        /// <summary>
        /// Путь к значению пути установленного HTC Vive Port.
        /// </summary>
        private const string InstallRegeditKey = "AppPath";
        
        /// <summary>
        /// Относительный путь до Json.
        /// </summary>
        private const string JsonRelativePath = @"App/ViveVRRuntime/ViveVR_openxr/ViveOpenXR.json";

        /// <summary>
        /// Имя подсистемы.
        /// </summary>
        public override string Name => "HTC Vive Port";

        /// <summary>
        /// Инициализация подсистемы.
        /// </summary>
        public VivePortRuntime()
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