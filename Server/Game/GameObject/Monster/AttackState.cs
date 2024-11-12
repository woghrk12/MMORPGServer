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

        #region Methods

        public override void OnEnter()
        {
            GameRoom room = controller.Room;
            if (ReferenceEquals(room, null) == true) return;

            room.Broadcast(new UpdateCreatureStateBroadcast() { CreatureID = controller.ID, NewState = ECreatureState.Attack });
        }

        #endregion Methods
    }
}