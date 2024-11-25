using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class AttackState : MonsterState
    {
        #region Variables

        private int attackID = -1;

        #endregion Variables

        #region Properties

        public override EMonsterState MonsterStateID => EMonsterState.ATTACK;

        #endregion Properties

        #region Constructor

        public AttackState(Monster controller, int attackID) : base(controller)
        {
            this.attackID = attackID;
        }

        #endregion Constructor

        #region Methods

        public override void OnEnter()
        {
            controller.CurState = ECreatureState.Attack;

            GameRoom room = controller.Room;
            if (ReferenceEquals(room, null) == true) return;

            room.PerformAttack(controller.ID, attackID);
        }

        #endregion Methods
    }
}