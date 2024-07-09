using System;
using System.Collections.Generic;
using ServerCore;

namespace Server
{
    public class PacketManager
    {
        #region Variables

        private static PacketManager instance = null;

        private Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> receivedPacketHandlerDict = new();
        private Dictionary<ushort, Action<PacketSession, IPacket>> handlerDict = new();

        #endregion Variables

        #region Properties

        public static PacketManager Instance
        {
            get
            {
                if (ReferenceEquals(instance, null))
                {
                    instance = new PacketManager();
                }

                return instance;
            }
        }

        #endregion Properties   

        #region Methods

        public void Register()
        {
            receivedPacketHandlerDict.Add((ushort)EPacketID.CLIENT_CHAT, MakePacket<ClientChat>);
			handlerDict.Add((ushort)EPacketID.CLIENT_CHAT, PacketHandler.HandleClientChat);
			
        }

        private void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
        {
            T packet = new();
            packet.Read(buffer);

            if (handlerDict.TryGetValue(packet.Protocol, out Action<PacketSession, IPacket> action))
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

            if (receivedPacketHandlerDict.TryGetValue(id, out Action<PacketSession, ArraySegment<byte>> action))
            {
                action.Invoke(session, buffer);
            }
        }
    
        #endregion Events

        #endregion Methods
    }
}
