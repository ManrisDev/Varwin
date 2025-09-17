using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Varwin.Core;
using Varwin.Data.ServerData;

namespace Varwin
{
    /// <summary>
    /// Базовый класс для загружаемого ресурса.
    /// </summary>
    public abstract class ResourceOnDemand
    {
        /// <summary>
        /// Описание ресурса.
        /// </summary>
        public abstract ResourceDto DTO
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Базовый параметризированный загружаемый ресурс.
    /// </summary>
    /// <typeparam name="T">Тип ресурса.</typeparam>
    public abstract class ResourceOnDemand<T> : ResourceOnDemand where T: class
    {
        protected ResourceDto _dto;

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
                    OnUnloaded?.Invoke();
                }
                else
                {
                    Resource = (T) GameStateData.GetResourceValue(value.Guid);
                    
                    if (IsResourceExist)
                    {
                        OnLoaded?.Invoke(Resource);
                    }
                    else
                    {
                        OnUnloaded?.Invoke();
                    }
                }
                
                _dto = value;
            }
        }

        /// <summary>
        /// Ресурс.
        /// </summary>
        public T Resource { get; protected set; }

        /// <summary>
        /// Событие, вызываемое когда ресурс загружен.
        /// </summary>
        public event Action<T> OnLoaded;

        /// <summary>
        /// Событие, вызываемое когда ресурс выгружен
        /// </summary>
        public event Action OnUnloaded;

        public ResourceOnDemand()
        {
            ProjectData.GameModeChanging += OnGameModeChanging;
        }

        public ResourceOnDemand(ResourceDto dto) : this()
        {
            DTO = dto;
        }

        ~ResourceOnDemand()
        {
            if (IsResourceExist)
            {
                DestroyResource();
            }

            ProjectData.GameModeChanging -= OnGameModeChanging;
        }

        private void OnGameModeChanging(GameMode newGameMode)
        {
            Unload();
        }

        public void Load()
        {
            if (DTO == null)
            {
                return;
            }
            
            DTO.ForceLoad = true;
#if VARWINCLIENT
            AsyncRunner.Execute(ProjectLoadSystem.LoadResource(DTO), resource =>
            {
                OnResourceLoaded(DTO, resource.Value);
            });
#endif
        }

        public virtual void Unload()
        {
            if (DTO == null)
            {
                return;
            }

            if (GameStateData.UnloadResource(DTO))
            {
                OnResourceUnloaded();    
            }
        }

        protected virtual void OnResourceLoaded(ResourceDto dto, object resourceValue)
        {
            if (DTO == null)
            {
                return;
            }

            if (dto != DTO)
            {
                return;
            }
            
            Resource = (T) resourceValue;
            OnLoaded?.Invoke(Resource);
        }

        private void OnResourceUnloaded()
        {
            if (DTO == null)
            {
                return;
            }

            if (IsResourceExist)
            {
                DestroyResource();
            }
            
            OnUnloaded?.Invoke();
        }

        protected abstract bool IsResourceExist { get; }
        protected abstract void DestroyResource();

        protected void OnLoadedCall(T resource) => OnLoaded?.Invoke(resource);
        protected void OnUnloadedCall() => OnUnloaded?.Invoke();
        
        public static implicit operator T(ResourceOnDemand<T> t) => t?.Resource;
        public static implicit operator bool(ResourceOnDemand<T> t) => t != null;
    }
}