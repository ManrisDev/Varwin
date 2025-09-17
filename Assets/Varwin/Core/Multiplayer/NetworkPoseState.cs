using Unity.Netcode;
using UnityEngine;

namespace Varwin.Multiplayer
{
    public struct NetworkPoseState : INetworkSerializable
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValue(out Position);
                reader.ReadValue(out Rotation);
            }

            if (serializer.IsWriter)
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValue(Position);
                writer.WriteValue(Rotation);
            }
        }
    }
}