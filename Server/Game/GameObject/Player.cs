using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Player : GameObject
    {
        #region Properties

        public ClientSession Session { set; get; }

        #endregion Properties

        #region Constructor

        public Player(int ID) : base(ID)
        {
            ObjectType = EGameObjectType.Player;
        }

        #endregion Constructor
    }
}