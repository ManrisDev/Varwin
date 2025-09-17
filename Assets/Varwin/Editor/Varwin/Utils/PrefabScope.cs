using System;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public class PrefabScope : IDisposable
    {
        public readonly string AssetPath;
        public readonly GameObject Prefab;

        public PrefabScope(GameObject gameObject)
        {
            AssetPath = CreateObjectUtils.GetPrefabPath(gameObject);
            Prefab = PrefabUtility.LoadPrefabContents(AssetPath);
        }

        public PrefabScope(string assetPath)
        {
            AssetPath = assetPath;
            Prefab = PrefabUtility.LoadPrefabContents(assetPath);
        }

        public void Dispose()
        {
            PrefabUtility.SaveAsPrefabAsset(Prefab, AssetPath);
            PrefabUtility.UnloadPrefabContents(Prefab);
        }
    }
}