using Google.Protobuf;
using Google.Protobuf.Protocol;

namespace Server
{
    public class PacketHandler
    {
        public static void HandleCreatureMoveRequest(ClientSession session, IMessage message)
        {
            CreatureMoveRequest packet = message as CreatureMoveRequest;

            Console.WriteLine($"CreatureMoveRequest. Session ID : {session.SessionID} ({packet.PosX}, {packet.PosY}, {packet.MoveDirection})");

            if (ReferenceEquals(session.Player, null) == true) return;
            if (ReferenceEquals(session.Player.Room, null) == true) return;

            // TODO : Verify if the transmitted packet is valid.

            PlayerInfo info = session.Player.Info;
            switch (packet.MoveDirection)
            {
                case EMoveDirection.Up:
                    info.PosY += 1;
                    break;
                case EMoveDirection.Down:
                    info.PosY -= 1;
                    break;
                case EMoveDirection.Left:
                    info.PosX -= 1;
                    break;
                case EMoveDirection.Right:
                    info.PosX += 1;
                    break;
            }

            CreatureMoveBrodcast creatureMoveBrodcastPacket = new();
            creatureMoveBrodcastPacket.CreatureID = session.Player.Info.PlayerID;
            creatureMoveBrodcastPacket.MoveDirection = packet.MoveDirection;
            creatureMoveBrodcastPacket.PosX = info.PosX;
            creatureMoveBrodcastPacket.PosY = info.PosY;

            session.Player.Room.Brodcast(creatureMoveBrodcastPacket);
        }
    }
}