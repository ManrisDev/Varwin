using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace OpenXR.Extensions
{
    /// <summary>
    /// Механизм указания среды OpenXR в Windows.
    /// </summary>
    public class OpenXRWindowsRuntimeSelector : OpenXRRuntimeSelectorBase
    {
        /// <summary>
        /// Путь до активной подсистемы в реестре.
        /// </summary>
        private const string RegeditActiveRuntimePath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Khronos\OpenXR\1";
        
        /// <summary>
        /// Поле активного рантайма в реестре.
        /// </summary>
        private const string RegeditActiveRuntimeValue = "ActiveRuntime";
        
        /// <summary>
        /// Список рантаймов.
        /// </summary>
        private List<OpenXRRuntimeBase> _runtimes = new()
        {
            new OculusRuntime(),
            new SteamVRRuntime(),
            new WindowsMixedRealityRuntime(),
            new VivePortRuntime(),
            new VarjoRuntime()
        };

        /// <summary>
        /// Функция, возвращающая текущий рантайм.
        /// </summary>
        /// <returns>Текущий рантайм.</returns>
        public override OpenXRRuntimeBase GetActiveRuntime()
        {
            var path = Registry.GetValue(RegeditActiveRuntimePath, RegeditActiveRuntimeValue, null);
            if (!(path is string jsonPath))
            {
                return null;
            }
            
            foreach (var runtime in _runtimes)
            {
                if (runtime is not OpenXRWindowsRuntimeBase windowsRuntimeBase || string.IsNullOrWhiteSpace(windowsRuntimeBase.JsonPath))
                {
                    continue;
                }
                
                var runtimeFile = new FileInfo(windowsRuntimeBase.JsonPath);
                var currentFile = new FileInfo(jsonPath);
                if (currentFile.DirectoryName?.ToLowerInvariant() == runtimeFile.DirectoryName?.ToLowerInvariant() && currentFile.Name == runtimeFile.Name)
                {
                    return runtime;
                }
            }
            
            return new UnknownWindowsRuntime(jsonPath);
        }

        /// <summary>
        /// Функция, возвращающая список доступных рантаймов. 
        /// </summary>
        /// <returns>Список доступных рантаймов.</returns>
        public override IEnumerable<OpenXRRuntimeBase> GetAvailableRuntimes()
        {
            return _runtimes.FindAll(a => a.IsAvailable);
        }

        /// <summary>
        /// Метод, позволяющий указать активный рантайм.
        /// </summary>
        /// <param name="runtime">Рантайм.</param>
        public override void SetActiveRuntime(OpenXRRuntimeBase runtime)
        {
            var windowsRuntime = (OpenXRWindowsRuntimeBase) runtime;
            var processInfo = new ProcessStartInfo();
            processInfo.Verb = "runas";
            processInfo.FileName = "cmd";
            processInfo.Arguments = $@"/c reg add ""{RegeditActiveRuntimePath}"" /v {RegeditActiveRuntimeValue} /t REG_SZ /d ""{windowsRuntime.JsonPath}"" /f";

            var process = Process.Start(processInfo);
            process.WaitForExit();
//            Registry.SetValue(RegeditActiveRuntimePath, RegeditActiveRuntimeValue, windowsRuntime.JsonPath);
        }
    }
}