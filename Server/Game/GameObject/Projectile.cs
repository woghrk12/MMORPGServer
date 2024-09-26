using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Projectile : GameObject
    {
        #region Properties

        public ClientSession Session { set; get; }

        #endregion Properties

        #region Constructor

        public Projectile(int ID) : base(ID)
        {
            ObjectType = EGameObjectType.Projectile;
        }

        #endregion Constructor
    }
}