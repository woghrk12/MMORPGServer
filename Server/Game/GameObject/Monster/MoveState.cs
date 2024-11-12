using System.ComponentModel;
using Google.Protobuf.Protocol;

namespace Server.Game.MonsterState
{
    public class MoveState : MonsterState
    {
        #region Variables

        private bool isChasing = false;
        private long nextMoveTicks = 0;

        #endregion Variables

        #region Properties

        public sealed override ECreatureState StateID => ECreatureState.Move;

        #endregion Properties

        #region Constructor

        public MoveState(Monster controller) : base(controller) { }

        #endregion Constructor

        #region Methods

        public override void OnEnter()
        {
            isChasing = ReferenceEquals(controller.Target, null) == false;
        }

        public override void OnUpdate()
        {
            if (controller.MoveSpeed == 0)
            {
                controller.CurState = ECreatureState.Idle;
                return;
            }

            if (nextMoveTicks >= Environment.TickCount64) return;

            GameRoom room = controller.Room;
            if (ReferenceEquals(room, null) == true) return;

            Pos nextPos;

            if (isChasing)
            {
                Character target = controller.Target;
                if (ReferenceEquals(target, null) == true)
                {
                    controller.Target = null;
                    controller.CurState = ECreatureState.Idle;
                    return;
                }

                GameRoom targetRoom = target.Room;
                if (ReferenceEquals(room, targetRoom) == false)
                {
                    controller.Target = null;
                    controller.CurState = ECreatureState.Idle;
                    return;
                }

                if (Utility.CalculateDistance(controller.Position, target.Position) > controller.ChaseRange)
                {
                    controller.Target = null;
                    controller.CurState = ECreatureState.Idle;
                    return;
                }

                if (room.Map.FindPath(controller.Position, target.Position, out List<Pos> path) == false || path.Count > controller.ChaseRange)
                {
                    controller.Target = null;
                    controller.CurState = ECreatureState.Idle;
                    return;
                }

                nextPos = path[1];
            }
            else
            {
                Character target = controller.Target;

                if (ReferenceEquals(target, null) == false)
                {
                    isChasing = true;
                    return;
                }

                if (room.Map.FindPath(controller.Position, controller.PatrolPos, out List<Pos> path) == false || path.Count > controller.PatrolRange)
                {
                    controller.CurState = ECreatureState.Idle;
                    return;
                }

                nextPos = path[1];
            }

            Vector2Int moveVector = new Vector2Int(nextPos.X - controller.Position.X, nextPos.Y - controller.Position.Y);
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

            nextMoveTicks = Environment.TickCount64 + (long)(1000f / controller.MoveSpeed);

            room.MoveMonster(controller.ID, moveDirection);
        }

        #endregion Methods
    }
}