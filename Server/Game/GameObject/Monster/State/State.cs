namespace Server.Game.MonsterAI
{
    public abstract class State
    {
        #region Variables

        protected Monster controller = null;

        #endregion Variables

        #region Properties

        public abstract EMonsterState MonsterStateID { get; }
        public virtual bool IsTransitionBlocked { get; } = false;

        #endregion Properties

        #region Constructor

        public State(Monster controller)
        {
            this.controller = controller;
        }

        #endregion Constructor

        #region Methods

        public virtual void OnEnter() { }

        public virtual void OnUpdate() { }

        public virtual void OnExit() { }

        #endregion Methods
    }
}