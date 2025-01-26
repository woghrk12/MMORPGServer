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

            Console.WriteLine($"CharacterEnterGameRoomRequest. Name : {packet.CharacterID}");

            session.EnterGameRoom(packet.CharacterID);
        }

        public static void HandleMoveRequest(ClientSession session, IMessage message)
        {
            MoveRequest packet = message as MoveRequest;

            Console.WriteLine($"MoveRequest. Session ID : {session.SessionID} Target Pos : ({packet.TargetPosX}, {packet.TargetPosY}) Move Direction : {packet.MoveDirection}");

            Character character = session.Character;
            if (ReferenceEquals(character, null) == true) return;

            GameRoom room = character.Room;
            if (ReferenceEquals(room, null) == true) return;

            room.MoveCharacter(character.ID, packet.MoveDirection, new Pos(packet.TargetPosX, packet.TargetPosY));
        }

        public static void HandlePerformAttackRequest(ClientSession session, IMessage message)
        {
            PerformAttackRequest packet = message as PerformAttackRequest;

            Console.WriteLine($"PerformAttackRequest. Session ID : {session.SessionID} Attack ID : {packet.AttackID}");

            Character character = session.Character;
            if (ReferenceEquals(character, null) == true) return;

            GameRoom room = character.Room;
            if (ReferenceEquals(room, null) == true) return;

            room.PerformCharacterAttack(character.ID, packet.AttackID);
        }

        public static void HandleCharacterReviveRequest(ClientSession session, IMessage message)
        {
            CharacterReviveRequest packet = message as CharacterReviveRequest;

            Console.WriteLine($"ObjectReviveRequest. Session ID : {session.SessionID}, Object ID : {packet.CharacterID}");

            Character character = session.Character;
            if (ReferenceEquals(character, null) == true) return;

            GameRoom room = character.Room;
            if (ReferenceEquals(room, null) == true) return;

            room.ReviveCreature(packet.CharacterID, new Pos(0, 0));
        }
    }
}