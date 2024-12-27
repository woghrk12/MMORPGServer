using Google.Protobuf.Protocol;

namespace Server.Game.MonsterAI
{
    public class ChaseState : State
    {
        #region Variables

        private long nextMoveTicks = 0;

        private int chaseRange = 0;

        #endregion Variables

        #region Properties

        public override EMonsterState MonsterStateID => EMonsterState.CHASING;

        public override bool IsTransitionBlocked => nextMoveTicks >= Environment.TickCount64;

        #endregion Properties

        #region Constructor

        public ChaseState(Monster controller, int chaseRange) : base(controller)
        {
            this.chaseRange = chaseRange;
        }

        #endregion Constructor

        #region Methods

        public override void OnEnter()
        {
            controller.CurState = ECreatureState.Move;

            nextMoveTicks = 0;
        }

        public override void OnUpdate()
        {
            if (nextMoveTicks >= Environment.TickCount64) return;

            GameRoom room = controller.Room;
            if (ReferenceEquals(room, null) == true) return;

            if (controller.NextPos == controller.Position) return;

            nextMoveTicks = Environment.TickCount64 + (long)(1000f / controller.MoveSpeed);

            room.MoveMonster(controller.ID, Utility.GetDirection(controller.Position, controller.NextPos));
        }

        #endregion Methods
    }
}