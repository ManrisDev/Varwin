using UnityEngine;

namespace Varwin.Editor
{
    public static class EditorUtils
    {
        public static Vector2 CalcSize(string label)
        {
            var content = new GUIContent(label);
            var style = new GUIStyle();

            return style.CalcSize(content);
        }
        
        public static Vector2 CalcSize(string label, GUIStyle guiStyle)
        {
            var content = new GUIContent(label);

            return guiStyle.CalcSize(content);
        }
    }
}