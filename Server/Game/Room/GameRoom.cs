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

        public Dictionary<int, GameObject> CharacterDictionary => objectDictionary[EGameObjectType.Character];
        public Dictionary<int, GameObject> MonsterDictionary => objectDictionary[EGameObjectType.Monster];
        public Dictionary<int, GameObject> ProjectileDictionary => objectDictionary[EGameObjectType.Projectile];

        #endregion Properties

        #region  Constructor

        public GameRoom(int mapID)
        {
            Map.LoadMap(mapID);

            Array types = Enum.GetValues(typeof(EGameObjectType));
            foreach (EGameObjectType type in types)
            {
                objectDictionary.Add(type, new Dictionary<int, GameObject>());
            }

            // Temp
            Monster monster = ObjectManager.Instance.Add<Monster>();
            monster.Position = new Pos(0, 2);
            monster.MoveSpeed = 3;

            AddObject(monster);
        }

        #endregion Constructor

        #region Methods

        public void Update()
        {
            foreach (Dictionary<int, GameObject> dictionary in objectDictionary.Values)
            {
                foreach (GameObject gameObject in dictionary.Values)
                {
                    gameObject.OnUpdate();
                }
            }

            Flush();
        }

        public void Broadcast(IMessage packet, int excludedPlayerID = -1) => Push(Broadcast_T, packet, excludedPlayerID);

        public void Send(IMessage packet, int targetPlayerID) => Push(Send_T, packet, targetPlayerID);

        public void AddObject(GameObject gameObject) => Push(AddObject_T, gameObject);

        public void RemoveObject(int oldObjectID) => Push(RemoveObject_T, oldObjectID);

        public void EnterRoom(Character newCharacter) => Push(EnterRoom_T, newCharacter);

        public void LeaveRoom(int leftPlayerID) => Push(LeaveRoom_T, leftPlayerID);

        public void PerformMove(int objectID, Pos curPos, EMoveDirection moveDirection) => Push(PerformMove_T, objectID, curPos, moveDirection);

        public void PerformAttack(int objectID, int attackID) => Push(PerformAttack_T, objectID, attackID);

        public void ReviveObject(int objectID, Pos revivePos) => Push(ReviveObject_T, objectID, revivePos);

        public Character FindCharacter(Func<GameObject, bool> condition)
        {
            foreach (Character character in CharacterDictionary.Values)
            {
                if (condition.Invoke(character) == false) continue;

                return character;
            }

            return null;
        }

        private void Broadcast_T(IMessage packet, int excludedCharacterID = -1)
        {
            foreach (Character character in CharacterDictionary.Values)
            {
                if (character.ID == excludedCharacterID) continue;

                character.Session.Send(packet);
            }
        }

        private void Send_T(IMessage packet, int targetPlayerID)
        {
            if (CharacterDictionary.TryGetValue(targetPlayerID, out GameObject gameObject) == false) return;
            if (gameObject.ObjectType != EGameObjectType.Character) return;

            (gameObject as Character).Session.Send(packet);
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

            dictionary.Remove(gameObject.ID);
            ObjectManager.Instance.Remove(gameObject.ID);
        }

        private void EnterRoom_T(Character newCharacter)
        {
            if (ReferenceEquals(newCharacter, null) == true) return;

            DataManager.SendData(newCharacter);

            CharacterDictionary.Add(newCharacter.ID, newCharacter);
            newCharacter.Room = this;

            Map.AddObject(newCharacter);

            ObjectInfo newPlayerInfo = new()
            {
                ObjectID = newCharacter.ID,
                Name = newCharacter.Name,
                CurState = newCharacter.CurState,
                PosX = newCharacter.Position.X,
                PosY = newCharacter.Position.Y,
                MoveDirection = newCharacter.MoveDirection,
                FacingDirection = newCharacter.FacingDirection,
                MoveSpeed = 5,
                IsCollidable = newCharacter.IsCollidable,
                ObjectStat = newCharacter.Stat
            };

            // Send the packets to the player who has just enterend the room
            CharacterEnterGameRoomResponse playerEnteredRoomResponse = new()
            {
                ResultCode = 0,
                NewCharacter = newPlayerInfo
            };

            foreach (Dictionary<int, GameObject> dictionary in objectDictionary.Values)
            {
                foreach (GameObject gameObject in dictionary.Values)
                {
                    if (newCharacter.ID == gameObject.ID) continue;

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

            Send(playerEnteredRoomResponse, newCharacter.ID);

            // Send the packets to the players who are in the room
            CharacterEnterGameRoomBroadcast playerEnteredRoomBroadcast = new()
            {
                NewCharacter = newPlayerInfo
            };

            Broadcast(playerEnteredRoomBroadcast, newCharacter.ID);
        }

        private void LeaveRoom_T(int leftCharacterID)
        {
            if (CharacterDictionary.Remove(leftCharacterID, out GameObject leftCharacterObject) == false) return;

            Map.RemoveObject(leftCharacterObject);
            leftCharacterObject.Room = null;

            PlayerLeftRoomResponse playerLeftRoomResponsePacket = new();

            Send(playerLeftRoomResponsePacket, leftCharacterObject.ID);

            PlayerLeftRoomBroadcast playerLeftRoomBroadcastPacket = new()
            {
                OtherPlayerID = leftCharacterObject.ID
            };

            Broadcast(playerLeftRoomBroadcastPacket, leftCharacterObject.ID);
        }

        private void PerformMove_T(int objectID, Pos curPos, EMoveDirection moveDirection)
        {
            EGameObjectType type = ObjectManager.GetObjectTypeByID(objectID);

            if (objectDictionary.TryGetValue(type, out Dictionary<int, GameObject> dictionary) == false) return;
            if (dictionary.TryGetValue(objectID, out GameObject gameObject) == false) return;

            if (moveDirection == EMoveDirection.None)
            {
                gameObject.CurState = ECreatureState.Idle;
                gameObject.MoveDirection = EMoveDirection.None;
            }
            else
            {
                gameObject.CurState = ECreatureState.Move;
                gameObject.MoveDirection = moveDirection;
            }

            Map.MoveObject(gameObject, moveDirection);

            PerformMoveBroadcast performMoveBroadcastPacket = new()
            {
                ObjectID = gameObject.ID,
                MoveDirection = gameObject.MoveDirection,
                TargetPosX = gameObject.Position.X,
                TargetPosY = gameObject.Position.Y
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
            if (gameObject.CurState != ECreatureState.Idle) return;

            gameObject.CurState = ECreatureState.Attack;

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
                            if (damagable.CurState == ECreatureState.Dead) continue;

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
                    arrow.CurState = ECreatureState.Move;
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

            gameObject.CurState = ECreatureState.Idle;
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