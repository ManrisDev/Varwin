using Unity.Netcode;

namespace Varwin.Multiplayer
{
    /// <summary>
    /// Состояние интерактивности.
    /// </summary>
    public struct NetworkInteractionState : INetworkSerializable
    {
        /// <summary>
        /// Состояние.
        /// </summary>
        public bool State;
        
        /// <summary>
        /// Идентификатор клиента события.
        /// </summary>
        public ulong ClientId;
        
        /// <summary>
        /// Левой ли рукой.
        /// </summary>
        public bool IsLeftHand;

        /// <summary>
        /// Инициализация.
        /// </summary>
        /// <param name="state">Состояние.</param>
        /// <param name="clientId">Идентификатор клиента события.</param>
        /// <param name="isLeftHand">Левой ли рукой.</param>
        public NetworkInteractionState(bool state, ulong clientId, bool isLeftHand)
        {
            State = state;
            ClientId = clientId;
            IsLeftHand = isLeftHand;
        }
        
        /// <summary>
        /// Сериализация параметров.
        /// </summary>
        /// <param name="serializer">Сериализатор.</param>
        /// <typeparam name="T">Тип.</typeparam>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out State);
                reader.ReadValueSafe(out ClientId);
                reader.ReadValueSafe(out IsLeftHand);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(State);
                writer.WriteValueSafe(ClientId);
                writer.WriteValueSafe(IsLeftHand);
            }
        }
    }
}