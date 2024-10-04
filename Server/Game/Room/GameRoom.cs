using Google.Protobuf;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class GameRoom
    {
        #region Variables

        private object lockObj = new();

        private Dictionary<EGameObjectType, Dictionary<int, GameObject>> objectDictionary = new();

        #endregion Variables

        #region Properties

        public int RoomID { set; get; }

        public Map Map { private set; get; } = new();

        public Dictionary<int, GameObject> PlayerDictionary => objectDictionary[EGameObjectType.Player];
        public Dictionary<int, GameObject> MonsterDictionary => objectDictionary[EGameObjectType.Monster];
        public Dictionary<int, GameObject> ProjectileDictionary => objectDictionary[EGameObjectType.Projectile];

        #endregion Properties

        #region Constructor

        public GameRoom(int mapID)
        {
            Map.LoadMap(mapID);

            Array types = Enum.GetValues(typeof(EGameObjectType));
            foreach (EGameObjectType type in types)
            {
                objectDictionary.Add(type, new Dictionary<int, GameObject>());
            }
        }

        #endregion Constructor

        #region Methods

        public void Update()
        {
            lock (lockObj)
            {
                foreach (Dictionary<int, GameObject> dictionary in objectDictionary.Values)
                {
                    foreach (GameObject gameObject in dictionary.Values)
                    {
                        gameObject.OnUpdate();
                    }
                }
            }
        }

        public void AddObject(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, null) == true) return;

            lock (lockObj)
            {
                objectDictionary[gameObject.ObjectType].Add(gameObject.ID, gameObject);
                gameObject.Room = this;

                Map.AddObject(gameObject);

                ObjectInfo newObjectInfo = new()
                {
                    ObjectID = gameObject.ID,
                    Name = gameObject.Name,
                    CurState = gameObject.CurState,
                    PosX = gameObject.Position.X,
                    PosY = gameObject.Position.Y,
                    FacingDirection = gameObject.FacingDirection,
                    MoveSpeed = gameObject.MoveSpeed,
                    IsCollidable = gameObject.IsCollidable
                };

                ObjectSpawnedBroadcast packet = new()
                {
                    NewObjectInfo = newObjectInfo
                };

                Broadcast(packet);
            }
        }

        public void RemoveObject(int oldObjectID)
        {
            lock (lockObj)
            {
                EGameObjectType type = ObjectManager.GetObjectTypeByID(oldObjectID);

                if (objectDictionary.TryGetValue(type, out Dictionary<int, GameObject> dictionary) == false) return;
                if (dictionary.TryGetValue(oldObjectID, out GameObject gameObject) == false) return;

                Map.RemoveObject(gameObject);
                gameObject.Room = null;

                ObjectDespawnedBroadcast packet = new()
                {
                    OldObjectID = oldObjectID
                };

                Broadcast(packet);
            }
        }

        public void EnterRoom(Player newPlayer)
        {
            if (ReferenceEquals(newPlayer, null) == true) return;

            lock (lockObj)
            {
                PlayerDictionary.Add(newPlayer.ID, newPlayer);
                newPlayer.Room = this;

                Map.AddObject(newPlayer);

                ObjectInfo newPlayerInfo = new()
                {
                    ObjectID = newPlayer.ID,
                    Name = newPlayer.Name,
                    CurState = newPlayer.CurState,
                    PosX = newPlayer.Position.X,
                    PosY = newPlayer.Position.Y,
                    MoveDirection = newPlayer.MoveDirection,
                    FacingDirection = newPlayer.FacingDirection,
                    MoveSpeed = 5,
                    IsCollidable = newPlayer.IsCollidable
                };

                // Send the packets to the player who has just enterend the room
                PlayerEnteredRoomResponse playerEnteredRoomResponse = new()
                {
                    NewPlayer = newPlayerInfo
                };

                foreach (Player player in PlayerDictionary.Values)
                {
                    if (newPlayer.ID == player.ID) continue;

                    ObjectInfo otherPlayerInfo = new()
                    {
                        ObjectID = player.ID,
                        Name = player.Name,
                        CurState = player.CurState,
                        PosX = player.Position.X,
                        PosY = player.Position.Y,
                        MoveDirection = player.MoveDirection,
                        FacingDirection = player.FacingDirection,
                        MoveSpeed = 5,
                        IsCollidable = player.IsCollidable
                    };

                    playerEnteredRoomResponse.OtherPlayers.Add(otherPlayerInfo);
                }

                newPlayer.Session.Send(playerEnteredRoomResponse);

                // Send the packets to the players who are in the room
                PlayerEnteredRoomBroadcast playerEnteredRoomBroadcast = new()
                {
                    NewPlayer = newPlayerInfo
                };

                foreach (Player player in PlayerDictionary.Values)
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
                if (PlayerDictionary.Remove(playerID, out GameObject leftPlayerObject) == false) return;

                Map.RemoveObject(leftPlayerObject);
                leftPlayerObject.Room = null;

                {
                    PlayerLeftRoomResponse packet = new();

                    (leftPlayerObject as Player).Session.Send(packet);
                }

                {
                    PlayerLeftRoomBroadcast packet = new()
                    {
                        OtherPlayerID = leftPlayerObject.ID
                    };

                    foreach (Player player in PlayerDictionary.Values)
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
                foreach (Player player in PlayerDictionary.Values)
                {
                    player.Session.Send(packet);
                }
            }
        }

        public void MoveObject(int objectID, Vector2Int curCellPos, EMoveDirection moveDirection)
        {
            lock (lockObj)
            {
                EGameObjectType type = ObjectManager.GetObjectTypeByID(objectID);

                if (objectDictionary.TryGetValue(type, out Dictionary<int, GameObject> dictionary) == false) return;
                if (dictionary.TryGetValue(objectID, out GameObject gameObject) == false) return;

                if (curCellPos.X != gameObject.Position.X || curCellPos.Y != gameObject.Position.Y) return;

                if (moveDirection == EMoveDirection.None)
                {
                    gameObject.CurState = EObjectState.Idle;
                    gameObject.MoveDirection = EMoveDirection.None;
                }
                else
                {
                    if (gameObject.CurState != EObjectState.Move)
                    {
                        gameObject.CurState = EObjectState.Move;
                    }

                    gameObject.MoveDirection = moveDirection;
                }

                Pos curPos = gameObject.Position;
                Map.MoveObject(gameObject, moveDirection);
                Pos targetPos = gameObject.Position;

                PerformMoveBroadcast performMoveBroadcastPacket = new()
                {
                    ObjectID = gameObject.ID,
                    MoveDirection = gameObject.MoveDirection,
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
                EGameObjectType type = ObjectManager.GetObjectTypeByID(objectID);

                if (objectDictionary.TryGetValue(type, out Dictionary<int, GameObject> dictionary) == false) return;
                if (dictionary.TryGetValue(objectID, out GameObject gameObject) == false) return;

                // TODO : Certify the attack info passed by the packet

                // TODO : Check whether the gameobject can attack
                if (gameObject.CurState != EObjectState.Idle) return;

                gameObject.CurState = EObjectState.Attack;

                long attackStartTime = DateTime.UtcNow.Ticks;

                PerformAttackBroadcast performAttackBroadcastPacket = new()
                {
                    ObjectID = objectID,
                    AttackStartTime = attackStartTime,
                    AttackInfo = new() { AttackID = attackInfo.AttackID }
                };

                Broadcast(performAttackBroadcastPacket);

                // TODO : Perform the damage calculation

                // Melee Attack
                if (attackInfo.AttackID == 1)
                {
                    Pos attackPos = gameObject.GetFrontPos();

                    if (Map.Find(attackPos, out List<GameObject> objectList) == false) return;

                    foreach (GameObject obj in objectList)
                    {
                        HitBroadcast hitBroadcastPacket = new()
                        {
                            AttackerID = objectID,
                            DefenderID = obj.ID
                        };

                        Broadcast(hitBroadcastPacket);
                    }
                }
                else if (attackInfo.AttackID == 2)
                {
                    Arrow arrow = ObjectManager.Instance.Add<Arrow>();

                    if (ReferenceEquals(arrow, null) == true) return;

                    arrow.Owner = gameObject;
                    arrow.Name = "Arrow";
                    arrow.CurState = EObjectState.Move;
                    arrow.Position = new Pos(gameObject.Position.X, gameObject.Position.Y);
                    arrow.MoveDirection = gameObject.FacingDirection;
                    arrow.MoveSpeed = 10;
                    arrow.IsCollidable = false;

                    AddObject(arrow);
                }
            }
        }

        public void SetObjectState(int objectID, EObjectState state)
        {
            lock (lockObj)
            {
                EGameObjectType type = ObjectManager.GetObjectTypeByID(objectID);

                if (objectDictionary.TryGetValue(type, out Dictionary<int, GameObject> dictionary) == false) return;
                if (dictionary.TryGetValue(objectID, out GameObject gameObject) == false) return;

                gameObject.CurState = state;
            }
        }

        #endregion Methods
    }
}