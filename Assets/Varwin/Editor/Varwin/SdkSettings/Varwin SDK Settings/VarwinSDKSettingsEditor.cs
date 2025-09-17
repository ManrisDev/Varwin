using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    /// <summary>
    /// Кастомный инспектор объекта настроек Varwin SDK.
    /// </summary>
    [CustomEditor(typeof(VarwinSDKSettings))]
    public class VarwinSDKSettingsEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Сериализованное свойство пути к папке для создания объектов.
        /// </summary>
        private SerializedProperty _objectCreationFolderPath;

        /// <summary>
        /// Сериализованное свойство пути к папке для билда объектов.
        /// </summary>
        private SerializedProperty _objectBuildingFolderPath;

        /// <summary>
        /// Сериализованное свойство пути к папке для билда шаблонов сцен.
        /// </summary>
        private SerializedProperty _sceneTemplateBuildingFolderPath;

        /// <summary>
        /// Сериализованное свойство переключения режима разработчика.
        /// </summary>
        private SerializedProperty _developerMode;

        /// <summary>
        /// Сериализованное свойство поддержки мобильной сборки объектов и шаблонов сцен. 
        /// </summary>
        private SerializedProperty _mobileBuildSupport;

        /// <summary>
        /// Сериализованное свойство поддержки динамического версионирования.
        /// </summary>
        private SerializedProperty _dynamicVersioningSupport;

        /// <summary>
        /// Сериализованное свойство поддержки сборки объектов и шаблонов сцен под Linux. 
        /// </summary>
        private SerializedProperty _linuxBuildSupport;

        /// <summary>
        /// Переопределение дефолтных значений для VarwinObjectDescriptor'а у объектов.
        /// </summary>
        private SerializedProperty _overrideDefaultVarwinObjectDescriptorSettings;

        /// <summary>
        /// Переопределение дефолтного значения поля добавления стандартных поведений в рантайме.
        /// </summary>
        private SerializedProperty _addBehavioursAtRuntime;

        /// <summary>
        /// Переопределение дефолного значения поля сборки объекта под мобильные платформы.
        /// </summary>
        private SerializedProperty _mobileReady;

        /// <summary>
        /// Переопределение дефолтного значения поля включения исходников в сборку.
        /// </summary>
        private SerializedProperty _sourcesIncluded;

        /// <summary>
        /// Переопределение дефолтного значения поля отключения использования объекта в логике сцены.
        /// </summary>
        private SerializedProperty _disableSceneLogic;

        /// <summary>
        /// Инициализация инспектора.
        /// </summary>
        private void OnEnable()
        {
            if (!serializedObject?.targetObject)
            {
                return;
            }

            _objectCreationFolderPath = serializedObject.FindProperty(nameof(VarwinSDKSettings.ObjectCreationFolderPath));
            _objectBuildingFolderPath = serializedObject.FindProperty(nameof(VarwinSDKSettings.ObjectBuildingFolderPath));
            _sceneTemplateBuildingFolderPath = serializedObject.FindProperty(nameof(VarwinSDKSettings.SceneTemplateBuildingFolderPath));

            _developerMode = serializedObject.FindProperty(nameof(VarwinSDKSettings.DeveloperMode));
            _mobileBuildSupport = serializedObject.FindProperty(nameof(VarwinSDKSettings.MobileBuildSupport));
            _dynamicVersioningSupport = serializedObject.FindProperty(nameof(VarwinSDKSettings.DynamicVersioningSupport));
            _linuxBuildSupport = serializedObject.FindProperty(nameof(VarwinSDKSettings.LinuxBuildSupport));

            _overrideDefaultVarwinObjectDescriptorSettings =
                serializedObject.FindProperty(nameof(VarwinSDKSettings.OverrideDefaultVarwinObjectDescriptorSettings));
            _addBehavioursAtRuntime = serializedObject.FindProperty(nameof(VarwinSDKSettings.AddBehavioursAtRuntime));
            _mobileReady = serializedObject.FindProperty(nameof(VarwinSDKSettings.MobileReady));
            _sourcesIncluded = serializedObject.FindProperty(nameof(VarwinSDKSettings.SourcesIncluded));
            _disableSceneLogic = serializedObject.FindProperty(nameof(VarwinSDKSettings.DisableSceneLogic));
        }

        /// <summary>
        /// Отрисовка полей инспектора.
        /// </summary>
        public override void OnInspectorGUI()
        {
            EditorGUIUtility.labelWidth = 290;

            serializedObject.Update();
            DrawFoldersProperties();

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(_developerMode);
            EditorGUILayout.PropertyField(_mobileBuildSupport);
            if (_mobileBuildSupport.boolValue)
            {
                if (!AndroidPlatformHelper.IsAndroidPlatformInstalled)
                {
                    EditorGUILayout.HelpBox("Android platform not supported", MessageType.Error);
                    if (GUILayout.Button("Install android module (from unity hub)"))
                    {
                        AndroidPlatformHelper.InstallAndroidPlatformFromHub();
                    }
                    if (GUILayout.Button("Open setup android module page"))
                    {
                        AndroidPlatformHelper.OpenSetupAndroidPage();
                    }
                }
            }

            EditorGUILayout.PropertyField(_dynamicVersioningSupport);
            EditorGUILayout.PropertyField(_linuxBuildSupport);

            EditorGUILayout.PropertyField(_overrideDefaultVarwinObjectDescriptorSettings);

            if (_overrideDefaultVarwinObjectDescriptorSettings.boolValue)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.PropertyField(_addBehavioursAtRuntime);
                EditorGUILayout.PropertyField(_mobileReady);
                EditorGUILayout.PropertyField(_sourcesIncluded);
                EditorGUILayout.PropertyField(_disableSceneLogic);
            }

            var isAnyPropertyChanged = serializedObject.ApplyModifiedProperties();
            if (isAnyPropertyChanged)
            {
                SdkSettings.ApplySettings();
            }

            EditorGUIUtility.labelWidth = 0;
        }

        /// <summary>
        /// Возвращает истину, если путь является частью пути.
        /// </summary>
        /// <param name="parentPath">Путь.</param>
        /// <param name="childPath">Проверяемый полный путь.</param>
        /// <returns>Истина, если путь является частью.</returns>
        private static bool IsSubfolder(string parentPath, string childPath)
        {
            var parentInfo = new DirectoryInfo(parentPath);
            var childInfo = new DirectoryInfo(childPath).Parent;
            while (childInfo != null)
            {
                if(childInfo.FullName == parentInfo.FullName)
                {
                    return true;
                }
                
                childInfo = childInfo.Parent;
            }
            return false;
        }
        
        /// <summary>
        /// Отрисовка свойств для путей папок.
        /// </summary>
        private void DrawFoldersProperties()
        {
            var openFolderWidth = GUILayout.Width(50);

            GUILayout.Label("Creation Settings", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(_objectCreationFolderPath);
                if (GUILayout.Button("...", EditorStyles.miniButton, openFolderWidth))
                {
                    var path = EditorUtility.OpenFolderPanel("Object Creation Folder Path", SdkSettings.ObjectCreationFolderPath, "").Trim()
                        .TrimEnd('/');

                    if (string.IsNullOrEmpty(path))
                    {
                        path = _objectCreationFolderPath.stringValue;
                    }
                    else
                    {
                        var assetsPath = Path.Combine(UnityProject.Path, "Assets");
                        
                        if (IsSubfolder(assetsPath, path))
                        {
                            path = Path.GetRelativePath(UnityProject.Path, path);
                        }
                        else
                        {
                            path = _objectCreationFolderPath.stringValue;
                            EditorUtility.DisplayDialog("Error!", "Object Folder must be in unity project's Assets folder", "OK");
                        }
                    }

                    _objectCreationFolderPath.stringValue = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(_objectBuildingFolderPath);
                if (GUILayout.Button("...", EditorStyles.miniButton, openFolderWidth))
                {
                    var path = EditorUtility.OpenFolderPanel("Object Building Folder Path", SdkSettings.ObjectBuildingFolderPath, "").Trim()
                        .TrimEnd('/');

                    path = string.IsNullOrEmpty(path)
                        ? _objectBuildingFolderPath.stringValue 
                        : Path.GetRelativePath(UnityProject.Path, path);

                    _objectBuildingFolderPath.stringValue = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(_sceneTemplateBuildingFolderPath);
                if (GUILayout.Button("...", EditorStyles.miniButton, openFolderWidth))
                {
                    var path = EditorUtility.OpenFolderPanel("Scene Template Building Folder Path", SdkSettings.SceneTemplateBuildingFolderPath, "")
                        .Trim()
                        .TrimEnd('/');

                    path = string.IsNullOrEmpty(path)
                        ? _objectBuildingFolderPath.stringValue 
                        : Path.GetRelativePath(UnityProject.Path, path);

                    _sceneTemplateBuildingFolderPath.stringValue = path;
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}