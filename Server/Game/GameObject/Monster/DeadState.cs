using Google.Protobuf.Protocol;

namespace Server.Game.MonsterState
{
    public class DeadState : MonsterState
    {
        #region Properties

        public sealed override ECreatureState StateID => ECreatureState.Dead;

        #endregion Properties

        #region Constructor

        public DeadState(Monster controller) : base(controller) { }

        #endregion Constructor

        #region Methods

        public override void OnEnter()
        {
            GameRoom room = controller.Room;
            if (ReferenceEquals(room, null) == true) return;

            room.Broadcast(new UpdateCreatureStateBroadcast() { CreatureID = controller.ID, NewState = ECreatureState.Dead });
        }

        #endregion Methods
    }
}