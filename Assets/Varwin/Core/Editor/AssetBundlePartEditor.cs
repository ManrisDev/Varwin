using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Varwin.Public
{
    [CustomEditor(typeof(AssetBundlePart))]
    public class AssetBundlePartEditor : UnityEditor.Editor
    {
        private AssetBundlePart _assetBundlePart;
        
        private SerializedProperty _assetsProperty;

        private void Awake()
        {
            OnEnable();
        }

        private void OnEnable()
        {
            _assetBundlePart = serializedObject.targetObject as AssetBundlePart;
            
            _assetsProperty = serializedObject.FindProperty("Assets");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            if (GUILayout.Button("Auto fill"))
            {
                FindAssets();
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(_assetsProperty, true);
            
            serializedObject.ApplyModifiedProperties();
        }
        
        public void FindAssets()
        {
            var assetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(_assetBundlePart));

            var assetsPaths = AssetDatabase.FindAssets("", new[] {assetPath});

            _assetBundlePart.Assets = new List<Object>();
            foreach (var path in assetsPaths)
            {
                var findedPath = AssetDatabase.GUIDToAssetPath(path);
                if (Directory.Exists(findedPath))
                {
                    continue;
                }

                var asset = AssetDatabase.LoadAssetAtPath<Object>(findedPath);
                if (!_assetBundlePart.Assets.Contains(asset) && asset != _assetBundlePart)
                {
                    _assetBundlePart.Assets.Add(asset);
                }
            }
            
            serializedObject.Update();

        }
    }
}
