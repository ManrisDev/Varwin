using System.IO;
using Newtonsoft.Json;
using Varwin.Data;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Varwin.Editor
{
    public class AssemblyDefinitionData : IJsonSerializable
    {
        public string name;
        public string[] references;
        public string[] optionalUnityReferences;
        public string[] includePlatforms;
        public string[] excludePlatforms;
        public bool overrideReferences;
        public string[] precompiledReferences;
        public bool allowUnsafeCode;
        public bool autoReferenced;
        public string[] defineConstraints;

        public string Serialize()
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            return JsonConvert.SerializeObject(this, Formatting.Indented, jsonSerializerSettings);
        }
    }
}
