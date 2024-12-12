using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class ChaseState : MonsterState
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
        }

        public override void OnUpdate()
        {
            if (nextMoveTicks >= Environment.TickCount64) return;

            GameRoom room = controller.Room;
            if (ReferenceEquals(room, null) == true) return;

            Character target = controller.Target;
            if (ReferenceEquals(target, null) == true)
            {
                controller.MonsterState = EMonsterState.IDLE;
                return;
            }

            if (ReferenceEquals(room, target.Room) == false)
            {
                controller.Target = null;
                controller.MonsterState = EMonsterState.IDLE;
                return;
            }

            int distance = Utility.CalculateDistance(controller.Position, target.Position);
            if (distance <= 1)
            {
                controller.MoveDirection = EMoveDirection.None;
                controller.FacingDirection = Utility.GetDirection(controller.Position, target.Position);
                controller.MonsterState = EMonsterState.ATTACK;
                return;
            }

            if (distance > chaseRange)
            {
                controller.Target = null;
                controller.MonsterState = EMonsterState.IDLE;
                return;
            }

            if (room.Map.FindPath(controller.Position, target.Position, out List<Pos> path, chaseRange) == false)
            {
                controller.Target = null;
                controller.MonsterState = EMonsterState.IDLE;
                return;
            }

            nextMoveTicks = Environment.TickCount64 + (long)(1000f / controller.MoveSpeed);

            room.MoveMonster(controller.ID, Utility.GetDirection(controller.Position, path[1]));
        }

        #endregion Methods
    }
}