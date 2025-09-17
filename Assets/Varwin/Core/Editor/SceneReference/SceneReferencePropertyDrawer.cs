using UnityEditor;
using UnityEngine;

namespace Varwin
{
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferencePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sceneObjectProperty = GetSceneObjectProperty(property);
            sceneObjectProperty.objectReferenceValue = EditorGUI.ObjectField(position, sceneObjectProperty.objectReferenceValue, typeof(SceneAsset), false);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }

        private static SerializedProperty GetSceneObjectProperty(SerializedProperty property)
        {
            return property.FindPropertyRelative("Value");
        }
    }
}