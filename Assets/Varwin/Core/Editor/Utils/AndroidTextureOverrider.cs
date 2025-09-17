using System;
using UnityEditor;

namespace Varwin.Editor
{
    public static class AndroidTextureOverrider
    {
        public static void OverrideTextures(string rootPath)
        {
            var paths = AssetDatabase.GetDependencies(rootPath);
            foreach (var path in paths)
            {
                var assetImporter = AssetImporter.GetAtPath(path);
                if (assetImporter == null)
                {
                    continue;
                }
                
                var textureImporter = assetImporter as TextureImporter;
                if (textureImporter == null)
                {
                    continue;
                }

                if (textureImporter.textureType == TextureImporterType.Sprite)
                {
                    continue;
                }

                if (textureImporter.textureType == TextureImporterType.Default)
                {
                    continue;
                }

                TextureImporterPlatformSettings settings = textureImporter.GetPlatformTextureSettings("Android");
                settings.overridden = true;
                settings.format = GetFormat(textureImporter);
                textureImporter.SetPlatformTextureSettings(settings);
            }
            
            AssetDatabase.SaveAssets();
        }

        private static TextureImporterFormat GetFormat(TextureImporter importer)
        {
            if (importer.textureType == TextureImporterType.NormalMap)
            {
                return TextureImporterFormat.ASTC_6x6;
            }

            if (importer.DoesSourceTextureHaveAlpha() || importer.alphaSource != TextureImporterAlphaSource.None)
            {
                return TextureImporterFormat.ASTC_6x6;
            }
            else
            {
                return TextureImporterFormat.ASTC_6x6;
            }
        }
    }
}