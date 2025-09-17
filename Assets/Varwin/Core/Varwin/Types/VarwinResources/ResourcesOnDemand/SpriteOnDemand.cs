using UnityEngine;
using Varwin.Data.ServerData;

namespace Varwin
{
    public class SpriteOnDemand : ResourceOnDemand<Sprite>
    {
        public SpriteOnDemand() : base()
        {
            
        }
        
        public SpriteOnDemand(ResourceDto dto) : base(dto)
        {
            
        }

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
                    _sourceTexture = (Texture2D) GameStateData.GetResourceValue(value.Guid);
                    
                    if (IsResourceExist)
                    {
                        CreateSprite();
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
            
            if (resourceValue is Texture2D texture && texture)
            {
                _sourceTexture = texture;
                CreateSprite();
                OnLoadedCall(Resource);
            }
        }

        private void CreateSprite()
        {
            Resource = Sprite.Create(_sourceTexture, new Rect(0, 0, _sourceTexture.width, _sourceTexture.height), 0.5f * Vector2.one, 100f);
        }

        protected override bool IsResourceExist => _sourceTexture;
        protected override void DestroyResource()
        {
            if (Resource)
            {
                UnityEngine.Object.Destroy(Resource);
                UnityEngine.Object.Destroy(_sourceTexture);
            }
        }

        private Texture2D _sourceTexture;
        
        public static implicit operator Sprite(SpriteOnDemand t) => t?.Resource;
        public static implicit operator SpriteOnDemand(ResourceDto resourceDto) => new SpriteOnDemand(resourceDto);
        public static implicit operator bool(SpriteOnDemand t) => t != null;
        
    }
}