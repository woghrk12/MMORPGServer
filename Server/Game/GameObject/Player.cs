using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Player : GameObject
    {
        #region Properties

        public ClientSession Session { set; get; }

        #endregion Properties

        #region Constructor

        public Player()
        {
            ObjectType = EGameObjectType.Player;
            IsCollidable = true;
        }

        #endregion Constructor
    }
}