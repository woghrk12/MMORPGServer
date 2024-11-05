using Google.Protobuf.Protocol;

namespace Server.Game.MonsterState
{
    public class AttackState : MonsterState
    {
        #region Properties

        public sealed override ECreatureState StateID => ECreatureState.Attack;

        #endregion Properties

        #region Constructor

        public AttackState(Monster controller) : base(controller) { }

        #endregion Constructor
    }
}