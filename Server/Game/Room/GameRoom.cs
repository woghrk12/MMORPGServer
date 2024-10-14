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
                    IsCollidable = gameObject.IsCollidable,
                    ObjectStat = gameObject.Stat
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
                DataManager.SendData(newPlayer);

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
                    IsCollidable = newPlayer.IsCollidable,
                    ObjectStat = newPlayer.Stat
                };

                // Send the packets to the player who has just enterend the room
                PlayerEnteredRoomResponse playerEnteredRoomResponse = new()
                {
                    NewPlayer = newPlayerInfo
                };

                foreach (Dictionary<int, GameObject> dictionary in objectDictionary.Values)
                {
                    foreach (GameObject gameObject in dictionary.Values)
                    {
                        if (newPlayer.ID == gameObject.ID) continue;

                        ObjectInfo otherObjectInfo = new()
                        {
                            ObjectID = gameObject.ID,
                            Name = gameObject.Name,
                            CurState = gameObject.CurState,
                            PosX = gameObject.Position.X,
                            PosY = gameObject.Position.Y,
                            MoveDirection = gameObject.MoveDirection,
                            FacingDirection = gameObject.FacingDirection,
                            MoveSpeed = gameObject.MoveSpeed,
                            IsCollidable = gameObject.IsCollidable,
                            ObjectStat = gameObject.Stat
                        };

                        playerEnteredRoomResponse.OtherObjects.Add(otherObjectInfo);
                    }
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

        public void PerformAttack(int objectID, int attackID)
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

                long attackStartTime = Environment.TickCount64;

                PerformAttackBroadcast performAttackBroadcastPacket = new()
                {
                    ObjectID = objectID,
                    AttackID = attackID
                };

                Broadcast(performAttackBroadcastPacket);

                if (DataManager.AttacklStatDictionary.TryGetValue(attackID, out Data.AttackStat attackStat) == false) return;

                switch (attackStat.AttackType)
                {
                    case EAttackType.Melee:
                        for (int i = 1; i <= attackStat.Range; i++)
                        {
                            Pos attackPos = gameObject.GetFrontPos(i);

                            if (Map.Find(attackPos, out List<GameObject> objectList) == false) continue;

                            List<GameObject> damagableList = objectList.FindAll((obj) => obj.ObjectType != EGameObjectType.Projectile);

                            foreach (GameObject damagable in damagableList)
                            {
                                if (damagable.CurState == EObjectState.Dead) continue;

                                // TODO : The attack coefficient needs to be adjusted based on the attacker's level
                                damagable.OnDamaged(gameObject, gameObject.Stat.AttackPower * attackStat.CoeffDictionary[1]);
                            }
                        }

                        break;

                    case EAttackType.Range:
                        if (DataManager.ProjectileStatDictionary.TryGetValue(attackStat.ProjectileID, out Data.ProjectileStat projectileStat) == false) return;

                        // TODO : Add the logic to generate different projectiles based on attack data
                        Arrow arrow = ObjectManager.Instance.Add<Arrow>();

                        arrow.Owner = gameObject;
                        arrow.AttackStat = attackStat;
                        arrow.Name = projectileStat.Name;
                        arrow.CurState = EObjectState.Move;
                        arrow.Position = new Pos(gameObject.Position.X, gameObject.Position.Y);
                        arrow.MoveDirection = gameObject.FacingDirection;
                        arrow.MoveSpeed = projectileStat.Speed;
                        arrow.IsCollidable = false;

                        AddObject(arrow);

                        break;
                }

                gameObject.BeginAttackPostDelayCheck(attackStartTime + (long)(1000 * attackStat.PostDelay));
            }
        }

        public void ReviveObject(int objectID, Pos revivePos)
        {
            lock (lockObj)
            {
                EGameObjectType type = ObjectManager.GetObjectTypeByID(objectID);

                if (type == EGameObjectType.Projectile) return;
                if (objectDictionary.TryGetValue(type, out Dictionary<int, GameObject> dictionary) == false) return;
                if (dictionary.TryGetValue(objectID, out GameObject gameObject) == false) return;

                gameObject.CurState = EObjectState.Idle;
                gameObject.MoveDirection = EMoveDirection.None;
                gameObject.IsCollidable = true;
                gameObject.Stat.CurHP = gameObject.Stat.MaxHP;

                Map.MoveObject(gameObject, revivePos);

                ObjectReviveBroadcast packet = new()
                {
                    ObjectID = gameObject.ID,
                    RevivePosX = gameObject.Position.X,
                    RevivePosY = gameObject.Position.Y
                };

                Broadcast(packet);
            }
        }

        #endregion Methods
    }
}