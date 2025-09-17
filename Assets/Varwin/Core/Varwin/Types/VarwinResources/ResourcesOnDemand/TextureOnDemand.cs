using System;
using UnityEngine;
using Varwin.Data.ServerData;

namespace Varwin
{
    public class TextureOnDemand : ResourceOnDemand<Texture>
    {
        public TextureOnDemand() : base()
        {
            
        }

        public TextureOnDemand(ResourceDto dto) : base(dto)
        {
            
        }

        [Obsolete("Use \"Resource\" property instead")]
        public Texture Texture => Resource;
        
        protected override bool IsResourceExist => Resource;
        protected override void DestroyResource()
        {
            UnityEngine.Object.Destroy(Resource);
        }
        
        public static implicit operator Texture(TextureOnDemand t) => t?.Resource;
        public static implicit operator TextureOnDemand(ResourceDto resourceDto) => new TextureOnDemand(resourceDto);
        public static implicit operator bool(TextureOnDemand t) => t != null;
    }
}
