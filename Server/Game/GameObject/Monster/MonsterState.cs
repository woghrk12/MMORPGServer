namespace Server.Game
{
    public abstract class MonsterState
    {
        #region Variables

        protected Monster controller = null;

        #endregion Variables

        #region Properties

        public abstract EMonsterState MonsterStateID { get; }

        #endregion Properties

        #region Constructor

        public MonsterState(Monster controller)
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