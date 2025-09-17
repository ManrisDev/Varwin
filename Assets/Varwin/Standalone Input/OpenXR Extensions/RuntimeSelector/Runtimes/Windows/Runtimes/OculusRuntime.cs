using System.IO;
using Microsoft.Win32;

namespace OpenXR.Extensions
{
    /// <summary>
    /// Подсистема Oculus.
    /// </summary>
    public class OculusRuntime : OpenXRWindowsRuntimeBase
    {
        /// <summary>
        /// Путь в реестре до установленного Oculus.
        /// </summary>
        private const string InstallRegeditPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Oculus";
        
        /// <summary>
        /// Путь к значению пути установленного Oculus.
        /// </summary>
        private const string InstallRegeditKey = "InstallLocation";
        
        /// <summary>
        /// Относительный путь до Json.
        /// </summary>
        private const string JsonRelativePath = @"Support\oculus-runtime\oculus_openxr_64.json";

        /// <summary>
        /// Имя подсистемы.
        /// </summary>
        public override string Name => "Oculus";

        /// <summary>
        /// Инициализация подсистемы.
        /// </summary>
        public OculusRuntime()
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