using Varwin.Data;

namespace Varwin.SceneTemplateBuilding
{
    public class SceneTemplateBundleJson : IJsonSerializable
    {
        public string name;
        public string description;
        public string image;
        public string assetBundleLabel;
        public string[] dllNames;
    }
}