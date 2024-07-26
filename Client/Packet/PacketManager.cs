using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;

using ServerCore;

namespace Client
{
    public class PacketManager
    {
        #region Variables

        private static PacketManager instance = new();

        private Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> receivedPacketHandlerDict = new();
        private Dictionary<ushort, Action<PacketSession, IMessage>> handlerDict = new();

        #endregion Variables

        #region Properties

        public static PacketManager Instance => instance;

        #endregion Properties

        #region Constructor

        public PacketManager()
        {
            throw new NotImplementedException();

            // Implement the constructor to register methods for handling the packets that the client needs.
            // Example
            // {0} : The class name of the packet.
            // receivedPacketHandlerDict.Add((ushort)EMessageID.{0}}, MakePacket<{0}}>);
            // handlerDict.Add((ushort)EMessageID.{0}}, PacketHandler.Handle{0}});
        }

        #endregion Constructor

        #region Methods

        public Action<PacketSession, IMessage> GetPacketHandler(ushort id)
        {
            return handlerDict.TryGetValue(id, out Action<PacketSession, IMessage> action) == true ? action : null;
        }

        private void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
        {
            T packet = new();
            packet.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

            if (handlerDict.TryGetValue(id, out Action<PacketSession, IMessage> action))
            {
                action.Invoke(session, packet);
            }
        }

        #region Events

        public void OnReceivePacket(PacketSession session, ArraySegment<byte> buffer)
        {
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            if (receivedPacketHandlerDict.TryGetValue(id, out Action<PacketSession, ArraySegment<byte>, ushort> action))
            {
                action.Invoke(session, buffer, id);
            }
        }

        #endregion Events

        #endregion Methods
    }
}