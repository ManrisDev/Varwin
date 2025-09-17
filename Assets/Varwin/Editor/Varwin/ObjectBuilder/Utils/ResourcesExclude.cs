using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Varwin.Editor
{
    /// <summary>
    /// Класс, описывающий выключение ресурсов из AssetBundle'ов.
    /// </summary>
    public static class ResourcesExclude
    {
        /// <summary>
        /// GUID'ы ресурсов, которые будут исключены из билда.
        /// </summary>
        private static readonly List<string> _excludedResourcesGuids = new()
        {
            "4d819fedd0266884c8d69ffd2aef0711", // Hiragino Font Variant
            "1032285219ac09347af6e586c6ff1a9f", // Hiragino Font Dynamic
            "07bd874c63767ba47940d885a8bae0ed", // Hiragino Font
            "d8b31dcbe28f20b47be2f1703ddb2afa", // Ubuntu Regular SDF
            "f32fabdce2e8eb544832ca96b17e4d24", // Ubuntu Bold
            "fa96e0a82eda5ba42a256f5d03294de4", // Ubuntu Bold Italic
            "3ac5c8843ecf7c84f9ab433f548e93ca", // Ubuntu Italic
            "afe3086c58743cc409684e189c5a7c70", // Ubuntu Regular Font
            "11449d6e729099b48aa85e2de00966c7", // Ubuntu Regular Dynamic Font
            "18b8b3f7dd50fc447bf24d66a515b319", // Ubuntu Light
            "4c6aaa54cce3ae74ab04309a81e2ac7d", // Ubuntu Light Italic
            "b78d926f04f66a540affb6b4849c9aa4", // Ubuntu Medium
            "0f99c8064e477914f9ee7a54fba71156", // Ubuntu Medium Italic
            
        };
        
        /// <summary>
        /// Функция, возвращающая Assetbundlebuild, включающий исключаемые из билда ресурсы.
        /// </summary>
        /// <returns>Часть исключаемых ресурсов.</returns>
        public static AssetBundleBuild GetExcludedResourcesBundle()
        {
            var assetBundleBuild = new AssetBundleBuild
            {
                assetBundleName = "ExcludedResources"
            };

            var assetBundlePaths = new List<string>();
            foreach (var guid in _excludedResourcesGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                if (!string.IsNullOrEmpty(assetPath))
                {
                    assetBundlePaths.Add(assetPath);
                }
            }

            assetBundleBuild.assetNames = assetBundlePaths.ToArray();
            return assetBundleBuild;
        }

        /// <summary>
        /// Добавление исключаемых ресурсов и исключение их же уже из готовых bundle'ов.
        /// </summary>
        /// <param name="bundles">Список bundle'ов, предназначенных для билда.</param>
        public static void AppendExcludedResources(List<AssetBundleBuild> bundles)
        {
            var excludedDependenciesBundle = GetExcludedResourcesBundle();

            for (var index = 0; index < bundles.Count; index++)
            {
                var bundle = bundles[index];
                var resultAssetNameList = bundle.assetNames.ToList();
                resultAssetNameList.RemoveAll(a => excludedDependenciesBundle.assetNames.Contains(a));
                bundle.assetNames = resultAssetNameList.ToArray();
                bundles[index] = bundle;
            }
            
            bundles.Add(excludedDependenciesBundle);
        }
    }
}