using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Varwin.Data
{
    public class AssetInfo : IJsonSerializable
    {
        public string AssetName { get; set; }
        public List<string> Assembly { get; set; }
        public List<string> AssetBundleParts { get; set; }
    }
}
