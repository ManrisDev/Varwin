using Newtonsoft.Json;

namespace OpenXR.Extensions
{
    public class OpenXRRuntimeDescription
    {
        [JsonProperty("file_format_version")]
        public string FileFormatVersion { get; set; }
        
        [JsonProperty("runtime")]
        public OpenXRRuntimeInfo RuntimeInfo { get; set; }
    }
}