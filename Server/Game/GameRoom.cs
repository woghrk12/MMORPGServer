using Google.Protobuf;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class GameRoom
    {
        #region Variables

        private object lockObj = new();

        private List<Player> playerList = new();

        #endregion Variables

        #region Properties

        public int RoomID { set; get; }

        #endregion Properties

        #region Methods

        public void EnterRoom(Player newPlayer)
        {
            if (ReferenceEquals(newPlayer, null) == true) return;

            lock (lockObj)
            {
                playerList.Add(newPlayer);
                newPlayer.Room = this;

                {
                    PlayerEnteredRoomResponse packet = new();

                    packet.MyInfo = newPlayer.Info;
                    foreach (Player player in playerList)
                    {
                        if (newPlayer.Info.PlayerID == player.Info.PlayerID) continue;

                        packet.OtherPlayers.Add(player.Info);
                    }

                    newPlayer.Session.Send(packet);
                }

                {
                    PlayerEnteredRoomBrodcast packet = new();

                    packet.NewPlayer = newPlayer.Info;

                    foreach (Player player in playerList)
                    {
                        if (newPlayer.Info.PlayerID == player.Info.PlayerID) continue;

                        player.Session.Send(packet);
                    }
                }
            }
        }

        public void LeaveRoom(int playerID)
        {
            lock (lockObj)
            {
                Player leftPlayer = playerList.Find(player => player.Info.PlayerID == playerID);

                if (ReferenceEquals(leftPlayer, null) == true) return;

                playerList.Remove(leftPlayer);
                leftPlayer.Room = null;

                {
                    PlayerLeftRoomResponse packet = new();

                    leftPlayer.Session.Send(packet);
                }

                {
                    PlayerLeftRoomBrodcast packet = new()
                    {
                        OtherPlayerID = leftPlayer.Info.PlayerID
                    };

                    foreach (Player player in playerList)
                    {
                        player.Session.Send(packet);
                    }
                }
            }
        }

        public void Brodcast(IMessage packet)
        {
            lock (lockObj)
            {
                foreach (Player player in playerList)
                {
                    player.Session.Send(packet);
                }
            }
        }

        public void MovePlayer(Player player, EMoveDirection moveDirection)
        {
            if (ReferenceEquals(player, null) == true) return;

            lock (lockObj)
            {
                // TODO : Verify if the transmitted packet is valid.

                PlayerInfo info = player.Info;
                switch (moveDirection)
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

                CreatureMoveBrodcast creatureMoveBrodcastPacket = new()
                {
                    CreatureID = player.Info.PlayerID,
                    MoveDirection = moveDirection,
                    PosX = info.PosX,
                    PosY = info.PosY
                };

                Brodcast(creatureMoveBrodcastPacket);
            }
        }

        #endregion Methods
    }
}