namespace Server.Game.MonsterAI
{
    public abstract class Decision
    {
        #region Variables

        protected Monster controller = null;

        #endregion Variables

        #region Properties

        public EMonsterState FallbackState { get; }

        #endregion Properties

        #region Constructor

        public Decision(Monster controller, EMonsterState fallbackState)
        {
            this.controller = controller;

            FallbackState = fallbackState;
        }

        #endregion Constructor

        #region Methods

        public abstract bool Decide();

        #endregion Methods
    }
}