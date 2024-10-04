using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Projectile : GameObject
    {
        #region Properties

        public GameObject Owner { set; get; }

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