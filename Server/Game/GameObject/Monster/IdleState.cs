using Google.Protobuf.Protocol;
using System;

namespace Server.Game.MonsterState
{
    public class IdleState : MonsterState
    {
        #region Variables

        private long nextBehaviourTicks = 0;
        private Random rand = new();

        #endregion Variables

        #region Properties

        public sealed override EObjectState StateID => EObjectState.Idle;

        #endregion Properties

        #region Constructor

        public IdleState(Monster controller) : base(controller) { }

        #endregion Constructor

        #region Methods

        public override void OnEnter()
        {
            nextBehaviourTicks = Environment.TickCount64 + 3000;
        }

        public override void OnUpdate()
        {
            if (nextBehaviourTicks >= Environment.TickCount64) return;

            GameRoom room = controller.Room;
            if (ReferenceEquals(room, null) == true) return;

            int patrolRange = controller.PatrolRange;
            controller.PatrolPos = controller.Position + new Pos(rand.Next(-patrolRange, patrolRange), rand.Next(-patrolRange, patrolRange));

            Console.WriteLine($"Patrol Pos : ({controller.PatrolPos.X}, {controller.PatrolPos.Y})");
            controller.CurState = EObjectState.Move;
        }

        #endregion Methods
    }
}