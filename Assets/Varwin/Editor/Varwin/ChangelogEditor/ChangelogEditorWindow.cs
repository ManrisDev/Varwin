using System;
using UnityEditor;
using UnityEngine;

namespace Varwin
{
    public class ChangelogEditorWindow : EditorWindow
    {
        private static readonly Rect WindowSize = new()
        {
            min = new(560, 400),
            max = new(800, 600)
        };

        public event ChangelogWindowBuildButtonPressed BuildButtonPressed;
        public event ChangelogWindowCancelButtonPressed CancelButtonPressed;
        
        public delegate void ChangelogWindowBuildButtonPressed(ChangelogEditorWindow changelogEditorWindow, string changelog, bool isChanged);
        public delegate void ChangelogWindowCancelButtonPressed(ChangelogEditorWindow changelogEditorWindow);
        
        public string Changelog;
        public bool IsChanged;
        public bool Destroyed;

        public static ChangelogEditorWindow OpenWindow()
        {
            return OpenWindow(string.Empty);
        }
        
        public static ChangelogEditorWindow OpenWindow(string changelog)
        {
            var changelogEditorWindow = GetWindow<ChangelogEditorWindow>(true, "Edit changelog", true);
            changelogEditorWindow.Changelog = changelog;
            changelogEditorWindow.IsChanged = false;
            
            changelogEditorWindow.minSize = WindowSize.min;
            changelogEditorWindow.maxSize = WindowSize.max;
            
            changelogEditorWindow.Show();

            return changelogEditorWindow;
        }

        private void Awake()
        {
            Destroyed = false;
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();

            var previousChangelog = Changelog;
            Changelog = EditorGUILayout.TextArea(Changelog, GUILayout.ExpandHeight(true));
            if (!string.Equals(previousChangelog, Changelog))
            {
                IsChanged = true;
            }
            
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Build"))
            {
                BuildButtonPressed?.Invoke(this, Changelog, IsChanged);
            }

            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
        }

        private void OnDestroy()
        {
            Destroyed = true;
            CancelButtonPressed?.Invoke(this);
        }
    }
}