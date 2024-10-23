namespace Server.Game.MonsterState
{
    public abstract class MonsterState : State
    {
        #region Variables

        protected Monster controller = null;

        #endregion Variables

        #region Constructor

        public MonsterState(Monster controller)
        {
            this.controller = controller;
        }

        #endregion Constructor
    }
}