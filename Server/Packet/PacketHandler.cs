using Google.Protobuf;
using Google.Protobuf.Protocol;

namespace Server
{
    public class PacketHandler
    {
        public static void HandlePlayerMoveRequest(ClientSession session, IMessage message)
        {
            PlayerMoveRequest packet = message as PlayerMoveRequest;

            Console.WriteLine(packet.Direction);
        }
    }
}