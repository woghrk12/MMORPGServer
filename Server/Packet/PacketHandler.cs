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

            Console.WriteLine($"PerformMoveRequest. Session ID : {session.SessionID} Cell Pos : ({packet.CurPosX}, {packet.CurPosY}) Move Direction : {packet.MoveDirection}");

            Player player = session.Player;
            if (ReferenceEquals(player, null) == true) return;

            GameRoom room = player.Room;
            if (ReferenceEquals(room, null) == true) return;

            room.MoveObject(player.ID, new Vector2Int(packet.CurPosX, packet.CurPosY), packet.MoveDirection);
        }

        public static void HandlePerformAttackRequest(ClientSession session, IMessage message)
        {
            PerformAttackRequest packet = message as PerformAttackRequest;

            Console.WriteLine($"PerformAttackRequest. Session ID : {session.SessionID} Attack ID : {packet.AttackID}");

            Player player = session.Player;
            if (ReferenceEquals(player, null) == true) return;

            GameRoom room = player.Room;
            if (ReferenceEquals(room, null) == true) return;

            room.PerformAttack(player.ID, packet.AttackID);
        }

        public static void HandleObjectReviveRequest(ClientSession session, IMessage message)
        {
            ObjectReviveRequest packet = message as ObjectReviveRequest;

            Console.WriteLine($"ObjectReviveRequest. Session ID : {session.SessionID}, Object ID : {packet.ObjectID}");

            Player player = session.Player;
            if (ReferenceEquals(player, null) == true) return;

            GameRoom room = player.Room;
            if (ReferenceEquals(room, null) == true) return;

            room.ReviveObject(packet.ObjectID, new Pos(0, 0));
        }
    }
}