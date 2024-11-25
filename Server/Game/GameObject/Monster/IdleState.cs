using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class IdleState : MonsterState
    {
        #region Variables

        private long nextBehaviourTicks = 0;

        #endregion Variables

        #region Properties

        public override EMonsterState MonsterStateID => EMonsterState.IDLE;

        #endregion Properties

        #region Constructor

        public IdleState(Monster controller) : base(controller) { }

        #endregion Constructor

        #region Methods

        public override void OnEnter()
        {
            controller.CurState = ECreatureState.Idle;

            nextBehaviourTicks = Environment.TickCount64 + 3000;
        }

        public override void OnUpdate()
        {
            if (nextBehaviourTicks >= Environment.TickCount64) return;

            GameRoom room = controller.Room;
            if (ReferenceEquals(room, null) == true) return;

            controller.MonsterState = ReferenceEquals(controller.Target, null) == true ? EMonsterState.PATROL : EMonsterState.CHASING;
        }

        #endregion Methods
    }
}