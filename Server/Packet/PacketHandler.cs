using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Game;

namespace Server
{
    public class PacketHandler
    {
        public static void HandleLoginRequest(ClientSession session, IMessage message)
        {
            LoginRequest packet = message as LoginRequest;

            Console.WriteLine($"LoginRequest. ID : {packet.Id}");

            session.Login(packet.Id);
        }

        public static void HandleCreateCharacterRequest(ClientSession session, IMessage message)
        {
            CreateCharacterRequest packet = message as CreateCharacterRequest;

            Console.WriteLine($"CreateCharacterRequest. Name : {packet.Name}");

            session.CreateCharacter(packet.Name);
        }

        public static void HandleCharacterEnterGameRoomRequest(ClientSession session, IMessage message)
        {
            CharacterEnterGameRoomRequest packet = message as CharacterEnterGameRoomRequest;

            Console.WriteLine($"CharacterEnterGameRoomRequest. Name : {packet.Name}");

            session.EnterGameRoom(packet.Name);
        }

        public static void HandlePerformMoveRequest(ClientSession session, IMessage message)
        {
            PerformMoveRequest packet = message as PerformMoveRequest;

            Console.WriteLine($"PerformMoveRequest. Session ID : {session.SessionID} Cur Pos : ({packet.CurPosX}, {packet.CurPosY}) Target Pos : ({packet.TargetPosX}, {packet.TargetPosY}) Move Direction : {packet.MoveDirection}");

            Character character = session.Character;
            if (ReferenceEquals(character, null) == true) return;

            GameRoom room = character.Room;
            if (ReferenceEquals(room, null) == true) return;

            room.PerformMove(character.ID, new Pos(packet.CurPosX, packet.CurPosY), packet.MoveDirection);
        }

        public static void HandlePerformAttackRequest(ClientSession session, IMessage message)
        {
            PerformAttackRequest packet = message as PerformAttackRequest;

            Console.WriteLine($"PerformAttackRequest. Session ID : {session.SessionID} Attack ID : {packet.AttackID}");

            Character character = session.Character;
            if (ReferenceEquals(character, null) == true) return;

            GameRoom room = character.Room;
            if (ReferenceEquals(room, null) == true) return;

            room.PerformAttack(character.ID, packet.AttackID);
        }

        public static void HandleObjectReviveRequest(ClientSession session, IMessage message)
        {
            ObjectReviveRequest packet = message as ObjectReviveRequest;

            Console.WriteLine($"ObjectReviveRequest. Session ID : {session.SessionID}, Object ID : {packet.ObjectID}");

            Character character = session.Character;
            if (ReferenceEquals(character, null) == true) return;

            GameRoom room = character.Room;
            if (ReferenceEquals(room, null) == true) return;

            room.ReviveCreature(packet.ObjectID, new Pos(0, 0));
        }
    }
}