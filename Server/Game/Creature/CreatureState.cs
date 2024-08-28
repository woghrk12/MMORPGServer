using Google.Protobuf.Protocol;

namespace Server.Game.Creature
{
    public abstract class CreatureState
    {
        #region Properties

        public abstract ECreatureState StateID { get; }

        #endregion Properties

        #region Methods

        public virtual void OnStart() { }

        public virtual void OnUpdate() { }

        public virtual void OnEnd() { }

        #endregion Methods
    }
}