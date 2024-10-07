using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class ObjectManager
    {
        #region Variables

        private object lockObj = new();

        private Dictionary<int, GameObject> objectDictionary = new();
        private int counter = 1;

        #endregion Variables

        #region Properties

        public static ObjectManager Instance { get; } = new();

        #endregion Properties

        #region Methods

        public static EGameObjectType GetObjectTypeByID(int objectID) => (EGameObjectType)(objectID >> 24 & 0x7F);

        public T Add<T>() where T : GameObject, new()
        {
            T gameObject = null;

            lock (lockObj)
            {
                gameObject = new();
                gameObject.ID = GenerateID(gameObject.ObjectType);

                // TODO : Name needs to be set based on the nickname stored in the database or the name from the data sheet
                gameObject.Name = gameObject.ObjectType.ToString() + "_" + gameObject.ID;

                objectDictionary.Add(gameObject.ID, gameObject);
            }

            return gameObject;
        }

        public bool Remove(int objectID)
        {
            lock (lockObj)
            {
                return objectDictionary.Remove(objectID);
            }
        }

        public GameObject Find(int objectID)
        {
            lock (lockObj)
            {
                return objectDictionary.TryGetValue(objectID, out GameObject gameObject) == true ? gameObject : null;
            }
        }

        private int GenerateID(EGameObjectType type) => (int)type << 24 | counter++;

        #endregion Methods
    }
}