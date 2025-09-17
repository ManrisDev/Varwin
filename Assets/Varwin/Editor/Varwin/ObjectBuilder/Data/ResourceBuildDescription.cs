using Varwin.Data;

namespace Varwin.Editor
{
    public class ResourceBuildDescription : IJsonSerializable
    {
        public string Name;
        public string ResourcePath;
        public bool HasError = false;

    }
}