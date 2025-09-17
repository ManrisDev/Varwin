using System.Collections.Generic;

namespace Varwin.Data.ServerData
{
    public static class ResourceFormatCodes
    {
        public static readonly IEnumerable<string> TextFormats = new List<string>
        {
            "txt",
            "csv"
        };

        public static readonly IEnumerable<string> ImageFormats = new List<string>
        {
            "png",
            "jpg",
            "jpeg"
        };

        public static readonly IEnumerable<string> ModelFormats = new List<string>
        {
            "fbx",
            "obj",
            "dae",
            "glb",
            "gltf"        
        };
        
        public static readonly IEnumerable<string> AudioFormats = new List<string>
        {
            "aif",
            "ogg",
            "wav",
        };
        
        public static readonly IEnumerable<string> VideoFormats = new List<string>
        {
            "mp4",
            "mov",
            "webm",
            "wmv",
        };
        
        public static readonly IEnumerable<string> AllFormats = new List<string>
        {
            // text
            "txt",
            "csv",
            // image
            "png",
            "jpg",
            "jpeg",
            // model
            "fbx",
            "obj",
            "dae",
            "glb",
            "gltf",
            // audio
            "aif",
            "ogg",
            "wav",
            // video
            "mp4",
            "mov",
            "webm",
            "wmv",
        };
    }
}