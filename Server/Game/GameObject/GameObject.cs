using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class GameObject
    {
        #region Variables

        private event Action updated = null;

        #endregion Variables

        #region Properties

        public GameRoom Room { set; get; }

        // [UNUSED(1)][TYPE(7)][ID(24)]
        public int ID { set; get; } = -1;

        public string Name { set; get; } = string.Empty;

        public EGameObjectType ObjectType { protected set; get; }

        public Pos Position { set; get; } = new Pos(0, 0);

        public bool IsCollidable { set; get; } = true;

        public event Action Updated { add { updated += value; } remove { updated -= value; } }

        #endregion Properties

        #region Methods

        #region Events

        public virtual void OnUpdate()
        {
            updated?.Invoke();
        }

        #endregion Events

        #endregion Methods
    }
}