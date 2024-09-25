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

                map.AddCreature(newPlayer);

                CreatureInfo newPlayerInfo = new()
                {
                    CreatureID = newPlayer.ID,
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

                    CreatureInfo otherPlayerInfo = new()
                    {
                        CreatureID = player.ID,
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

                map.RemoveCreature(leftPlayer);
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

        public void MoveCreature(int creatureID, Vector2Int curCellPos, EMoveDirection moveDirection)
        {
            lock (lockObj)
            {
                if (playerDictionary.TryGetValue(creatureID, out Player player) == false) return;

                if (curCellPos.X != player.Position.X || curCellPos.Y != player.Position.Y) return;

                if (moveDirection == EMoveDirection.None)
                {
                    player.CurState = ECreatureState.Idle;
                    player.MoveDirection = EMoveDirection.None;
                }
                else
                {
                    if (player.CurState != ECreatureState.Move)
                    {
                        player.CurState = ECreatureState.Move;
                    }

                    player.MoveDirection = moveDirection;
                }

                Pos curPos = player.Position;
                map.MoveCreature(player, moveDirection);
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
                    CreatureID = player.ID,
                    MoveDirection = player.MoveDirection,
                    CurPosX = curPos.X,
                    CurPosY = curPos.Y,
                    TargetPosX = targetPos.X,
                    TargetPosY = targetPos.Y
                };

                Brodcast(performMoveBroadcastPacket);
            }
        }

        public void PerformAttack(int creatureID, AttackInfo attackInfo)
        {
            lock (lockObj)
            {
                if (playerDictionary.TryGetValue(creatureID, out Player player) == false) return;

                // TODO : Certify the attack info passed by the packet

                // TODO : Check whether the creature can attack
                if (player.CurState != ECreatureState.Idle) return;

                player.CurState = ECreatureState.Attack;

                long attackStartTime = DateTime.UtcNow.Ticks;

                PerformAttackResponse performAttackResponsePacket = new()
                {
                    AttackStartTime = attackStartTime,
                    AttackInfo = new() { AttackID = 1 }
                };

                player.Session.Send(performAttackResponsePacket);

                PerformAttackBroadcast performAttackBroadcastPacket = new()
                {
                    CreatureID = creatureID,
                    AttackStartTime = attackStartTime,
                    AttackInfo = new() { AttackID = 1 }
                };

                Brodcast(performAttackBroadcastPacket);

                // TODO : Perform the damage calculation
                Pos attackPos = player.GetFrontPos();

                if (map.Find(attackPos, out List<Creature> creatureList) == false) return;

                foreach (Creature creature in creatureList)
                {
                    HitBroadcast hitBroadcastPacket = new()
                    {
                        AttackerID = creatureID,
                        DefenderID = creature.ID
                    };

                    Brodcast(hitBroadcastPacket);
                }
            }
        }

        public void SetCreatureState(int creatureID, ECreatureState state)
        {
            lock (lockObj)
            {
                if (playerDictionary.TryGetValue(creatureID, out Player player) == false) return;

                player.CurState = state;
            }
        }

        #endregion Methods
    }
}