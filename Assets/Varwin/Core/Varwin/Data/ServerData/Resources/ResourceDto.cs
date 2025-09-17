using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Varwin.Data.ServerData
{
    public class ResourceDtoContainer: IJsonSerializable
    {
        public ResourceDto Node { get; set; }
    }
    
    public class ResourceDto : IJsonSerializable
    {
        public int Id { get; set; }
        public string Guid { get; set; }
        public I18n Name { get; set; }

        public string Assets { get; set; }
        public string Path => $"{Assets}/{Guid}.{Format}";
        public string Preview => $"{Assets}/preview.png";
        
        public string Format { get; set; }

        [JsonIgnore] public bool OnDemand { get; set; }
        [JsonIgnore] public bool ForceLoad { get; set; }
        
        [JsonIgnore] public bool StreamAudio { get; set; }

        [JsonIgnore] public TextureFormat? TextureFormat { get; set; } 
        
        public string GetLocalizedName()
        {
            return Name.GetCurrentLocale();
        }
    }
}