using System.Collections.Generic;
using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin.DesktopInput
{
    public class DesktopPointerControllerComponent : PointerControllerComponent
    {
        [SerializeField]
        private GameObject _teleportGameObject;
            
        protected override List<IBasePointer> _pointers { get; set; }

        private void Awake()
        {
            TeleportPointer teleport = _teleportGameObject.AddComponent<TeleportPointer>();
            _pointers = new List<IBasePointer> {teleport};

            foreach (IBasePointer pointer in _pointers)
            {
                pointer.Init();
            }
        }

        private void Update()
        {
            UpdatePointers();
        }
    }
}
