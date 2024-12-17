namespace Server.Game.MonsterAI
{
    public class Transition
    {
        #region Variables

        private Monster controller = null;

        private EMonsterState fallbackState = EMonsterState.IDLE;
        private List<Decision> decisionList = null;

        #endregion Variables

        #region Constructor

        public Transition(Monster controller, EMonsterState fallbackState, List<Decision> decisionList)
        {
            this.controller = controller;

            this.fallbackState = fallbackState;
            this.decisionList = decisionList;
        }

        #endregion Constructor

        #region Methods

        public EMonsterState Evaluate()
        {
            if (ReferenceEquals(decisionList, null) == true) return EMonsterState.IDLE;

            foreach (Decision decision in decisionList)
            {
                if (decision.Decide() == false) continue;

                return decision.TargetState;
            }

            return fallbackState;
        }

        #endregion Methods
    }
}