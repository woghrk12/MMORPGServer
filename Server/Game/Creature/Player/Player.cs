using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Player : Creature
    {
        #region Variables

        private EMoveDirection inputDirection = EMoveDirection.None;

        #endregion Variables

        #region Properties

        public ClientSession Session { set; get; }

        public EMoveDirection InputDirection
        {
            set
            {
                if (inputDirection == value) return;

                inputDirection = value;

                if (CurState == ECreatureState.Idle)
                {
                    CurState = ECreatureState.Move;
                }
            }
            get => inputDirection;
        }

        #endregion Properties

        #region Constructor

        public Player(int ID) : base(ID)
        {
            stateDictionary.Add(ECreatureState.Idle, new PlayerState.IdleState(this));
            stateDictionary.Add(ECreatureState.Move, new PlayerState.MoveState(this));

            curState = stateDictionary[ECreatureState.Idle];
            curState.OnStart();
        }

        #endregion Constructor
    }
}