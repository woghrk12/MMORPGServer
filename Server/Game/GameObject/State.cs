using Google.Protobuf.Protocol;

namespace Server.Game
{
    public abstract class State
    {
        #region Properties

        public abstract ECreatureState StateID { get; }

        #endregion Properties

        #region Methods

        public virtual void OnEnter() { }

        public virtual void OnUpdate() { }

        public virtual void OnExit() { }

        #endregion Methods
    }
}