using Newtonsoft.Json;

namespace OpenXR.Extensions
{
    public class OpenXRRuntimeInfo 
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("library_path")]
        public string LibraryPath { get; set; }
    }
}