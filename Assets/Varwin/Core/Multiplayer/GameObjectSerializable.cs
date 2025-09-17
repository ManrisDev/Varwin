using Unity.Netcode;
using UnityEngine;

namespace Varwin.Multiplayer
{
    public struct GameObjectSerializable : INetworkSerializable
    {
        private ulong _globalHashId;
        private ulong _objectId;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _objectId);
            serializer.SerializeValue(ref _globalHashId);
        }

        public void SetGameObject(GameObject gameObject)
        {
            var networkObject = gameObject.GetComponent<NetworkObject>();
            if (!networkObject)
            {
                return;
            }
            
            _objectId = networkObject.NetworkObjectId;
            _globalHashId = networkObject.GlobalObjectIdHash;
        }
        
        public GameObject GetGameObject()
        {
            var networkObjects = Object.FindObjectsOfType<NetworkObject>();
            foreach (var networkObject in networkObjects)
            {
                if (networkObject.NetworkObjectId == _objectId && networkObject.GlobalObjectIdHash == _globalHashId)
                {
                    return networkObject.gameObject;
                }
            }

            return null;
        }
    }
}