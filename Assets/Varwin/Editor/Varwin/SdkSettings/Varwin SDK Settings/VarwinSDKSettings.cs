using UnityEngine;

namespace Varwin.Editor
{
    /// <summary>
    /// Объект настроек Varwin SDK.
    /// </summary>
    public class VarwinSDKSettings : ScriptableObject
    {
        /// <summary>
        /// Локальный путь до папки, в которую будут создаваться Varwin объекты.
        /// </summary>
        [Tooltip("Path to folder where Varwin objects will be created. Path is relative to Unity project path.")]
        public string ObjectCreationFolderPath;

        /// <summary>
        /// Путь до папки, куда будут билдится объекты.
        /// </summary>
        [Tooltip("Path to folder where Varwin objects will be baked. Path is relative to Unity project path.")]
        public string ObjectBuildingFolderPath;

        /// <summary>
        /// Путь до папки, куда будут билдится шаблоны сцен.
        /// </summary>
        [Tooltip("Path to folder where Varwin scene templates will be baked. Path is relative to Unity project path.")]
        public string SceneTemplateBuildingFolderPath;

        /// <summary>
        /// Поддержка режима разработчика.
        /// </summary>
        [Tooltip("Enable developer mode. In developer mode you can build only logic for varwin object. Use with caution!")]
        public bool DeveloperMode;

        /// <summary>
        /// Поддержка сборки объектов и шаблонов сцен под мобильные устройства.
        /// </summary>
        [Tooltip("Enable mobile build support. If enabled, Varwin SDK will support build Varwin objects and scene templates for mobile devices.")]
        public bool MobileBuildSupport;

        /// <summary>
        /// Поддержка динамического версионирования.
        /// </summary>
        [Tooltip("Enable dynamic versioning. If enabled, Varwin SDK will support dynamic versioning of dlls for Varwin objects and scene templates." +
                 "Dynamic versioning change dll type (by changing dll name) every object build.")]
        public bool DynamicVersioningSupport;

        /// <summary>
        /// Поддержка сборки объектов и шаблонов сцен под Linux.
        /// </summary>
        [Tooltip("Enable linux support. If enabled, Varwin SDK will support build Varwin objects and scene templates for linux. ")]
        public bool LinuxBuildSupport;

        /// <summary>
        /// Переопределение настройок по умолчанию для Varwin объектов.
        /// </summary>
        [Tooltip(
            "Enable override default Varwin object descriptor settings. If enabled, Varwin SDK will override default Varwin object descriptor settings for every object.")]
        public bool OverrideDefaultVarwinObjectDescriptorSettings;

        /// <summary>
        /// Переопределение настройки AddBehavioursAtRuntime для всех Varwin объектов.
        /// </summary>
        [Tooltip("If enabled, Varwin SDK will override default AddBehavioursAtRuntime settings for every object.")]
        public bool AddBehavioursAtRuntime;

        /// <summary>
        /// Переопределение поддержки сборки объектов и шаблонов сцен под мобильные устройства.
        /// </summary>
        [Tooltip("If enabled, Varwin SDK override default MobileReady setting for every object.")]
        public bool MobileReady;

        /// <summary>
        /// Переопределение свойства SourcesIncluded для всех Varwin объектов.
        /// </summary>
        [Tooltip("If enabled, Varwin SDK include unitypackage with source files in object.")]
        public bool SourcesIncluded;

        /// <summary>
        /// Переопределение свойства DisableSceneLogic для всех Varwin объектов.
        /// </summary>
        [Tooltip("If enabled, Object will not be included in scene logic. That's mean, that object will not appear in blockly editor.")]
        public bool DisableSceneLogic;

        [HideInInspector] public bool ChanglogEditing;

        /// <summary>
        /// Создать объект с настройками Varwin SDK.
        /// </summary>
        /// <param name="useDefaultSettings">Использовать ли дефолтные настройки при создании объекта.</param>
        /// <returns>Созданный объект с настройками Varwin SDK.</returns>
        public static VarwinSDKSettings CreateInstance(bool useDefaultSettings)
        {
            var settings = CreateInstance<VarwinSDKSettings>();

            if (useDefaultSettings)
            {
                settings.LinuxBuildSupport = true;
                settings.MobileBuildSupport = true;
                settings.DynamicVersioningSupport = true;
            }

            settings.ObjectCreationFolderPath = SdkSettings.DefaultObjectCreationPath;
            settings.ObjectBuildingFolderPath = SdkSettings.DefaultObjectBuildingPath;
            settings.SceneTemplateBuildingFolderPath = SdkSettings.DefaultSceneTemplateBuildingPath;

            return settings;
        }
    }
}