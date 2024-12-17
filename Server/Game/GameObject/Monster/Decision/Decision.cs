namespace Server.Game.MonsterAI
{
    public abstract class Decision
    {
        #region Variables

        protected Monster controller = null;

        #endregion Variables

        #region Properties

        public EMonsterState TargetState { get; }

        #endregion Properties

        #region Constructor

        public Decision(Monster controller, EMonsterState targetState)
        {
            this.controller = controller;

            TargetState = targetState;
        }

        #endregion Constructor

        #region Methods

        public abstract bool Decide();

        #endregion Methods
    }
}