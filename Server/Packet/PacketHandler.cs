using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.DB;
using Server.Game;

namespace Server
{
    public class PacketHandler
    {
        public static void HandleLoginRequest(ClientSession session, IMessage message)
        {
            LoginRequest packet = message as LoginRequest;

            Console.WriteLine($"LoginRequest. ID : {packet.Id}");

            // TODO : Security check

            using (AppDBContext db = new AppDBContext())
            {
                AccountDB account = db.Accounts
                    .Where(a => a.Name == packet.Id).FirstOrDefault();

                if (ReferenceEquals(account, null) == true)
                {
                    LoginResponse loginResponsePacket = new() { ResultCode = 1 };
                    session.Send(loginResponsePacket);
                }
                else
                {
                    db.Accounts.Add(new AccountDB() { Name = packet.Id });
                    db.SaveChanges();

                    LoginResponse loginResponsePacket = new() { ResultCode = 1 };
                    session.Send(loginResponsePacket);
                }
            }
        }

        public static void HandlePerformMoveRequest(ClientSession session, IMessage message)
        {
            PerformMoveRequest packet = message as PerformMoveRequest;

            Console.WriteLine($"PerformMoveRequest. Session ID : {session.SessionID} Cur Pos : ({packet.CurPosX}, {packet.CurPosY}) Target Pos : ({packet.TargetPosX}, {packet.TargetPosY}) Move Direction : {packet.MoveDirection}");

            Player player = session.Player;
            if (ReferenceEquals(player, null) == true) return;

            GameRoom room = player.Room;
            if (ReferenceEquals(room, null) == true) return;

            room.PerformMove(player.ID, new Pos(packet.CurPosX, packet.CurPosY), packet.MoveDirection);
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