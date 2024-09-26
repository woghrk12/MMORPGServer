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
                playerDictionary.Add(newPlayer.ID, newPlayer);
                newPlayer.Room = this;

                map.AddObject(newPlayer);

                ObjectInfo newPlayerInfo = new()
                {
                    ObjectID = newPlayer.ID,
                    Name = newPlayer.Name,
                    CurState = newPlayer.CurState,
                    PosX = newPlayer.Position.X,
                    PosY = newPlayer.Position.Y,
                    FacingDirection = newPlayer.MoveDirection,
                    MoveSpeed = 5
                };

                // Send the packets to the player who has just enterend the room
                PlayerEnteredRoomResponse playerEnteredRoomResponse = new()
                {
                    NewPlayer = newPlayerInfo
                };

                foreach (Player player in playerDictionary.Values)
                {
                    if (newPlayer.ID == player.ID) continue;

                    ObjectInfo otherPlayerInfo = new()
                    {
                        ObjectID = player.ID,
                        Name = player.Name,
                        CurState = player.CurState,
                        PosX = player.Position.X,
                        PosY = player.Position.Y,
                        FacingDirection = player.MoveDirection,
                        MoveSpeed = 5
                    };

                    playerEnteredRoomResponse.OtherPlayers.Add(otherPlayerInfo);
                }

                newPlayer.Session.Send(playerEnteredRoomResponse);

                // Send the packets to the players who are in the room
                PlayerEnteredRoomBroadcast playerEnteredRoomBroadcast = new()
                {
                    NewPlayer = newPlayerInfo
                };

                foreach (Player player in playerDictionary.Values)
                {
                    if (newPlayer.ID == player.ID) continue;

                    player.Session.Send(playerEnteredRoomBroadcast);
                }
            }
        }

        public void LeaveRoom(int playerID)
        {
            lock (lockObj)
            {
                if (playerDictionary.Remove(playerID, out Player leftPlayer) == false) return;

                map.RemoveObject(leftPlayer);
                leftPlayer.Room = null;

                {
                    PlayerLeftRoomResponse packet = new();

                    leftPlayer.Session.Send(packet);
                }

                {
                    PlayerLeftRoomBroadcast packet = new()
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

        public void Broadcast(IMessage packet)
        {
            lock (lockObj)
            {
                foreach (Player player in playerDictionary.Values)
                {
                    player.Session.Send(packet);
                }
            }
        }

        public void MoveObject(int objectID, Vector2Int curCellPos, EMoveDirection moveDirection)
        {
            lock (lockObj)
            {
                if (playerDictionary.TryGetValue(objectID, out Player player) == false) return;

                if (curCellPos.X != player.Position.X || curCellPos.Y != player.Position.Y) return;

                if (moveDirection == EMoveDirection.None)
                {
                    player.CurState = EObjectState.Idle;
                    player.MoveDirection = EMoveDirection.None;
                }
                else
                {
                    if (player.CurState != EObjectState.Move)
                    {
                        player.CurState = EObjectState.Move;
                    }

                    player.MoveDirection = moveDirection;
                }

                Pos curPos = player.Position;
                map.MoveObject(player, moveDirection);
                Pos targetPos = player.Position;

                PerformMoveResponse performMoveResponsePacket = new()
                {
                    CurPosX = curPos.X,
                    CurPosY = curPos.Y,
                    TargetPosX = targetPos.X,
                    TargetPosY = targetPos.Y
                };

                player.Session.Send(performMoveResponsePacket);

                PerformMoveBroadcast performMoveBroadcastPacket = new()
                {
                    ObjectID = player.ID,
                    MoveDirection = player.MoveDirection,
                    CurPosX = curPos.X,
                    CurPosY = curPos.Y,
                    TargetPosX = targetPos.X,
                    TargetPosY = targetPos.Y
                };

                Broadcast(performMoveBroadcastPacket);
            }
        }

        public void PerformAttack(int objectID, AttackInfo attackInfo)
        {
            lock (lockObj)
            {
                if (playerDictionary.TryGetValue(objectID, out Player player) == false) return;

                // TODO : Certify the attack info passed by the packet

                // TODO : Check whether the gameobject can attack
                if (player.CurState != EObjectState.Idle) return;

                player.CurState = EObjectState.Attack;

                long attackStartTime = DateTime.UtcNow.Ticks;

                PerformAttackResponse performAttackResponsePacket = new()
                {
                    AttackStartTime = attackStartTime,
                    AttackInfo = new() { AttackID = 1 }
                };

                player.Session.Send(performAttackResponsePacket);

                PerformAttackBroadcast performAttackBroadcastPacket = new()
                {
                    ObjectID = objectID,
                    AttackStartTime = attackStartTime,
                    AttackInfo = new() { AttackID = 1 }
                };

                Broadcast(performAttackBroadcastPacket);

                // TODO : Perform the damage calculation
                Pos attackPos = player.GetFrontPos();

                if (map.Find(attackPos, out List<GameObject> objectList) == false) return;

                foreach (GameObject gameObject in objectList)
                {
                    HitBroadcast hitBroadcastPacket = new()
                    {
                        AttackerID = objectID,
                        DefenderID = gameObject.ID
                    };

                    Broadcast(hitBroadcastPacket);
                }
            }
        }

        public void SetObjectState(int objectID, EObjectState state)
        {
            lock (lockObj)
            {
                if (playerDictionary.TryGetValue(objectID, out Player player) == false) return;

                player.CurState = state;
            }
        }

        #endregion Methods
    }
}