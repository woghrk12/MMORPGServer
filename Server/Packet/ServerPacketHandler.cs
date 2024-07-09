using System;
using ServerCore;

namespace Server
{
    public class PacketHandler
    {
        public static void HandleClientChat(PacketSession session, IPacket packet)
        {
            ClientChat chatPacket = packet as ClientChat;
            ClientSession clientSession = session as ClientSession;

            if (ReferenceEquals(clientSession.Room, null) == true) return;

        }
    }
}
