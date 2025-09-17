using System.IO;
using Microsoft.Win32;

namespace OpenXR.Extensions
{
    /// <summary>
    /// Подсистема SteamVR.
    /// </summary>
    public class SteamVRRuntime : OpenXRWindowsRuntimeBase
    {
        /// <summary>
        /// Путь в реестре до установленного SteamVR.
        /// </summary>
        private const string InstallRegeditPath = @"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam";
        
        /// <summary>
        /// Путь к значению пути установленного SteamVR.
        /// </summary>
        private const string InstallRegeditKey = "SteamPath";
        
        /// <summary>
        /// Относительный путь до Json.
        /// </summary>
        private const string JsonRelativePath = @"steamapps/common/SteamVR/steamxr_win64.json";
        
        /// <summary>
        /// Имя подсистемы.
        /// </summary>
        public override string Name => "SteamVR";

        /// <summary>
        /// Инициализация подсистемы.
        /// </summary>
        public SteamVRRuntime()
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