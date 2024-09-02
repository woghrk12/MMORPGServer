using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class IdleState<T> : CreatureState where T : Creature
    {
        #region Constructor

        protected T controller = null;

        #endregion Constructor

        #region Properties

        public sealed override ECreatureState StateID => ECreatureState.Idle;

        #endregion Properties

        #region Constructor

        public IdleState(T controller)
        {
            this.controller = controller;
        }

        #endregion Constructor
    }
}