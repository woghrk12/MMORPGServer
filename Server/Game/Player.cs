using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Player
    {
        #region Properties

        public PlayerInfo Info { set; get; } = new();

        public GameRoom Room { set; get; }

        public ClientSession Session { set; get; }

        #endregion Properties
    }
}