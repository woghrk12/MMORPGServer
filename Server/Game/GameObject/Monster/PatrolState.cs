using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class PatrolState : MonsterState
    {
        #region Variables

        private long nextMoveTicks = 0;

        private int patrolRange = 0;
        private Pos patrolPos = new Pos(0, 0);

        #endregion Variables

        #region Properties

        public override EMonsterState MonsterStateID => EMonsterState.PATROL;

        #endregion Properties

        #region Constructor

        public PatrolState(Monster controller, int patrolRange) : base(controller)
        {
            this.patrolRange = patrolRange;
        }

        #endregion Constructor

        #region Methods

        public override void OnEnter()
        {
            controller.CurState = ECreatureState.Move;

            nextMoveTicks = 0;

            // TODO : Predefine the locations where movement is possible
            Random rand = new();
            patrolPos = controller.Position + new Pos(rand.Next(-patrolRange, patrolRange), rand.Next(-patrolRange, patrolRange));
        }

        public override void OnUpdate()
        {
            if (nextMoveTicks >= Environment.TickCount64) return;

            GameRoom room = controller.Room;
            if (ReferenceEquals(room, null) == true) return;

            if (ReferenceEquals(controller.Target, null) == false)
            {
                controller.MonsterState = EMonsterState.CHASING;
                return;
            }

            if (room.Map.FindPath(controller.Position, patrolPos, out List<Pos> path) == false)
            {
                controller.MonsterState = EMonsterState.IDLE;
                return;
            }

            nextMoveTicks = Environment.TickCount64 + (long)(1000f / controller.MoveSpeed);

            room.MoveMonster(controller.ID, Utility.GetDirection(controller.Position, path[1]));
        }

        #endregion Methods
    }
}