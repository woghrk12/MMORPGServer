using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Game;

namespace Server
{
    public class PacketHandler
    {
        public static void HandleCreatureMoveRequest(ClientSession session, IMessage message)
        {
            CreatureMoveRequest packet = message as CreatureMoveRequest;

            Console.WriteLine($"CreatureMoveRequest. Session ID : {session.SessionID} ({packet.PosX}, {packet.PosY}, {packet.MoveDirection})");

            Player player = session.Player;
            if (ReferenceEquals(player, null) == true) return;

            GameRoom room = player.Room;
            if (ReferenceEquals(room, null) == true) return;

            room.MovePlayer(player, packet.MoveDirection);
        }
    }
}