using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;

using ServerCore;

namespace Server
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
            receivedPacketHandlerDict.Add((ushort)EMessageID.PlayerMoveRequest, MakePacket<PlayerMoveRequest>);
            handlerDict.Add((ushort)EMessageID.PlayerMoveRequest, PacketHandler.HandlePlayerMoveRequest);
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