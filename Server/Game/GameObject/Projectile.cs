using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Projectile : GameObject
    {
        #region Variables

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
        }

        #endregion Constructor
    }
}