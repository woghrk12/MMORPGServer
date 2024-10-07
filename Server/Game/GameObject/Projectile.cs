using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Projectile : GameObject
    {
        #region Properties

        public GameObject Owner { set; get; } = null;

        public Data.AttackStat AttackStat { set; get; } = null;

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