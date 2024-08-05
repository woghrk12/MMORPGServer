namespace Server.Game
{
    public class RoomManager
    {
        #region Variables

        private object lockObj = new();

        private Dictionary<int, GameRoom> roomDict = new();
        private int roomID = 1;

        #endregion Variables

        #region Properties

        public static RoomManager Instance { get; } = new();

        #endregion Properties

        #region Methods

        public GameRoom Add()
        {
            GameRoom room = new();

            lock (lockObj)
            {
                room.RoomID = roomID++;

                roomDict.Add(room.RoomID, room);
            }

            return room;
        }

        public bool Remove(int roomID)
        {
            lock (lockObj)
            {
                return roomDict.Remove(roomID);
            }
        }

        public GameRoom Find(int roomID)
        {
            lock (lockObj)
            {
                return roomDict.TryGetValue(roomID, out GameRoom room) == true ? room : null;
            }
        }

        #endregion Methods
    }
}