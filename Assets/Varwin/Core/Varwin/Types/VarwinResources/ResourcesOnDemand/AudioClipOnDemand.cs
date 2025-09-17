using UnityEngine;
using Varwin.Data.ServerData;

namespace Varwin
{
    public class AudioClipOnDemand : ResourceOnDemand<AudioClip>
    {
        public AudioClipOnDemand() : base()
        {
            
        }

        public AudioClipOnDemand(ResourceDto dto) : base(dto)
        {
            
        }
        
        protected override bool IsResourceExist => Resource;
        protected override void DestroyResource()
        {
            UnityEngine.Object.Destroy(Resource);
        }
        
        public static implicit operator AudioClip(AudioClipOnDemand t) => t?.Resource;
        public static implicit operator AudioClipOnDemand(ResourceDto resourceDto) => new AudioClipOnDemand(resourceDto);
        public static implicit operator bool(AudioClipOnDemand t) => t != null;
    }
}