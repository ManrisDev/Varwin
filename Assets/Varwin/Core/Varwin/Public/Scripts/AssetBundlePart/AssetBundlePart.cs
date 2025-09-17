using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Varwin.Public
{
    [CreateAssetMenu(fileName = "AssetBundle Part", menuName = "Varwin/AssetBundle Part", order = 0)]
    public class AssetBundlePart : ScriptableObject
    {
        public List<Object> Assets;
    }
    
}
