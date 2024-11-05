using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class GameObject
    {
        #region Variables

        private EMoveDirection moveDirection = EMoveDirection.None;

        private event Action updated = null;

        protected Dictionary<EObjectState, State> stateDictionary = new();
        protected State curState = null;

        private long attackEndTicks = 0;

        #endregion Variables

        #region Properties

        public GameRoom Room { set; get; }

        // [UNUSED(1)][TYPE(7)][ID(24)]
        public int ID { set; get; } = -1;

        public string Name { set; get; } = string.Empty;

        public EGameObjectType ObjectType { protected set; get; }

        public Pos Position { set; get; } = new Pos(0, 0);

        public int MoveSpeed { set; get; } = 5;

        public EMoveDirection MoveDirection
        {
            set
            {
                if (moveDirection == value) return;

                moveDirection = value;

                if (moveDirection != EMoveDirection.None)
                {
                    FacingDirection = moveDirection;
                }
            }
            get => moveDirection;
        }

        public EMoveDirection FacingDirection { set; get; } = EMoveDirection.Right;

        public bool IsCollidable { set; get; } = true;

        public EObjectState CurState
        {
            set
            {
                if (stateDictionary.ContainsKey(value) == false) return;
                if (ReferenceEquals(curState, null) == false)
                {
                    if (curState.StateID == value) return;

                    curState.OnExit();
                }

                curState = stateDictionary[value];
                curState.OnEnter();

                GameRoom room = Room;
                if (ReferenceEquals(room, null) == false)
                {
                    UpdateObjectStateBroadcast packet = new()
                    {
                        ObjectID = ID,
                        NewState = curState.StateID
                    };

                    room.Broadcast(packet);
                }
            }
            get => ReferenceEquals(curState, null) == false ? curState.StateID : EObjectState.Idle;
        }

        public event Action Updated
        {
            add
            {
                updated += value;
            }
            remove
            {
                updated -= value;
            }
        }

        public ObjectStat Stat { private set; get; } = new();

        #endregion Properties

        #region Methods

        public Pos GetFrontPos(int distance = 1)
        {
            Pos frontPos = Position;

            switch (FacingDirection)
            {
                case EMoveDirection.Up:
                    frontPos += Pos.Up * distance;
                    break;

                case EMoveDirection.Down:
                    frontPos += Pos.Down * distance;
                    break;

                case EMoveDirection.Left:
                    frontPos += Pos.Left * distance;
                    break;

                case EMoveDirection.Right:
                    frontPos += Pos.Right * distance;
                    break;
            }

            return frontPos;
        }

        public void BeginAttackPostDelayCheck(long attackEndTicks)
        {
            this.attackEndTicks = attackEndTicks;

            updated += CheckAttackPostDelay;
        }

        private void CheckAttackPostDelay()
        {
            if (attackEndTicks > Environment.TickCount64) return;

            CurState = EObjectState.Idle;

            AttackCompleteBroadcast packet = new()
            {
                ObjectID = ID
            };

            foreach (Character character in Room.CharacterDictionary.Values)
            {
                character.Session.Send(packet);
            }

            updated -= CheckAttackPostDelay;
        }

        #region Events

        public void OnUpdate()
        {
            updated?.Invoke();
            curState?.OnUpdate();
        }

        public virtual void OnDamaged(GameObject attacker, int damage)
        {
            Stat.CurHP -= damage;

            if (Stat.CurHP <= 0)
            {
                Stat.CurHP = 0;

                OnDead(attacker);
            }

            HitBroadcast packet = new()
            {
                ObjectID = ID,
                CurHp = Stat.CurHP,
                Damage = damage
            };

            Room.Broadcast(packet);
        }

        public virtual void OnDead(GameObject attacker) { }

        #endregion Events

        #endregion Methods
    }
}