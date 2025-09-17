using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Varwin.Data.ServerData
{
    public static class ResourceRequestTypeEx
    {
        public static readonly Dictionary<Type, ResourceRequestType> TypeToRequestType = new()
        {
            { typeof(GameObject), ResourceRequestType.Model },
            { typeof(Texture), ResourceRequestType.Image },
            { typeof(Sprite), ResourceRequestType.Image },
            { typeof(TextureOnDemand), ResourceRequestType.Image },
            { typeof(SpriteOnDemand), ResourceRequestType.Image },
            { typeof(TextAsset), ResourceRequestType.TextFile },
            { typeof(AudioClip), ResourceRequestType.Audio },
            { typeof(AudioClipOnDemand), ResourceRequestType.Audio },
            { typeof(VarwinVideoClip), ResourceRequestType.Video },
            { typeof(VideoOnDemand), ResourceRequestType.Video }
        };
        
        public static string ToSearchString(this ResourceRequestType resourceRequestType)
        {
            string[] formats = resourceRequestType.ToSearchArray();

            return string.Join(",", formats.Select(x => $"\"{x}\""));
        }

        public static string[] ToSearchArray(this ResourceRequestType resourceRequestType)
        {
            IEnumerable<string> formats;
            
            switch (resourceRequestType)
            {
                case ResourceRequestType.All:
                    formats = ResourceFormatCodes.AllFormats;
                    break;
                case ResourceRequestType.Image:
                    formats = ResourceFormatCodes.ImageFormats;
                    break;
                case ResourceRequestType.Model:
                    formats = ResourceFormatCodes.ModelFormats;
                    break;
                case ResourceRequestType.TextFile:
                    formats = ResourceFormatCodes.TextFormats;
                    break;
                case ResourceRequestType.Audio:
                    formats = ResourceFormatCodes.AudioFormats;
                    break;
                case ResourceRequestType.Video:
                    formats = ResourceFormatCodes.VideoFormats;
                    break;
                default:
                    return null;
            }

            return formats.ToArray();
        }
    }
}