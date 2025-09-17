using System;
using UnityEngine;

namespace Varwin.Public
{
    public class SpawnPoint : MonoBehaviour, ISwitchPlatformModeSubscriber, ISwitchModeSubscriber
    {
        public static event Action<SpawnPoint> DefaultSpawnPointSpawned;
        
        [SerializeField] private GameObject _render;
        [SerializeField] private Collider _collider;

        public bool IsDefault;
        
        private void Start()
        {
            Setup();
            
            if (IsDefault)
            {
                DefaultSpawnPointSpawned?.Invoke(this);
            }
        }

        public void OnSwitchPlatformMode(PlatformMode newPlatformMode, PlatformMode oldPlatformMode)
        {
            Setup();
        }

        public void OnSwitchMode(GameMode newMode, GameMode oldMode)
        {
            Setup();
            if (IsDefault)
            {
                DefaultSpawnPointSpawned?.Invoke(this);
            }
        }

        private void Setup()
        {
            bool active = ProjectData.GameMode == GameMode.Edit;
            _render.SetActive(active);
            _collider.enabled = active;
        }
    }
}