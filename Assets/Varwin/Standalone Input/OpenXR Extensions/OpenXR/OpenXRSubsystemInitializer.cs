using System;
using System.Diagnostics;
using System.IO;
#if UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)  
using Microsoft.Win32;
#endif
using Newtonsoft.Json;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace OpenXR.Extensions
{
    /// <summary>
    /// Инициализатор OpenXR Runtime.
    /// </summary>
    public static class OpenXRSubsystemInitializer
    {
#if UNITY_EDITOR_WIN || (!UNITY_EDITOR && UNITY_STANDALONE_WIN)
        /// <summary>
        /// Путь до OpenXR Runtime json в Windows.
        /// </summary>
        public static string JsonPath => (string) Registry.LocalMachine.OpenSubKey("SOFTWARE")?.OpenSubKey("KHRONOS")?.OpenSubKey("OpenXR")?.OpenSubKey("1")?.GetValue("ActiveRuntime");
        
#else
        /// <summary>
        /// Путь до OpenXR Runtime json в Linux.
        /// </summary>
        public static string JsonPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "openxr","1","active_runtime.json");
#endif

        /// <summary>
        /// Имеется ли активный рантайм.
        /// </summary>
        /// <returns>Истина, если есть.</returns>
        public static bool HasActiveRuntime()
        {
            return !string.IsNullOrEmpty(JsonPath) && File.Exists(JsonPath);
        }
        
        /// <summary>
        /// Запустить активный рантайм.
        /// </summary>
        public static void LaunchDefaultOpenXRRuntime()
        {
            if (!HasActiveRuntime())
            {
                Debug.LogError("OpenXR Active runtime not found!");
                return;
            }

            var defaultRuntimeDescription = File.ReadAllText(JsonPath);
            var openXRRuntimeDescription = JsonConvert.DeserializeObject<OpenXRRuntimeDescription>(defaultRuntimeDescription);

            if (openXRRuntimeDescription?.RuntimeInfo == null)
            {
                return;
            }

            var lowerInvariantName = openXRRuntimeDescription.RuntimeInfo.Name?.ToLowerInvariant();
            var lowerInvariantLibraryPath = openXRRuntimeDescription.RuntimeInfo.LibraryPath?.ToLowerInvariant();
            if (lowerInvariantName != null && lowerInvariantName.Contains("oculus"))
            {
                var jsonDirectory = new DirectoryInfo(Path.GetDirectoryName(JsonPath) ?? string.Empty);
                if (jsonDirectory.Parent == null)
                {
                    return;
                }
                
                var executePath = Path.Combine(jsonDirectory.Parent.ToString(), "oculus-client\\OculusClient.exe");
                if (string.IsNullOrEmpty(executePath))
                {
                    Process.Start(executePath);
                }
            }
            
            if (lowerInvariantName != null && lowerInvariantName.Contains("steamvr"))
            {
                Application.OpenURL("steam://rungameid/250820");
            }
            
            if (lowerInvariantLibraryPath != null && lowerInvariantLibraryPath.Contains("mixedreality"))
            {
                var executePath = "ms-holocamera:Mixed Reality Portal";
                Process.Start(executePath);
            }

            if (openXRRuntimeDescription.RuntimeInfo?.LibraryPath?.ToLower()?.Contains("monado") ?? false)
            {
                var executePath = "monado-service";
                Process.Start(executePath);
            }
        }
    }
}