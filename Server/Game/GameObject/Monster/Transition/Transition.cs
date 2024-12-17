namespace Server.Game.MonsterAI
{
    public class Transition
    {
        #region Variables

        private Monster controller = null;

        private EMonsterState targetState = EMonsterState.IDLE;
        private List<Decision> decisionList = null;

        #endregion Variables

        #region Constructor

        public Transition(Monster controller, EMonsterState targetState, List<Decision> decisionList)
        {
            this.controller = controller;

            this.targetState = targetState;
            this.decisionList = decisionList;
        }

        #endregion Constructor

        #region Methods

        public EMonsterState Decide()
        {
            if (ReferenceEquals(decisionList, null) == true) return EMonsterState.IDLE;

            foreach (Decision decision in decisionList)
            {
                if (decision.Decide() == true) continue;

                return decision.FallbackState;
            }

            return targetState;
        }

        #endregion Methods
    }
}