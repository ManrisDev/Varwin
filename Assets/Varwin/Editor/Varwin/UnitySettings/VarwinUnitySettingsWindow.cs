using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    [InitializeOnLoad]
    public class VarwinUnitySettingsWindow : EditorWindow
    {
        private static bool _forceShow = false;
         
        private static VarwinUnitySettingsWindow _window;

        [InitializeOnLoadMethod]
        private static void AddDefines()
        {
            if (!VarwinUnitySettings.Defines.Contains("VARWIN"))
            {
                VarwinUnitySettings.Defines = "VARWIN;" + VarwinUnitySettings.Defines;
            }
        }
        
        static VarwinUnitySettingsWindow()
        {            
            EditorApplication.update += Refresh;
        }

        private void Awake()
        {
            byte[] source = File.ReadAllBytes("Assets/Varwin/Editor/Varwin/UnitySettings/VarwinInputSettings/InputManager.asset");
            File.WriteAllBytes("ProjectSettings/InputManager.asset", source);
        }

        private void OnDestroy()
        {
            VarwinUnitySettings.Clear();
        }

        private static void Refresh()
        {
            if (NeedToShow())
            {
                if (SessionState.GetBool(VarwinUnitySettings.AcceptingRecommendedSettingsKey, false))
                {
                    AcceptAll();
                }
                else
                {
                    SessionState.EraseBool(VarwinUnitySettings.AcceptingRecommendedSettingsKey);
                    OpenWindow(false);
                }
            }
            
            EditorApplication.update -= Refresh;
        }

        private static bool NeedToShow()
        {
            bool show = _forceShow || VarwinUnitySettings.Options.Any(x => x.IsNeedToDraw);
            bool layersShow = VarwinUnitySettings.Layers.Any(x => !string.Equals(VarwinUnitySettings.GetLayer(x.Key), x.Value));
            bool tagsShow = !VarwinUnitySettings.TagsAreValid();
            return show || layersShow || tagsShow;
        }

        public static void OpenWindow(bool forceShow)
        {
            _forceShow = forceShow;
            _window = GetWindow<VarwinUnitySettingsWindow>(true);
            _window.minSize = new Vector2(400, 480);
            _window.titleContent = new GUIContent(SdkTexts.UnitySettingsWindowTitle);
        }

        Vector2 scrollPosition;

        public void OnGUI()
        {
            DrawLogo();
            EditorGUILayout.HelpBox(SdkTexts.RecommendedProjectSettings, MessageType.Warning);

            DrawScrollView();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            if (VarwinUnitySettings.Options.Any(x => x.IsNeedToDraw) || !VarwinUnitySettings.IsGraphicsSettingsValid())
            {
                DrawAcceptAll();
                DrawIgnoreAll();
            }
            else if (GUILayout.Button("Close"))
            {
                Close();
            }
            GUILayout.EndHorizontal();

            if (!NeedToShow())
            {
                Close();
            }
        }

        public void DrawLogo()
        {
            var logo = AssetDatabase.LoadAssetAtPath<Texture2D>(VarwinAboutWindow.VarwinLogoPath);
            var rect = GUILayoutUtility.GetRect(position.width, 90, GUI.skin.box);
            if (logo)
            {
                GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit);
            }
        }

        public void DrawScrollView()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            foreach (var option in VarwinUnitySettings.Options)
            {
                option.Draw();
            }

            DrawLayersList();
            DrawTagsList();
            DrawGraphicsSettingsAccept();
            
            DrawClearAllIgnores();
            
            GUILayout.EndScrollView();
        }

        public void DrawLayersList()
        {
            int count = VarwinUnitySettings.Layers.Count(x => (!string.Equals(VarwinUnitySettings.GetLayer(x.Key), x.Value)));

            if (count > 0)
            {
                EditorGUILayout.Space();
                
                GUILayout.Label("Layers:");
                
                var layerButtonStyle = new GUIStyle(EditorStyles.miniButtonLeft);
                
                foreach (var layer in VarwinUnitySettings.Layers)
                {
                    if (!string.Equals(VarwinUnitySettings.GetLayer(layer.Key), layer.Value))
                    {
                        if (GUILayout.Button($"Set Layer{layer.Key} as \"{layer.Value}\"", layerButtonStyle))
                        {
                            VarwinUnitySettings.SetLayer(layer.Key, layer.Value);
                            VarwinUnitySettings.Save();
                        }
                    }
                }

                if (count > 1)
                {
                    EditorGUILayout.Space();
                    
                    if (GUILayout.Button($"Apply all layers"))
                    {
                        ApplyAllLayers();
                    }
                }
                
                EditorGUILayout.Space();
            }
        }

        public void DrawTagsList()
        {
            bool isValid = VarwinUnitySettings.TagsAreValid();

            if (!isValid)
            {
                EditorGUILayout.Space();
                    
                if (GUILayout.Button($"Setup all tag"))
                {
                    ApplyAllTags();
                }
                
                EditorGUILayout.Space();
            }
        }
        
        public void DrawClearAllIgnores()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Clear All Ignores"))
            {
                foreach (var option in VarwinUnitySettings.Options)
                {
                    option.ClearIgnore();
                }
            }
            GUILayout.EndHorizontal();
        }

        public void DrawAcceptAll()
        {
            if (GUILayout.Button("Accept All"))
            {
                SessionState.SetBool(VarwinUnitySettings.AcceptingRecommendedSettingsKey, true);
                AcceptAll();
                EditorUtility.DisplayDialog("Accept All", SdkTexts.AllRecommendedOptionsWereApplied, "Ok");
                Close();
            }
        }

        public static void AcceptAll()
        {
            foreach (var option in VarwinUnitySettings.Options)
            {
                option.UseRecommended();
            }

            ApplyAllLayers();
            ApplyAllTags();

            if (!VarwinUnitySettings.IsGraphicsSettingsValid())
            {
                VarwinUnitySettings.OverwriteGraphicsSettings();
            }
        }

        public void DrawIgnoreAll()
        {
            if (GUILayout.Button("Ignore All"))
            {
                if (EditorUtility.DisplayDialog("Ignore All", "Are you sure?", "Yes, Ignore All", "Cancel"))
                {
                    foreach (var option in VarwinUnitySettings.Options)
                    {
                        // Only ignore those that do not currently match our recommended settings.
                        option.Ignore();
                    }
                    Close();
                }
            }
        }

        private static void ApplyAllTags()
        {
            VarwinUnitySettings.SetupTags();
            VarwinUnitySettings.Save();
        }

        private static void ApplyAllLayers()
        {
            foreach (var layer in VarwinUnitySettings.Layers)
            {
                VarwinUnitySettings.SetLayer(layer.Key, layer.Value);
            }

            VarwinUnitySettings.Save();
        }

        private void DrawGraphicsSettingsAccept()
        {
            if (!VarwinUnitySettings.IsGraphicsSettingsValid())
            {
                EditorGUILayout.Space();
                GUILayout.Label("Unity Graphics Settings required to be overridden by Varwin Graphics Settings");
                
                EditorGUILayout.Space();
                if (GUILayout.Button($"Apply Varwin Graphics Settings"))
                {
                    VarwinUnitySettings.OverwriteGraphicsSettings();
                }
            }
        }
    }
}
