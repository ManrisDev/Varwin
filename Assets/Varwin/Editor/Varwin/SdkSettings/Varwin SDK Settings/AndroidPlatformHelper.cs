using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    /// <summary>
    /// Класс-помошник для работы с android-платформой.
    /// </summary>
    public class AndroidPlatformHelper
    {
        /// <summary>
        /// Deep link для установки модуля андройда из unity hub.
        /// Работает для unity hub версии 3.8.0. В будущем может изменяться.
        /// </summary>
        private static readonly string SetupAndroidFormHubDeeplink = $"unityhub://{Application.unityVersion}?module=android";

        /// <summary>
        /// Ссылка на документация, где описывается ручная установка модуля андройда.
        /// </summary>
        private static readonly string SetupAndroidInstructionLink = $"https://docs.unity3d.com/{Application.unityVersion.Substring(0, 6)}/Documentation/Manual/android-sdksetup.html";

        /// <summary>
        /// Установлен ли модуль андройда для текущей платформы.
        /// </summary>
        public static bool IsAndroidPlatformInstalled => BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android);

        /// <summary>
        /// Установить модуль андройда для текущей версии редактора.
        /// Может не работать, если на ПК не установлен unityhub.
        /// </summary>
        public static void InstallAndroidPlatformFromHub() => Process.Start(SetupAndroidFormHubDeeplink);

        /// <summary>
        /// Открыть инструкцию по установке модуля андройда для текущей версии редактора unity.
        /// </summary>
        public static void OpenSetupAndroidPage() => Process.Start(SetupAndroidInstructionLink);
    }
}