using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    /// <summary>
    /// Окно настроек Varwin SDK.
    /// </summary>
    public class SdkSettingsWindow : EditorWindow
    {
        /// <summary>
        /// Размер окна.
        /// </summary>
        private static readonly Vector2 WindowSize = new(560, 300);

        /// <summary>
        /// Ссылка на объект класса.
        /// </summary>
        private static SdkSettingsWindow _window;

        /// <summary>
        /// Логика инициализации окна.
        /// </summary>
        public static void OpenWindow()
        {
            _window = GetWindow<SdkSettingsWindow>(false, SdkTexts.SdkSettingsWindowTitle, true);
            
            _window.minSize = WindowSize;
            _window.maxSize = WindowSize;
            
            _window._settings = SdkSettings.Settings;
            _window.SetupEditor();

            _window.Show();
        }

        private VarwinSDKSettings _settings;
        private UnityEditor.Editor _editor;

        /// <summary>
        /// Отрисовка редактора объекта настроек.
        /// </summary>
        private void OnGUI()
        {
            if (_editor)
            {
                _editor.OnInspectorGUI();
            }
        }

        /// <summary>
        /// Установка ссылки на инспектор объекта настроек.
        /// </summary>
        private void SetupEditor()
        {
            _settings = SdkSettings.Settings;
            _editor = _settings ? UnityEditor.Editor.CreateEditor(_settings) : null;
        }
    }
}
