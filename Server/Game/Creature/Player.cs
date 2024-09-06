namespace Server.Game
{
    public class Player : Creature
    {
        #region Properties

        public ClientSession Session { set; get; }

        #endregion Properties

        #region Constructor

        public Player(int ID) : base(ID)
        {

        }

        #endregion Constructor
    }
}