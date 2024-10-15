using Google.Protobuf;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class GameRoom : TaskQueue
    {
        #region Variables

        private Dictionary<EGameObjectType, Dictionary<int, GameObject>> objectDictionary = new();

        #endregion Variables

        #region Properties

        public int RoomID { set; get; }

        public Map Map { private set; get; } = new();

        public Dictionary<int, GameObject> PlayerDictionary => objectDictionary[EGameObjectType.Player];
        public Dictionary<int, GameObject> MonsterDictionary => objectDictionary[EGameObjectType.Monster];
        public Dictionary<int, GameObject> ProjectileDictionary => objectDictionary[EGameObjectType.Projectile];

        #endregion Properties

        #region Methods

        public void Init(int mapID) => Push(Init_T, mapID);

        public void Update() => Push(Update_T);

        public void Broadcast(IMessage packet, int excludedPlayerID = -1) => Push(Broadcast_T, packet, excludedPlayerID);

        public void Send(IMessage packet, int targetPlayerID) => Push(Send_T, packet, targetPlayerID);

        public void AddObject(GameObject gameObject) => Push(AddObject_T, gameObject);

        public void RemoveObject(int oldObjectID) => Push(RemoveObject_T, oldObjectID);

        public void EnterRoom(Player newPlayer) => Push(EnterRoom_T, newPlayer);

        public void LeaveRoom(int leftPlayerID) => Push(LeaveRoom_T, leftPlayerID);

        public void PerformMove(int objectID, Pos curPos, EMoveDirection moveDirection) => Push(PerformMove_T, objectID, curPos, moveDirection);

        public void PerformAttack(int objectID, int attackID) => Push(PerformAttack_T, objectID, attackID);

        public void ReviveObject(int objectID, Pos revivePos) => Push(ReviveObject_T, objectID, revivePos);

        private void Init_T(int mapID)
        {
            Map.LoadMap(mapID);

            Array types = Enum.GetValues(typeof(EGameObjectType));
            foreach (EGameObjectType type in types)
            {
                objectDictionary.Add(type, new Dictionary<int, GameObject>());
            }
        }

        private void Update_T()
        {
            foreach (Dictionary<int, GameObject> dictionary in objectDictionary.Values)
            {
                foreach (GameObject gameObject in dictionary.Values)
                {
                    gameObject.OnUpdate();
                }
            }
        }

        private void Broadcast_T(IMessage packet, int excludedPlayerID = -1)
        {
            foreach (Player player in PlayerDictionary.Values)
            {
                if (player.ID == excludedPlayerID) continue;

                player.Session.Send(packet);
            }
        }

        private void Send_T(IMessage packet, int targetPlayerID)
        {
            if (PlayerDictionary.TryGetValue(targetPlayerID, out GameObject gameObject) == false) return;
            if (gameObject.ObjectType != EGameObjectType.Player) return;

            (gameObject as Player).Session.Send(packet);
        }

        private void AddObject_T(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, null) == true) return;

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

        private void RemoveObject_T(int oldObjectID)
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

        private void EnterRoom_T(Player newPlayer)
        {
            if (ReferenceEquals(newPlayer, null) == true) return;

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

            Send(playerEnteredRoomResponse, newPlayer.ID);

            // Send the packets to the players who are in the room
            PlayerEnteredRoomBroadcast playerEnteredRoomBroadcast = new()
            {
                NewPlayer = newPlayerInfo
            };

            Broadcast(playerEnteredRoomBroadcast, newPlayer.ID);
        }

        private void LeaveRoom_T(int leftPlayerID)
        {
            if (PlayerDictionary.Remove(leftPlayerID, out GameObject leftPlayerObject) == false) return;

            Map.RemoveObject(leftPlayerObject);
            leftPlayerObject.Room = null;

            PlayerLeftRoomResponse playerLeftRoomResponsePacket = new();

            Send(playerLeftRoomResponsePacket, leftPlayerObject.ID);

            PlayerLeftRoomBroadcast playerLeftRoomBroadcastPacket = new()
            {
                OtherPlayerID = leftPlayerObject.ID
            };

            Broadcast(playerLeftRoomBroadcastPacket, leftPlayerObject.ID);
        }

        private void PerformMove_T(int objectID, Pos curPos, EMoveDirection moveDirection)
        {
            EGameObjectType type = ObjectManager.GetObjectTypeByID(objectID);

            if (objectDictionary.TryGetValue(type, out Dictionary<int, GameObject> dictionary) == false) return;
            if (dictionary.TryGetValue(objectID, out GameObject gameObject) == false) return;

            if (moveDirection == EMoveDirection.None)
            {
                gameObject.CurState = EObjectState.Idle;
                gameObject.MoveDirection = EMoveDirection.None;
            }
            else
            {
                gameObject.CurState = EObjectState.Move;
                gameObject.MoveDirection = moveDirection;
            }

            Pos pos = gameObject.Position;
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

        private void PerformAttack_T(int objectID, int attackID)
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

                    Push(AddObject, arrow);

                    break;
            }

            gameObject.BeginAttackPostDelayCheck(attackStartTime + (long)(1000 * attackStat.PostDelay));
        }

        private void ReviveObject_T(int objectID, Pos revivePos)
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

        #endregion Methods
    }
}