using System.IO;
using UnityEditor;
using UnityEngine;
using Varwin.Editor.PreviewGenerators;

namespace Varwin.Editor
{
    public static class IconBuilder 
    {
        public static void Build(ObjectBuildDescription build)
        {           
            var settings = new PreviewSettings(ImageSize.Square256, Vector2Int.one, true, false);
            var generator = new PreviewGenerator();
            
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(build.ContainedObjectDescriptor.Prefab);
            var texture = generator.Generate(prefab, settings, build, typeof(IconGenerationState));
            
            var bytes = settings.GetBytes(texture);
            File.WriteAllBytes(build.IconPath, bytes);
            
            AssetDatabase.ImportAsset(build.IconPath);
            
            CreateObjectUtils.ApplyPrefabInstanceChanges(build.ContainedObjectDescriptor.gameObject);
        }
    }
}