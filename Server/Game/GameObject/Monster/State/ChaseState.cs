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
            IsTransitionBlocked = true;
        }

        public override void OnUpdate()
        {
            if (nextMoveTicks >= Environment.TickCount64)
            {
                IsTransitionBlocked = true;
                return;
            }

            GameRoom room = controller.Room;
            if (ReferenceEquals(room, null) == true) return;

            if (room.Map.FindPath(controller.Position, controller.TargetPos, out List<Pos> path, chaseRange) == false)
            {
                controller.CurMonsterState = EMonsterState.IDLE;
                return;
            }

            IsTransitionBlocked = false;
            nextMoveTicks = Environment.TickCount64 + (long)(1000f / controller.MoveSpeed);

            room.MoveMonster(controller.ID, Utility.GetDirection(controller.Position, path[1]));
        }

        #endregion Methods
    }
}