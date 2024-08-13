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
                    PlayerLeftRoomBrodcast packet = new();

                    packet.OtherPlayerID = leftPlayer.Info.PlayerID;

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

        #endregion Methods
    }
}