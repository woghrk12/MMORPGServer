using Google.Protobuf;
using Google.Protobuf.Protocol;

namespace Server
{
    public class PacketHandler
    {
        public static void HandleCreatureMoveRequest(ClientSession session, IMessage message)
        {
            CreatureMoveRequest packet = message as CreatureMoveRequest;
        }
    }
}