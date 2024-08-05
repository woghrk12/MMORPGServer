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
                    PlayerEnterBrodcast packet = new();

                    packet.Player = newPlayer.Info;

                    newPlayer.Session.Send(packet);
                }

                {
                    CreatureSpawnBrodcast packet = new();

                    foreach (Player player in playerList)
                    {
                        if (newPlayer.Info.PlayerID == player.Info.PlayerID) continue;

                        packet.Players.Add(player.Info);
                    }

                    newPlayer.Session.Send(packet);
                }

                {
                    CreatureSpawnBrodcast packet = new();

                    packet.Players.Add(newPlayer.Info);

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
                    PlayerLeaveBrodcast packet = new();

                    leftPlayer.Session.Send(packet);
                }

                {
                    CreatureDespawnBrodcast packet = new();

                    packet.PlayerIDs.Add(leftPlayer.Info.PlayerID);

                    foreach (Player player in playerList)
                    {
                        player.Session.Send(packet);
                    }
                }
            }
        }

        #endregion Methods
    }
}