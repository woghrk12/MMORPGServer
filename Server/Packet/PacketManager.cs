using Google.Protobuf;
using Google.Protobuf.Protocol;

namespace Server
{
    public class PacketManager
    {
        #region Variables

        private static PacketManager instance = new();

        private Dictionary<ushort, Action<ClientSession, ArraySegment<byte>, ushort>> receivedPacketHandlerDict = new();
        private Dictionary<ushort, Action<ClientSession, IMessage>> handlerDict = new();

        #endregion Variables

        #region Properties

        public static PacketManager Instance => instance;

        #endregion Properties

        #region Constructor

        public PacketManager()
        {
            receivedPacketHandlerDict.Add((ushort)EMessageID.InputDirectionRequest, MakePacket<InputDirectionRequest>);

            handlerDict.Add((ushort)EMessageID.InputDirectionRequest, PacketHandler.HandleInputDirectionRequest);
        }

        #endregion Constructor

        #region Methods

        public Action<ClientSession, IMessage> GetPacketHandler(ushort id)
        {
            return handlerDict.TryGetValue(id, out Action<ClientSession, IMessage> action) == true ? action : null;
        }

        private void MakePacket<T>(ClientSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
        {
            T packet = new();
            packet.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

            if (handlerDict.TryGetValue(id, out Action<ClientSession, IMessage> action))
            {
                action.Invoke(session, packet);
            }
        }

        #region Events

        public void OnReceivePacket(ClientSession session, ArraySegment<byte> buffer)
        {
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            if (receivedPacketHandlerDict.TryGetValue(id, out Action<ClientSession, ArraySegment<byte>, ushort> action))
            {
                action.Invoke(session, buffer, id);
            }
        }

        #endregion Events

        #endregion Methods
    }
}