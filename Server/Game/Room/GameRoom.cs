using Google.Protobuf;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class GameRoom : TaskQueue
    {
        #region Properties

        public int RoomID { set; get; }

        public Map Map { private set; get; } = new();

        public Dictionary<int, Character> CharacterDictionary { private set; get; } = new();
        public Dictionary<int, Monster> MonsterDictionary { private set; get; } = new();
        public Dictionary<int, Projectile> ProjectileDictionary { private set; get; } = new();

        #endregion Properties

        #region  Constructor

        public GameRoom(int mapID)
        {
            Map.LoadMap(mapID);

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
            foreach (Character character in CharacterDictionary.Values)
            {
                character.OnUpdate();
            }

            foreach (Monster monster in MonsterDictionary.Values)
            {
                monster.OnUpdate();
            }

            foreach (Projectile projectile in ProjectileDictionary.Values)
            {
                projectile.OnUpdate();
            }

            Flush();
        }

        public void Broadcast(IMessage packet, int excludedPlayerID = -1) => Push(Broadcast_T, packet, excludedPlayerID);

        public void Send(IMessage packet, int targetPlayerID) => Push(Send_T, packet, targetPlayerID);

        public void EnterRoom(Character newCharacter) => Push(EnterRoom_T, newCharacter);

        public void LeaveRoom(int leftPlayerID) => Push(LeaveRoom_T, leftPlayerID);

        public void AddObject(GameObject gameObject) => Push(AddObject_T, gameObject);

        public void RemoveObject(int oldObjectID) => Push(RemoveObject_T, oldObjectID);

        public void PerformMove(int objectID, Pos curPos, EMoveDirection moveDirection) => Push(PerformMove_T, objectID, curPos, moveDirection);

        public void PerformAttack(int creatureID, int attackID) => Push(PerformAttack_T, creatureID, attackID);

        public void ReviveCreature(int creatureID, Pos revivePos) => Push(ReviveCreature_T, creatureID, revivePos);

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

        private void Send_T(IMessage packet, int targetCharacterID)
        {
            if (CharacterDictionary.TryGetValue(targetCharacterID, out Character character) == false) return;

            character.Session.Send(packet);
        }

        private void EnterRoom_T(Character newCharacter)
        {
            if (ReferenceEquals(newCharacter, null) == true) return;

            CharacterDictionary.Add(newCharacter.ID, newCharacter);
            newCharacter.Room = this;

            Map.AddObject(newCharacter);

            ObjectInfo newCharacterInfo = new()
            {
                ObjectID = newCharacter.ID,
                Name = newCharacter.Name,
                PosX = newCharacter.Position.X,
                PosY = newCharacter.Position.Y,
                IsCollidable = newCharacter.IsCollidable,
                CreatureInfo = new CreatureInfo()
                {
                    CurState = newCharacter.CurState,
                    MoveDirection = newCharacter.MoveDirection,
                    FacingDirection = newCharacter.FacingDirection,
                    MoveSpeed = newCharacter.MoveSpeed,
                    Stat = new CreatureStat()
                    {
                        MaxHP = newCharacter.MaxHp,
                        CurHP = newCharacter.CurHp,
                        AttackPower = newCharacter.AttackPower
                    }
                }
            };

            // Send the packets to the player who has just enterend the room
            CharacterEnterGameRoomResponse characterEnteredRoomResponsePacket = new()
            {
                ResultCode = 0,
                NewCharacter = newCharacterInfo
            };

            foreach (Character character in CharacterDictionary.Values)
            {
                if (newCharacter.ID == character.ID) continue;

                ObjectInfo characterInfo = new()
                {
                    ObjectID = character.ID,
                    Name = character.Name,
                    PosX = character.Position.X,
                    PosY = character.Position.Y,
                    IsCollidable = character.IsCollidable,
                    CreatureInfo = new CreatureInfo()
                    {
                        CurState = character.CurState,
                        MoveDirection = character.MoveDirection,
                        FacingDirection = character.FacingDirection,
                        MoveSpeed = character.MoveSpeed,
                        Stat = new CreatureStat()
                        {
                            MaxHP = character.MaxHp,
                            CurHP = character.CurHp,
                            AttackPower = character.AttackPower
                        }
                    }
                };

                characterEnteredRoomResponsePacket.OtherObjects.Add(characterInfo);
            }

            foreach (Monster monster in MonsterDictionary.Values)
            {
                ObjectInfo monsterInfo = new()
                {
                    ObjectID = monster.ID,
                    Name = monster.Name,
                    PosX = monster.Position.X,
                    PosY = monster.Position.Y,
                    IsCollidable = monster.IsCollidable,
                    CreatureInfo = new CreatureInfo()
                    {
                        CurState = monster.CurState,
                        MoveDirection = monster.MoveDirection,
                        FacingDirection = monster.FacingDirection,
                        MoveSpeed = monster.MoveSpeed,
                        Stat = new CreatureStat()
                        {
                            MaxHP = monster.MaxHp,
                            CurHP = monster.CurHp,
                            AttackPower = monster.AttackPower
                        }
                    }
                };

                characterEnteredRoomResponsePacket.OtherObjects.Add(monsterInfo);
            }

            foreach (Projectile projectile in ProjectileDictionary.Values)
            {
                ObjectInfo projectileInfo = new()
                {
                    ObjectID = projectile.ID,
                    Name = projectile.Name,
                    PosX = projectile.Position.X,
                    PosY = projectile.Position.Y,
                    IsCollidable = projectile.IsCollidable,
                    ProjectileInfo = new ProjectileInfo()
                    {
                        MoveDirection = projectile.MoveDirection,
                        MoveSpeed = projectile.MoveSpeed
                    }
                };

                characterEnteredRoomResponsePacket.OtherObjects.Add(projectileInfo);
            }

            Send(characterEnteredRoomResponsePacket, newCharacter.ID);

            // Send the packets to the players who are in the room
            CharacterEnterGameRoomBroadcast characterEnteredRoomBroadcast = new()
            {
                NewCharacter = newCharacterInfo
            };

            Broadcast(characterEnteredRoomBroadcast, newCharacter.ID);
        }

        private void LeaveRoom_T(int leftCharacterID)
        {
            if (CharacterDictionary.Remove(leftCharacterID, out Character leftCharacter) == false) return;

            Map.RemoveObject(leftCharacter);
            leftCharacter.Room = null;

            Send(new CharacterLeftGameRoomResponse(), leftCharacterID);
            Broadcast(new CharacterLeftGameRoomBroadcast() { LeftCharacterID = leftCharacterID }, leftCharacterID);
        }

        private void AddObject_T(GameObject gameObject)
        {
            if (ReferenceEquals(gameObject, null) == true) return;

            gameObject.Room = this;
            Map.AddObject(gameObject);

            ObjectSpawnedBroadcast packet = new();

            switch (gameObject.ObjectType)
            {
                case EGameObjectType.Character:
                    Character newCharacter = gameObject as Character;

                    CharacterDictionary.Add(newCharacter.ID, newCharacter);

                    ObjectInfo newCharacterInfo = new()
                    {
                        ObjectID = newCharacter.ID,
                        Name = newCharacter.Name,
                        PosX = newCharacter.Position.X,
                        PosY = newCharacter.Position.Y,
                        IsCollidable = newCharacter.IsCollidable,
                        CreatureInfo = new CreatureInfo()
                        {
                            CurState = newCharacter.CurState,
                            MoveDirection = newCharacter.MoveDirection,
                            FacingDirection = newCharacter.FacingDirection,
                            MoveSpeed = newCharacter.MoveSpeed,
                            Stat = new CreatureStat()
                            {
                                MaxHP = newCharacter.MaxHp,
                                CurHP = newCharacter.CurHp,
                                AttackPower = newCharacter.AttackPower
                            }
                        }
                    };

                    packet.NewObjectInfo = newCharacterInfo;

                    break;

                case EGameObjectType.Monster:
                    Monster newMonster = gameObject as Monster;

                    MonsterDictionary.Add(newMonster.ID, newMonster);

                    ObjectInfo newMonsterInfo = new()
                    {
                        ObjectID = newMonster.ID,
                        Name = newMonster.Name,
                        PosX = newMonster.Position.X,
                        PosY = newMonster.Position.Y,
                        IsCollidable = newMonster.IsCollidable,
                        CreatureInfo = new CreatureInfo()
                        {
                            CurState = newMonster.CurState,
                            MoveDirection = newMonster.MoveDirection,
                            FacingDirection = newMonster.FacingDirection,
                            MoveSpeed = newMonster.MoveSpeed,
                            Stat = new CreatureStat()
                            {
                                MaxHP = newMonster.MaxHp,
                                CurHP = newMonster.CurHp,
                                AttackPower = newMonster.AttackPower
                            }
                        }
                    };

                    packet.NewObjectInfo = newMonsterInfo;

                    break;

                case EGameObjectType.Projectile:
                    Projectile newProjectile = gameObject as Projectile;

                    ProjectileDictionary.Add(newProjectile.ID, newProjectile);

                    ObjectInfo newProjectileInfo = new()
                    {
                        ObjectID = newProjectile.ID,
                        Name = newProjectile.Name,
                        PosX = newProjectile.Position.X,
                        PosY = newProjectile.Position.Y,
                        IsCollidable = newProjectile.IsCollidable,
                        ProjectileInfo = new ProjectileInfo()
                        {
                            MoveDirection = newProjectile.MoveDirection,
                            MoveSpeed = newProjectile.MoveSpeed
                        }
                    };

                    packet.NewObjectInfo = newProjectileInfo;

                    break;

                default:
                    Console.WriteLine($"Undefined Object Type. ID : {gameObject.ID} Type : {gameObject.ObjectType}");
                    return;
            }

            Broadcast(packet);
        }

        private void RemoveObject_T(int oldObjectID)
        {
            EGameObjectType type = ObjectManager.GetObjectTypeByID(oldObjectID);

            switch (type)
            {
                case EGameObjectType.Character:
                    if (CharacterDictionary.TryGetValue(oldObjectID, out Character character) == false) return;

                    Map.RemoveObject(character);
                    character.Room = null;
                    break;

                case EGameObjectType.Monster:
                    if (MonsterDictionary.TryGetValue(oldObjectID, out Monster monster) == false) return;

                    Map.RemoveObject(monster);
                    monster.Room = null;
                    break;

                case EGameObjectType.Projectile:
                    if (ProjectileDictionary.TryGetValue(oldObjectID, out Projectile projectile) == false) return;

                    Map.RemoveObject(projectile);
                    projectile.Room = null;
                    break;

                default:
                    Console.WriteLine($"Undefined Object Type. {type}");
                    return;
            }

            Broadcast(new ObjectDespawnedBroadcast() { OldObjectID = oldObjectID });
            ObjectManager.Instance.Remove(oldObjectID);
        }

        private void PerformMove_T(int objectID, Pos curPos, EMoveDirection moveDirection)
        {
            EGameObjectType type = ObjectManager.GetObjectTypeByID(objectID);

            GameObject gameObject;

            switch (type)
            {
                case EGameObjectType.Character:
                    if (CharacterDictionary.TryGetValue(objectID, out Character character) == false) return;

                    character.CurState = moveDirection != EMoveDirection.None ? ECreatureState.Move : ECreatureState.Idle;
                    character.MoveDirection = moveDirection;

                    gameObject = character;

                    break;

                case EGameObjectType.Monster:
                    if (MonsterDictionary.TryGetValue(objectID, out Monster monster) == false) return;

                    monster.CurState = moveDirection != EMoveDirection.None ? ECreatureState.Move : ECreatureState.Idle;
                    monster.MoveDirection = moveDirection;

                    gameObject = monster;

                    break;

                case EGameObjectType.Projectile:
                    if (ProjectileDictionary.TryGetValue(objectID, out Projectile projectile) == false) return;

                    projectile.MoveDirection = moveDirection;

                    gameObject = projectile;

                    break;

                default:
                    Console.WriteLine($"Unmovable GameObject Type. ID : {objectID} Type : {type}");

                    return;
            }

            Map.MoveObject(gameObject, moveDirection);

            PerformMoveBroadcast performMoveBroadcastPacket = new()
            {
                ObjectID = gameObject.ID,
                MoveDirection = moveDirection,
                TargetPosX = gameObject.Position.X,
                TargetPosY = gameObject.Position.Y
            };

            Broadcast(performMoveBroadcastPacket);
        }

        private void PerformAttack_T(int creatureID, int attackID)
        {
            EGameObjectType type = ObjectManager.GetObjectTypeByID(creatureID);

            Creature attacker;

            switch (type)
            {
                case EGameObjectType.Character:
                    if (CharacterDictionary.TryGetValue(creatureID, out Character character) == false) return;

                    attacker = character;

                    break;

                case EGameObjectType.Monster:
                    if (MonsterDictionary.TryGetValue(creatureID, out Monster monster) == false) return;

                    attacker = monster;

                    break;

                default:
                    Console.WriteLine($"Unattackable GameObject Type. ID : {creatureID} Type : {type}");

                    return;
            }

            if (attacker.CurState != ECreatureState.Idle) return;

            if (DataManager.AttackStatDictionary.TryGetValue(attackID, out Data.AttackStat attackStat) == false) return;

            attacker.CurState = ECreatureState.Attack;
            attacker.AttackStat = attackStat;

            PerformAttackBroadcast performAttackBroadcastPacket = new()
            {
                CreatureID = attacker.ID,
                AttackID = attackID
            };

            Broadcast(performAttackBroadcastPacket);

            switch (attackStat.AttackType)
            {
                case EAttackType.Melee:
                    for (int i = 1; i <= attackStat.Range; i++)
                    {
                        Pos attackPos = Utility.GetFrontPos(attacker.Position, attacker.FacingDirection, i);

                        if (Map.Find(attackPos, out List<GameObject> objectList) == false) continue;

                        foreach (GameObject obj in objectList)
                        {
                            Creature damagable = obj as Creature;

                            if (ReferenceEquals(damagable, null) == true) continue;

                            damagable.OnDamaged(attacker, attacker.AttackPower * attackStat.CoeffDictionary[attacker.Level]);
                        }
                    }

                    break;

                case EAttackType.Range:
                    if (DataManager.ProjectileStatDictionary.TryGetValue(attackStat.ProjectileID, out Data.ProjectileStat projectileStat) == false) return;

                    // TODO : Add the logic to generate different projectiles based on attack data
                    Arrow arrow = ObjectManager.Instance.Add<Arrow>();

                    arrow.Owner = attacker;
                    arrow.AttackStat = attackStat;
                    arrow.Name = projectileStat.Name;
                    arrow.Position = new Pos(attacker.Position.X, attacker.Position.Y);
                    arrow.MoveDirection = attacker.FacingDirection;
                    arrow.MoveSpeed = projectileStat.Speed;
                    arrow.IsCollidable = false;

                    Push(AddObject, arrow);

                    break;
            }
        }

        private void ReviveCreature_T(int creatureID, Pos revivePos)
        {
            EGameObjectType type = ObjectManager.GetObjectTypeByID(creatureID);

            Creature creature;

            switch (type)
            {
                case EGameObjectType.Character:
                    if (CharacterDictionary.TryGetValue(creatureID, out Character character) == false) return;

                    creature = character;

                    break;

                case EGameObjectType.Monster:
                    if (MonsterDictionary.TryGetValue(creatureID, out Monster monster) == false) return;

                    creature = monster;

                    break;

                default:
                    Console.WriteLine($"Unrevivable GameObject Type. ID : {creatureID} Type : {type}");

                    return;
            }

            if (creature.CurState != ECreatureState.Dead) return;

            creature.CurState = ECreatureState.Idle;
            creature.MoveDirection = EMoveDirection.None;
            creature.IsCollidable = true;
            creature.CurHp = creature.MaxHp;

            Map.MoveObject(creature, revivePos);

            CharacterReviveBroadcast packet = new()
            {
                CharacterID = creature.ID,
                RevivePosX = creature.Position.X,
                RevivePosY = creature.Position.Y
            };

            Broadcast(packet);
        }

        #endregion Methods
    }
}