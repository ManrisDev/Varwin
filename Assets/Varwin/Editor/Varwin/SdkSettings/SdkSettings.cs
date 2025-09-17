using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace Varwin.Editor
{
    /// <summary>
    /// Класс для обращения к настройкам SDK.
    /// </summary>
    public static class SdkSettings
    {
        /// <summary>
        /// Путь до объекта, в котором хранятся настройки Varwin SDK.
        /// </summary>
        private const string SettingsFilePath = "Assets/Varwin/Editor/Varwin/SdkSettings/SdkSettings.asset";

        /// <summary>
        /// Дефолтный локальный путь для создание Varwin объектов.
        /// </summary>
        public static string DefaultObjectCreationPath => "Assets/Objects";

        /// <summary>
        /// Дефолтный локальный путь для билда объектов.
        /// </summary>
        public static string DefaultObjectBuildingPath => "BakedObjects";

        /// <summary>
        /// Дефолтный локальный путь для билда шаблонов сцен.
        /// </summary>
        public static string DefaultSceneTemplateBuildingPath => "BakedSceneTemplates";

        /// <summary>
        /// Путь до папки, в которой по дефолту будут создаваться объекты.
        /// </summary>
        public static string ObjectCreationFolderPath => Settings.ObjectCreationFolderPath;

        /// <summary>
        /// Путь до папки, в которую билдятся объекты.
        /// </summary>
        public static string ObjectBuildingFolderPath => !string.IsNullOrEmpty(Settings.ObjectBuildingFolderPath)
            ? Settings.ObjectBuildingFolderPath
            : DefaultObjectBuildingPath;

        /// <summary>
        /// Путь до папки, в которую билдятся шаблоны сцен.
        /// </summary>
        public static string SceneTemplateBuildingFolderPath => !string.IsNullOrEmpty(Settings.SceneTemplateBuildingFolderPath)
            ? Settings.SceneTemplateBuildingFolderPath
            : DefaultSceneTemplateBuildingPath;

        /// <summary>
        /// Свойство, ссылающееся на настройки Varwin SDK.
        /// </summary>
        public static VarwinSDKSettings Settings => _settings ? _settings : _settings = GetSettingsObject();

        private static VarwinSDKSettings _settings;

        /// <summary>
        /// Применение настроек при инициализации SDK.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void Setup()
        {
            _settings = GetSettingsObject();
            ApplySettings();
        }

        /// <summary> 
        /// Применть настройки Varwin SDK.
        /// </summary>
        public static void ApplySettings()
        {
            Features.DeveloperMode.Enabled = Settings.DeveloperMode;
            Features.Mobile.Enabled = Settings.MobileBuildSupport;
            Features.Linux.Enabled = Settings.LinuxBuildSupport;
            Features.WebGL.Enabled = false;
            Features.DynamicVersioning.Enabled = Settings.DynamicVersioningSupport;
            Features.Changelog.Enabled = Settings.ChanglogEditing;
            Features.OverrideDefaultObjectSettings.Enabled = Settings.OverrideDefaultVarwinObjectDescriptorSettings;
            Features.AddBehavioursAtRuntime.Enabled = Settings.AddBehavioursAtRuntime;
            Features.MobileReady.Enabled = Settings.MobileReady;
            Features.SourcesIncluded.Enabled = Settings.SourcesIncluded;
            Features.DisableSceneLogic.Enabled = Settings.DisableSceneLogic;

            SetScriptingDefines();
        }

        /// <summary>
        /// Обновить дефайны проекта, если необходимо.
        /// </summary>
        private static void SetScriptingDefines()
        {
            const string developerModeDefine = "VARWIN_DEVELOPER_MODE";

            var targetGroup = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            PlayerSettings.GetScriptingDefineSymbols(targetGroup, out string[] currentDefines);

            var newDefines = new List<string>();
            newDefines.AddRange(currentDefines);

            if (Settings.DeveloperMode && !currentDefines.Contains(developerModeDefine))
            {
                newDefines.Add(developerModeDefine);
            }
            else if (!Settings.DeveloperMode && currentDefines.Contains(developerModeDefine))
            {
                newDefines.Remove(developerModeDefine);
            }

            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup),
                newDefines.ToArray());
        }

        /// <summary>
        /// Загрузка или создание объекта с настройками Varwin SDK.
        /// </summary>
        /// <returns>Объект с настройками Varwin SDK.</returns>
        private static VarwinSDKSettings GetSettingsObject()
        {
            if (!File.Exists(SettingsFilePath))
            {
                var settings = VarwinSDKSettings.CreateInstance(true);
                AssetDatabase.CreateAsset(settings, SettingsFilePath);

                return settings;
            }

            return AssetDatabase.LoadAssetAtPath<VarwinSDKSettings>(SettingsFilePath);
        }

        /// <summary>
        /// Инициализация ссылок на фичи Varwin SDK.
        /// </summary>
        public static class Features
        {
            public static readonly SdkFeature DeveloperMode = new();
            public static readonly SdkFeature Mobile = new();
            public static readonly SdkFeature Linux = new();
            public static readonly SdkFeature WebGL = new();
            public static readonly SdkFeature DynamicVersioning = new();
            public static readonly SdkFeature Changelog = new();

            public static readonly SdkFeature OverrideDefaultObjectSettings = new();
            public static readonly SdkFeature AddBehavioursAtRuntime = new();
            public static readonly SdkFeature MobileReady = new();
            public static readonly SdkFeature SourcesIncluded = new();
            public static readonly SdkFeature DisableSceneLogic = new();
        }
    }
}