using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Creature
    {
        #region Properties

        public GameRoom Room { set; get; }

        public int ID { private set; get; } = -1;
        public string Name { set; get; } = string.Empty;

        public ECreatureState CurState { set; get; }
        public Vector2Int CellPos { set; get; }

        public int MoveSpeed { set; get; } = 5;
        public EMoveDirection MoveDirection { set; get; }

        #endregion Properties

        #region Constructor

        public Creature(int ID)
        {
            this.ID = ID;
        }

        #endregion Constructor
    }
}