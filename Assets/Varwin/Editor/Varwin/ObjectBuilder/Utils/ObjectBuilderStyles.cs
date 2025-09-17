using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public static class ObjectBuilderStyles
    {
        public const int MessagePadding = 10;
        
        public static readonly GUIStyle ResultMessageStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            padding = new RectOffset(MessagePadding, MessagePadding, MessagePadding, 0)
        };

        public static readonly GUIStyle RedResultMessageStyle = new GUIStyle(ResultMessageStyle)
        {
            normal = {textColor = Color.red}
        };

        public static readonly GUIStyle RedResultInlineMessageStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            padding = new RectOffset(MessagePadding, MessagePadding, 0, 0),
            normal = {textColor = Color.red}
        };

        public static readonly GUIStyle GreenResultMessageStyle = new GUIStyle(ResultMessageStyle)
        {
            normal = {textColor = EditorGUIUtility.isProSkin ? Color.green : Color.Lerp(Color.black, Color.green, 0.5f)}
        };
    }
}