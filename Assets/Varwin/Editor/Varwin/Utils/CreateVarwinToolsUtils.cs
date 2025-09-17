using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public static class CreateVarwinToolsUtils
    {
        private const string DefaultPrefabPath = "Assets/Varwin/Scenes/Resources/VARWIN_TOOLS.prefab";

        [MenuItem("Varwin/Create/Varwin Tools")]
        [MenuItem("GameObject/Varwin/Varwin Tools", false, 10)]
        public static void CreateVarwinTools()
        {
            var toolsPrefab = AssetDatabase.LoadAssetAtPath<VarwinTools>(DefaultPrefabPath);

            if (!toolsPrefab)
            {
                var assetGuid = AssetDatabase.FindAssets("VARWIN_TOOLS", new[] { "Assets/Varwin" }).FirstOrDefault();
                if (!string.IsNullOrEmpty(assetGuid))
                {
                    toolsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(assetGuid))?.GetComponent<VarwinTools>();
                }

                if (!toolsPrefab)
                {
                    Debug.LogError($"Can't find {nameof(VarwinTools)} prefab in {DefaultPrefabPath}");
                    return;
                }
            }

            PrefabUtility.InstantiatePrefab(toolsPrefab);
        }
    }
}