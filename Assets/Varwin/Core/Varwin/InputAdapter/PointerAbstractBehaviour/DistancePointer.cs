using System;
using UnityEngine;

namespace Varwin.PlatformAdapter
{
    public abstract class DistancePointer : MonoBehaviour, IBasePointer
    {
        public static event Action SettingsUpdated;

        [SerializeField] private DistancePointerSettings _settings;

        private static DistancePointerSettings _customSettings;

        public abstract float RaycastDistance { get; }

        public static DistancePointerSettings CustomSettings
        {
            get => _customSettings;
            set
            {
                _customSettings = value;
                SettingsUpdated?.Invoke();
            }
        }

        protected DistancePointerSettings Settings => CustomSettings ? CustomSettings : _settings;

        protected DistancePointerSettings DefaultSettings => _settings;
        
        protected Transform Origin { get; private set; }

        public abstract bool CanToggle();

        public abstract bool CanPress();

        public abstract bool CanRelease();
        
        public abstract void Toggle(bool value);
        
        public abstract void Toggle();
        
        public abstract void Press();
        
        public abstract void Release();
        
        public abstract void UpdateState();

        public abstract bool IsActive();

        public void Init()
        {           
            var origins = transform.Find("PointerOrigins");
            
            if (origins)
            {
                Origin = origins.Find(DeviceHelper.IsOculus ? "Oculus" : "Generic");
            }

            if (!Origin)
            {
                Origin = transform;
            }

            OnInit();

            SettingsUpdated -= OnInit;
            SettingsUpdated += OnInit;
        }

        private void OnDestroy()
        {
            SettingsUpdated -= OnInit;
        }

        protected abstract void OnInit();
    }
}