using System;
using System.Linq;
using UnityEngine;
using Varwin.Data.ServerData;

namespace Varwin.Public
{
    public class VirtualObject : MonoBehaviour
    {
        public LocalizedDictionary<string> Name;
        
        private VarwinObjectDescriptor _varwinObjectDescriptor;
        private Rigidbody _rigidbody;
        public ObjectController ObjectController => _varwinObjectDescriptor.ObjectController;
        public int IdObject => GetComponent<ObjectId>().Id;

        private void OnValidate()
        {
            var objectId = GetComponent<ObjectId>();

            if (objectId)
            {
                return;
            }
            
            objectId = gameObject.AddComponent<ObjectId>();
            objectId.Id = gameObject.GetInstanceID();
        }

        public void Initialize(ObjectController targetController, VirtualObjectInfo virtualObjectInfo)
        {
            var initObjectParams = new InitObjectParams()
            {
                Id = virtualObjectInfo?.InstanceId ?? GetUniqueId(),
                Asset = gameObject,
                RootGameObject = gameObject,
                WrappersCollection = new WrappersCollection(),
                IdScene = ProjectData.SceneId,
                LocalizedNames = Name.ToI18n(),
                IdObject = IdObject,
                IsVirtualObject = true,
#if VARWINCLIENT
                Name = Name.GetValue(Settings.Instance?.Language ?? "en"),
#endif
                LockChildren = virtualObjectInfo?.LockChildren ?? false,
            };

            if (!gameObject.TryGetComponent(out _rigidbody))
            {
                _rigidbody = gameObject.AddComponent<Rigidbody>();
                _rigidbody.isKinematic = true;
            }

            _varwinObjectDescriptor = gameObject.AddComponent<VarwinObjectDescriptor>();
            _varwinObjectDescriptor.RootGuid = Guid.NewGuid().ToString();
            _varwinObjectDescriptor.Guid = _varwinObjectDescriptor.RootGuid;
            _varwinObjectDescriptor.InitObjectController(new ObjectController(initObjectParams));
            _varwinObjectDescriptor.ObjectController.SetParent(targetController);
        }

        private int GetMaxId(SceneObjectDto sceneObjectDto)
        {
            var maxId = sceneObjectDto.InstanceId;

            if (sceneObjectDto.SceneObjects == null)
            {
                return maxId;
            }
            
            foreach (var child in sceneObjectDto.SceneObjects)
            {
                if (child.Data?.VirtualObjectsInfos != null)
                {
                    maxId = child.Data.VirtualObjectsInfos.Select(virtualObjectInfo => virtualObjectInfo.InstanceId).Prepend(maxId).Max();
                }

                var childMaxId = GetMaxId(child);

                if (childMaxId > maxId)
                {
                    maxId = childMaxId;
                }
            }

            return maxId;
        }
        
        private int GetUniqueId()
        {
            if (ProjectData.CurrentScene?.SceneObjects == null)
            {
                return 0;
            }
            
            var maxId = -1;
            foreach (var sceneObjectDto in ProjectData.CurrentScene.SceneObjects)
            {
                var newMaxId = GetMaxId(sceneObjectDto);
                if (newMaxId > maxId)
                {
                    maxId = newMaxId;
                }
            }

            var currentMax = GameStateData.GetObjectIds().Max();
            return currentMax > maxId ? currentMax + 1 : maxId + 1;
        }
        
        private void OnDestroy()
        {
            if (_varwinObjectDescriptor)
            {
                _varwinObjectDescriptor.ObjectController?.Delete();
            }

            Destroy(_varwinObjectDescriptor);
        }
    }
}