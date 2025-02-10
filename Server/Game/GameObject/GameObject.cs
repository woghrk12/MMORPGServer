using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class GameObject
    {
        #region Variables

        private object lockObj = new();

        private event Action updated = null;

        #endregion Variables

        #region Properties

        public GameRoom Room { set; get; }

        /// <summary>
        /// <para>The ID that refers to the GameObject existing in the game room.</para>
        /// [UNUSED(1)][TYPE(7)][ID(24)] 
        /// </summary>
        public int ID { set; get; } = -1;

        public string Name { set; get; } = string.Empty;

        public EGameObjectType ObjectType { protected set; get; }

        public Pos Position { set; get; } = new Pos(0, 0);

        public bool IsCollidable { set; get; } = true;

        public event Action Updated { add { updated += value; } remove { updated -= value; } }

        #endregion Properties

        #region Methods

        public void Update()
        {
            lock (lockObj)
            {
                OnUpdate();
            }
        }

        #region Events

        protected virtual void OnUpdate()
        {
            updated?.Invoke();
        }

        #endregion Events

        #endregion Methods
    }
}