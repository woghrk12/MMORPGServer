using Google.Protobuf.Protocol;

namespace Server.Game.MonsterState
{
    public class MoveState : MonsterState
    {
        #region Variables

        private long nextMoveTicks = 0;

        #endregion Variables

        #region Properties

        public sealed override EObjectState StateID => EObjectState.Move;

        #endregion Properties

        #region Constructor

        public MoveState(Monster controller) : base(controller) { }

        #endregion Constructor

        #region Methods

        public override void OnUpdate()
        {
            if (controller.MoveSpeed == 0)
            {
                controller.CurState = EObjectState.Idle;
                return;
            }

            if (nextMoveTicks >= Environment.TickCount64) return;
            nextMoveTicks = Environment.TickCount64 + (long)(1000f / controller.MoveSpeed);

            GameRoom room = controller.Room;
            if (ReferenceEquals(room, null) == true) return;

            Player target = controller.Target;
            if (ReferenceEquals(target, null) == true || ReferenceEquals(room, target.Room) == false)
            {
                controller.Target = null;
                controller.CurState = EObjectState.Idle;
                return;
            }

            if (Utility.CalculateDistance(controller.Position, target.Position) > controller.ChaseRange)
            {
                controller.Target = null;
                controller.CurState = EObjectState.Idle;
                return;
            }

            if (room.Map.FindPath(controller.Position, target.Position, out List<Pos> path) == false || path.Count > controller.ChaseRange)
            {
                controller.Target = null;
                controller.CurState = EObjectState.Idle;
                return;
            }

            Vector2Int moveVector = new Vector2Int(path[1].X - controller.Position.X, path[1].Y - controller.Position.Y);
            EMoveDirection moveDirection = EMoveDirection.None;

            if (moveVector == Vector2Int.Up)
            {
                moveDirection = EMoveDirection.Up;
            }
            else if (moveVector == Vector2Int.Down)
            {
                moveDirection = EMoveDirection.Down;
            }
            else if (moveVector == Vector2Int.Left)
            {
                moveDirection = EMoveDirection.Left;
            }
            else if (moveVector == Vector2Int.Right)
            {
                moveDirection = EMoveDirection.Right;
            }

            room.PerformMove(controller.ID, controller.Position, moveDirection);
        }

        #endregion Methods
    }
}