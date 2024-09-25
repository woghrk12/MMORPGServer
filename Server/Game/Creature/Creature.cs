using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Creature
    {
        #region Variables

        private EMoveDirection moveDirection = EMoveDirection.None;

        #endregion Variables

        #region Properties

        public GameRoom Room { set; get; }

        public int ID { private set; get; } = -1;
        public string Name { set; get; } = string.Empty;

        public ECreatureState CurState { set; get; }
        public Pos Position { set; get; }

        public int MoveSpeed { set; get; } = 5;
        public EMoveDirection MoveDirection
        {
            set
            {
                if (moveDirection == value) return;

                moveDirection = value;

                if (moveDirection != EMoveDirection.None)
                {
                    FacingDirection = moveDirection;
                }
            }
            get => moveDirection;
        }

        public EMoveDirection FacingDirection { private set; get; } = EMoveDirection.Right;

        #endregion Properties

        #region Constructor

        public Creature(int ID)
        {
            this.ID = ID;
        }

        #endregion Constructor

        #region Methods

        public Pos GetFrontPos(int distance = 1)
        {
            Pos frontPos = Position;

            switch (FacingDirection)
            {
                case EMoveDirection.Up:
                    frontPos += Pos.Up * distance;
                    break;

                case EMoveDirection.Down:
                    frontPos += Pos.Down * distance;
                    break;

                case EMoveDirection.Left:
                    frontPos += Pos.Left * distance;
                    break;

                case EMoveDirection.Right:
                    frontPos += Pos.Right * distance;
                    break;
            }

            return frontPos;
        }

        #endregion Methods
    }
}