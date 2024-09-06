using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Game;

namespace Server
{
    public class PacketHandler
    {
        public static void HandlePerformMoveRequest(ClientSession session, IMessage message)
        {
            PerformMoveRequest packet = message as PerformMoveRequest;

            Console.WriteLine($"PerformMoveRequest. Session ID : {session.SessionID} Cell Pos : ({packet.CurCellPosX}, {packet.CurCellPosY}) Move Direction : {packet.MoveDirection}");

            Player player = session.Player;
            if (ReferenceEquals(player, null) == true) return;

            GameRoom room = player.Room;
            if (ReferenceEquals(room, null) == true) return;

            room.MoveCreature(player.ID, (packet.CurCellPosX, packet.CurCellPosY), packet.MoveDirection);
        }
    }
}