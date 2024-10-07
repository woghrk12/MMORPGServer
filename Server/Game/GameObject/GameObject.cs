using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class GameObject
    {
        #region Variables

        private EMoveDirection moveDirection = EMoveDirection.None;

        private event Action updated = null;

        private long attackEndTicks = 0;

        #endregion Variables

        #region Properties

        public GameRoom Room { set; get; }

        // [UNUSED(1)][TYPE(7)][ID(24)]
        public int ID { set; get; } = -1;

        public string Name { set; get; } = string.Empty;

        public EGameObjectType ObjectType { protected set; get; }

        public EObjectState CurState { set; get; }

        public Pos Position { set; get; }

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

        public EMoveDirection FacingDirection { private set; get; } = EMoveDirection.Right;

        public bool IsCollidable { set; get; }

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

            foreach (Player player in Room.PlayerDictionary.Values)
            {
                player.Session.Send(packet);
            }

            updated -= CheckAttackPostDelay;
        }

        #region Events

        public void OnUpdate()
        {
            updated?.Invoke();
        }

        #endregion Events

        #endregion Methods
    }
}