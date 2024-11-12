using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Game
{
    public abstract class Projectile : GameObject
    {
        #region Variables

        private long nextMoveTicks = 0;

        private EMoveDirection moveDirection = EMoveDirection.None;

        #endregion Variables

        #region Properties

        public Creature Owner { set; get; } = null;

        public Data.AttackStat AttackStat { set; get; } = null;

        public EMoveDirection MoveDirection
        {
            set
            {
                if (moveDirection == value) return;

                moveDirection = value;
            }
            get => moveDirection;
        }

        public int MoveSpeed { set; get; } = 5;

        #endregion Properties

        #region Constructor

        public Projectile()
        {
            ObjectType = EGameObjectType.Projectile;
            IsCollidable = false;

            Updated += Move;
        }

        #endregion Constructor

        #region Methods

        protected virtual void Move()
        {
            GameRoom room = Room;
            if (ReferenceEquals(room, null) == true) return;

            Creature owner = Owner;
            if (ReferenceEquals(owner, null) == true) return;

            AttackStat attackStat = AttackStat;
            if (ReferenceEquals(attackStat, null) == true) return;

            if (nextMoveTicks >= Environment.TickCount64) return;
            nextMoveTicks = Environment.TickCount64 + (long)(1000f / MoveSpeed);

            room.MoveProjectile(ID, MoveDirection, IsDestroyed, OnMoved);
        }

        protected abstract bool IsDestroyed(Pos frontPos);

        protected virtual void OnMoved(Pos frontPos) { }

        #endregion Methods
    }
}