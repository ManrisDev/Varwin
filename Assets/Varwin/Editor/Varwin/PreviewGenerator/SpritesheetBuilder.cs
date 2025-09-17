using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Varwin.Public;
using Varwin.Editor.PreviewGenerators;

namespace Varwin.Editor
{
    public static class SpritesheetBuilder 
    {
        public static void Build(VarwinObjectDescriptor descriptor, string folder, ObjectBuildDescription buildDescription)
        {
            var settings = new PreviewSettings(ImageSize.MarketplaceLibrary, new Vector2Int(15, 1), false, true);
            var generator = new PreviewGenerator();
            
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(descriptor.Prefab);
            var texture = generator.Generate(prefab, settings, buildDescription, typeof(PreviewGenerationState));

            var bytes = settings.GetBytes(texture);
            var path = GetExportPath(settings, folder, descriptor.RootGuid);
            File.WriteAllBytes(path, bytes);
        }

        public static string GetExportPath(PreviewSettings settings, string folder, string name)
        {
            var extension = settings == null || settings.ExportAsJpg ? ".jpg" : ".png";
            return folder + "/spritesheet_" + name + extension;
        }
    }
}