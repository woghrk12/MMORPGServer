using Google.Protobuf.Protocol;

namespace Server.Game
{
    public abstract class CreatureState
    {
        #region Properties

        public abstract ECreatureState StateID { get; }

        #endregion Properties

        #region Methods

        public virtual void OnStart() { }

        public virtual void OnUpdate(float deltaTime) { }

        public virtual void OnEnd() { }

        #endregion Methods
    }
}