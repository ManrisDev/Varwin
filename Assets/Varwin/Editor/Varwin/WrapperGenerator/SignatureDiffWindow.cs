using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Varwin.Public;

namespace Varwin.Editor
{
    public class SignatureDiffWindow : EditorWindow
    {
        private static Vector2 MinWindowSize => new Vector2(420, 480);
        private static Vector2 MaxWindowSize => new Vector2(640, 800);
        private static Vector2 _scrollPosition;
        private static string _message;

        public static void OpenWindow()
        {
            var window = GetWindow<SignatureDiffWindow>(true, "Signature changes", true);
            window.minSize = MinWindowSize;
            window.maxSize = MaxWindowSize;
            window.Show();
        }

        public static void SetDiffs(IEnumerable<KeyValuePair<Signature, Signature>> diffs)
        {
            _message = diffs.Aggregate("", (current, signatureDiff) => current + SignatureUtils.MakeSignatureWarning(signatureDiff) + Environment.NewLine);
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            EditorGUILayout.TextArea(_message.TrimEnd(Environment.NewLine.ToCharArray()));

            EditorGUILayout.EndScrollView();
        }
    }
}