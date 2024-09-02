using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Game;

namespace Server
{
    public class PacketHandler
    {
        public static void HandleInputDirectionRequest(ClientSession session, IMessage message)
        {
            InputDirectionRequest packet = message as InputDirectionRequest;

            Console.WriteLine($"InputDirectionRequest. Session ID : {session.SessionID} ({packet.MoveDirection})");

            Player player = session.Player;
            if (ReferenceEquals(player, null) == true) return;

            GameRoom room = player.Room;
            if (ReferenceEquals(room, null) == true) return;

            room.ModifyDirection(player, packet.MoveDirection);
        }
    }
}