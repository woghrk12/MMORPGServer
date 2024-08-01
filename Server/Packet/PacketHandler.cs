using Google.Protobuf;
using Google.Protobuf.Protocol;

using ServerCore;

namespace Server
{
    public class PacketHandler
    {
        public static void HandlePlayerMoveRequest(PacketSession session, IMessage message)
        {
            PlayerMoveRequest packet = message as PlayerMoveRequest;
            ClientSession clientSession = session as ClientSession;

            Console.WriteLine(packet.Direction);
        }
    }
}