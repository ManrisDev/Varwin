using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public class VarwinUnityVersionWarninigWindow : EditorWindow
    {
        private static VarwinUnityVersionWarninigWindow _window;

        private const string RequiredUnityVersion = "2021.3.0f1";
        private const string UnityVersionWarning = "Your unity version is {0}. Required unity version {1}. SDK may not work correctly.";
        private const string UnityVersionWarningTitle = "Unity Version Warning";

        private static string SkipWarningKey => $"UNITY_VERSION_WARNING_DONT_SHOW_{RequiredUnityVersion}";

        [InitializeOnLoadMethod]
        public static void OnLoad() => EditorApplication.update += CheckUnityVersion;
        
        public static void CheckUnityVersion()
        {
            EditorApplication.update -= CheckUnityVersion;

            if (Application.unityVersion != RequiredUnityVersion)
            {
                if(!PlayerPrefs.HasKey(SkipWarningKey))
                    PlayerPrefs.SetInt(SkipWarningKey, 0);

                ShowWindow();
            }
        }

        private static void ShowWindow()
        {
            if(_window || PlayerPrefs.GetInt(SkipWarningKey) == 1)
                return;

            _window = GetWindow<VarwinUnityVersionWarninigWindow>();

            _window.titleContent = new GUIContent(UnityVersionWarningTitle);
            _window.minSize = _window.maxSize = new Vector2(600, 150);
            
            string warningText = string.Format(UnityVersionWarning, Application.unityVersion, RequiredUnityVersion);

            Debug.LogError($"<Color=yellow>{warningText}. You can load required version from <a href=\"https://unity.com/releases/editor/archive\">Unity archive </a></Color>");

            _window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            var style = new GUIStyle(GUI.skin.label);
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.yellow;

            string warningText = string.Format(UnityVersionWarning, Application.unityVersion, RequiredUnityVersion);

            GUILayout.Label(warningText, style);

            if (GUILayout.Button($"Download {RequiredUnityVersion} unity version"))
            {
                Application.OpenURL($"unityhub://{RequiredUnityVersion}");
            }
            
            if (GUILayout.Button("Don't show me again"))
            {
                PlayerPrefs.SetInt(SkipWarningKey, 1);
                Close();
            }

            EditorGUILayout.EndVertical();
        }
    }
}