using System.Data;
using Google.Protobuf.Protocol;

namespace Server.Game.MonsterAI
{
    public class PatrolState : State
    {
        #region Variables

        private long nextMoveTicks = 0;

        private int patrolRange = 0;

        #endregion Variables

        #region Properties

        public override EMonsterState MonsterStateID => EMonsterState.PATROL;

        public override bool IsTransitionBlocked => nextMoveTicks >= Environment.TickCount64;

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
            GameRoom room = controller.Room;
            if (ReferenceEquals(room, null) == true) return;

            Random rand = new();
            do
            {
                controller.TargetPos = controller.Position + new Pos(rand.Next(-patrolRange, patrolRange), rand.Next(-patrolRange, patrolRange));
            }
            while (room.Map.CheckCanMove(controller.TargetPos) == false);
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