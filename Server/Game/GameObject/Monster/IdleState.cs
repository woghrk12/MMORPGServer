using Google.Protobuf.Protocol;

namespace Server.Game.MonsterState
{
    public class IdleState : MonsterState
    {
        #region Variables

        private long nextDetectionTicks = 0;

        #endregion Variables

        #region Properties

        public sealed override EObjectState StateID => EObjectState.Idle;

        #endregion Properties

        #region Constructor

        public IdleState(Monster controller) : base(controller) { }

        #endregion Constructor

        #region Methods

        public override void OnUpdate()
        {
            if (nextDetectionTicks > Environment.TickCount64) return;
            nextDetectionTicks = Environment.TickCount64 + 1000;

            GameRoom room = controller.Room;
            if (ReferenceEquals(room, null) == true) return;

            Player target = room.FindPlayer(p =>
            {
                return Utility.CalculateDistance(p.Position, controller.Position) <= controller.DetectionRange;
            });

            if (ReferenceEquals(target, null) == true) return;

            controller.Target = target;

            controller.CurState = EObjectState.Move;
        }

        #endregion Methods
    }
}