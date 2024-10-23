using Google.Protobuf.Protocol;

namespace Server.Game.MonsterState
{
    public class DeadState : MonsterState
    {
        #region Properties

        public sealed override EObjectState StateID => EObjectState.Dead;

        #endregion Properties

        #region Constructor

        public DeadState(Monster controller) : base(controller) { }

        #endregion Constructor
    }
}