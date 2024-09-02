using Google.Protobuf;
using Google.Protobuf.Collections;
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
                playerDictionary.Add(newPlayer.ID, newPlayer);
                newPlayer.Room = this;

                CreatureInfo newPlayerInfo = new()
                {
                    CreatureID = newPlayer.ID,
                    Name = newPlayer.Name,
                    CurState = newPlayer.CurState,
                    CellPosX = newPlayer.CellPos.X,
                    CellPosY = newPlayer.CellPos.Y,
                    FacingDirection = newPlayer.FacingDirection
                };

                // Send the packets to the player who has just enterend the room
                PlayerEnteredRoomResponse playerEnteredRoomResponse = new()
                {
                    NewPlayer = newPlayerInfo
                };

                foreach (Player player in playerDictionary.Values)
                {
                    if (newPlayer.ID == player.ID) continue;

                    CreatureInfo otherPlayerInfo = new()
                    {
                        CreatureID = player.ID,
                        Name = player.Name,
                        CurState = player.CurState,
                        CellPosX = player.CellPos.X,
                        CellPosY = player.CellPos.Y,
                        FacingDirection = player.FacingDirection
                    };

                    playerEnteredRoomResponse.OtherPlayers.Add(otherPlayerInfo);
                }

                newPlayer.Session.Send(playerEnteredRoomResponse);

                // Send the packets to the players who are in the room
                PlayerEnteredRoomBrodcast playerEnteredRoomBrodcast = new()
                {
                    NewPlayer = newPlayerInfo
                };

                foreach (Player player in playerDictionary.Values)
                {
                    if (newPlayer.ID == player.ID) continue;

                    PlayerEnteredRoomBrodcast packet = new()
                    {
                        NewPlayer = newPlayerInfo
                    };

                    player.Session.Send(playerEnteredRoomBrodcast);
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
                        OtherPlayerID = leftPlayer.ID
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

        public void ModifyDirection(Player player, EMoveDirection moveDirection)
        {
            if (ReferenceEquals(player, null) == true) return;

            lock (lockObj)
            {
                player.InputDirection = moveDirection;
            }
        }

        public void MoveCreature(int creatureID, EMoveDirection moveDirection)
        {
            if (playerDictionary.TryGetValue(creatureID, out Player player) == false) return;

            Vector2Int cellPos = player.CellPos;

            switch (moveDirection)
            {
                case EMoveDirection.Up:
                    cellPos += Vector2Int.Up;
                    break;

                case EMoveDirection.Down:
                    cellPos += Vector2Int.Down;
                    break;

                case EMoveDirection.Left:
                    cellPos += Vector2Int.Left;
                    break;

                case EMoveDirection.Right:
                    cellPos += Vector2Int.Right;
                    break;

                default:
                    player.CurState = ECreatureState.Idle;
                    return;
            }

            if (map.CheckCanMove(cellPos, true) == false) return;

            player.CellPos = cellPos;
        }

        #region Events

        public void OnUpdate(float deltaTime)
        {
            lock (lockObj)
            {
                foreach (Player player in playerDictionary.Values)
                {
                    player.OnUpdate(deltaTime);
                }
            }
        }

        #endregion Events

        #endregion Methods
    }
}