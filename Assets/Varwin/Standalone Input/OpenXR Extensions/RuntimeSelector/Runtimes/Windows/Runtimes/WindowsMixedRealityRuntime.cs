using System;
using System.IO;

namespace OpenXR.Extensions
{
    /// <summary>
    /// Описание WMR.
    /// </summary>
    public class WindowsMixedRealityRuntime : OpenXRWindowsRuntimeBase
    {
        /// <summary>
        /// Имя json файла.
        /// </summary>
        private const string JsonName = "MixedRealityRuntime.json";
        
        /// <summary>
        /// Имя подсистемы.
        /// </summary>
        public override string Name => "Windows Mixed Reality";

        /// <summary>
        /// Инициализация подсистемы.
        /// </summary>
        public WindowsMixedRealityRuntime()
        {
            var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
            JsonPath = Path.Combine(systemPath, JsonName);
        }
    }
}