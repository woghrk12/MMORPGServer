using Google.Protobuf.Protocol;

namespace Server.Game.MonsterState
{
    public class IdleState : MonsterState
    {
        #region Properties

        public sealed override EObjectState StateID => EObjectState.Idle;

        #endregion Properties

        #region Constructor

        public IdleState(Monster controller) : base(controller) { }

        #endregion Constructor
    }
}