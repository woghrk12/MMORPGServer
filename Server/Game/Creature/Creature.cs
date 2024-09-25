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

        public Vector2Int GetFrontCellPos(int distance = 1)
        {
            Vector2Int frontPos = new Vector2Int(Position.X, Position.Y);

            switch (FacingDirection)
            {
                case EMoveDirection.Up:
                    frontPos += Vector2Int.Up * distance;
                    break;

                case EMoveDirection.Down:
                    frontPos += Vector2Int.Down * distance;
                    break;

                case EMoveDirection.Left:
                    frontPos += Vector2Int.Left * distance;
                    break;

                case EMoveDirection.Right:
                    frontPos += Vector2Int.Right * distance;
                    break;
            }

            return frontPos;
        }

        #endregion Methods
    }
}