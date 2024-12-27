using Google.Protobuf.Protocol;

namespace Server.Game.MonsterAI
{
    public class AttackState : State
    {
        #region Variables

        private int attackID = -1;

        private long nextAttackTicks = 0;
        private long postDelayTicks = 0;
        private long cooldownTicks = 0;

        #endregion Variables

        #region Properties

        public override EMonsterState MonsterStateID => EMonsterState.ATTACK;

        public override bool IsTransitionBlocked => nextAttackTicks >= Environment.TickCount64;

        #endregion Properties

        #region Constructor

        public AttackState(Monster controller, int attackID) : base(controller)
        {
            this.attackID = attackID;

            try
            {
                Data.AttackStat attackStat = DataManager.AttackStatDictionary[attackID];

                postDelayTicks = attackStat.PostDelayTicks;
                cooldownTicks = attackStat.CooldownTicks;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion Constructor

        #region Methods

        public override void OnEnter()
        {
            controller.CurState = ECreatureState.Attack;

            nextAttackTicks = 0;
        }

        public override void OnUpdate()
        {
            if (nextAttackTicks >= Environment.TickCount64) return;

            GameRoom room = controller.Room;
            if (ReferenceEquals(room, null) == true) return;

            nextAttackTicks = Environment.TickCount64 + postDelayTicks + cooldownTicks;

            room.PerformMonsterAttack(controller.ID, attackID);
        }

        #endregion Methods
    }
}