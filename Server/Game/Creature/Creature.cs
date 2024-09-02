using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Creature
    {
        #region Variables

        protected Dictionary<ECreatureState, CreatureState> stateDictionary = new();
        protected CreatureState curState = null;
        private Vector2Int cellPos = Vector2Int.Zero;
        private EMoveDirection moveDirection = EMoveDirection.None;

        #endregion Variables

        #region Properties

        public int ID { private set; get; } = -1;
        public string Name { set; get; } = string.Empty;

        public Vector2Int CellPos
        {
            set
            {
                if (cellPos == value) return;

                cellPos = value;

                UpdateCreatureInfo();
            }
            get => cellPos;
        }

        public Vector2 CurPos { set; get; } = Vector2.Zero;
        public int MoveSpeed { set; get; } = 5;
        public EMoveDirection MoveDirection
        {
            set
            {
                if (moveDirection == value) return;

                moveDirection = value;
                FacingDirection = value;
            }
            get => moveDirection;
        }
        public EMoveDirection FacingDirection { set; get; } = EMoveDirection.None;

        public GameRoom Room { set; get; }

        public ECreatureState CurState
        {
            set
            {
                if (value == (ReferenceEquals(curState, null) == false ? curState.StateID : ECreatureState.Idle)) return;
                if (stateDictionary.TryGetValue(value, out CreatureState state) == false) return;

                curState?.OnEnd();
                curState = state;
                curState.OnStart();

                UpdateCreatureInfo();
            }
            get => ReferenceEquals(curState, null) == false ? curState.StateID : ECreatureState.Idle;
        }

        #endregion Properties

        #region Constructor

        public Creature(int ID)
        {
            this.ID = ID;
        }

        #endregion Constructor

        #region Methods

        public void UpdateCreatureInfo()
        {
            CreatureInfo info = new()
            {
                CreatureID = this.ID,
                Name = this.Name,
                CurState = this.CurState,
                CellPosX = this.cellPos.X,
                CellPosY = this.cellPos.Y,
                FacingDirection = this.FacingDirection,
                MoveSpeed = this.MoveSpeed
            };
            UpdateCreatureInfoBroadcast packet = new()
            {
                CreatureInfo = info
            };

            GameRoom room = this.Room;
            if (ReferenceEquals(room, null) == true) return;

            room.Brodcast(packet);
        }

        public virtual void OnUpdate(float deltaTime)
        {
            curState?.OnUpdate(deltaTime);
        }

        #endregion Methods
    }
}