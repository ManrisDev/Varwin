using System;
using Varwin.Data.ServerData;

namespace Varwin
{
    public class VideoOnDemand : ResourceOnDemand<VarwinVideoClip>
    {
        public override ResourceDto DTO
        {
            get => _dto;
            set
            {
                if (_dto == value)
                {
                    return;
                }

                if (value == null)
                {
                    Resource = null;
                    OnUnloadedCall();
                }
                else
                {
                    var resourceValue = GameStateData.GetResourceValue(value.Guid);
                    if (resourceValue is string url)
                    {
                        Resource = new VarwinVideoClip(url);    
                    }
                    else
                    {
                        Resource = null;
                    }

                    if (IsResourceExist)
                    {
                        OnLoadedCall(Resource);
                    }
                    else
                    {
                        OnUnloadedCall();
                    }
                }
                
                _dto = value;
            }
        }
        
        public VideoOnDemand() : base()
        {
            
        }

        public VideoOnDemand(ResourceDto dto) : base(dto)
        {
            
        }
        protected override bool IsResourceExist => Resource != null;
        protected override void DestroyResource()
        {
            Resource = null;
        }

        [Obsolete("Don't forget to clear memory with VideoPlayer.Stop()")]
        public override void Unload() => base.Unload();

        protected override void OnResourceLoaded(ResourceDto dto, object resourceValue)
        {
            if (DTO == null)
            {
                return;
            }

            if (dto != DTO)
            {
                return;
            }

            Resource = new VarwinVideoClip((string) resourceValue);
            OnLoadedCall(Resource);
        }
        
        public static implicit operator VarwinVideoClip(VideoOnDemand t) => t?.Resource;
        public static implicit operator VideoOnDemand(ResourceDto resourceDto) => new VideoOnDemand(resourceDto);
        public static implicit operator bool(VideoOnDemand t) => t != null;
    }
}