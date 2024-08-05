namespace Server.Game
{
    public class PlayerManager
    {
        #region Variables

        private object lockObj = new();

        private Dictionary<int, Player> playerDict = new();
        private int playerID = 1;

        #endregion Variables

        #region Properties

        public static PlayerManager Instance { get; } = new();

        #endregion Properties

        #region Methods

        public Player Add()
        {
            Player player = new();

            lock (lockObj)
            {
                player.Info.PlayerID = playerID++;

                playerDict.Add(player.Info.PlayerID, player);
            }

            return player;
        }

        public bool Remove(int playerID)
        {
            lock (lockObj)
            {
                return playerDict.Remove(playerID);
            }
        }

        public Player Find(int playerID)
        {
            lock (lockObj)
            {
                return playerDict.TryGetValue(playerID, out Player player) == true ? player : null;
            }
        }

        #endregion Methods
    }
}