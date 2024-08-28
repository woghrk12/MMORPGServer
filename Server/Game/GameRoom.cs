using Google.Protobuf;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class GameRoom
    {
        #region Variables

        private object lockObj = new();

        private Map map = new();

        private Dictionary<int, Player> playerDictionary = new();

        #endregion Variables

        #region Properties

        public int RoomID { set; get; }

        #endregion Properties

        #region Constructor

        public GameRoom(int mapID)
        {
            map.LoadMap(mapID);
        }

        #endregion Constructor

        #region Methods

        public void EnterRoom(Player newPlayer)
        {
            if (ReferenceEquals(newPlayer, null) == true) return;

            lock (lockObj)
            {
                playerDictionary.Add(newPlayer.Info.PlayerID, newPlayer);
                newPlayer.Room = this;

                {
                    PlayerEnteredRoomResponse packet = new();

                    packet.MyInfo = newPlayer.Info;
                    foreach (Player player in playerDictionary.Values)
                    {
                        if (newPlayer.Info.PlayerID == player.Info.PlayerID) continue;

                        packet.OtherPlayers.Add(player.Info);
                    }

                    newPlayer.Session.Send(packet);
                }

                {
                    PlayerEnteredRoomBrodcast packet = new();

                    packet.NewPlayer = newPlayer.Info;

                    foreach (Player player in playerDictionary.Values)
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
                if (playerDictionary.Remove(playerID, out Player leftPlayer) == false) return;

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

                    foreach (Player player in playerDictionary.Values)
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
                foreach (Player player in playerDictionary.Values)
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
                UpdatePlayerState(info, ECreatureState.Move);
            }
        }

        private void UpdatePlayerState(PlayerInfo info, ECreatureState state)
        {
            if (info.State == state) return;

            info.State = state;

            UpdateCreatureStateBroadcast packet = new()
            {
                CreatureID = info.PlayerID,
                State = state
            };

            Brodcast(packet);
        }

        #endregion Methods
    }
}