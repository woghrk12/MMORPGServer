using Newtonsoft.Json;

namespace Server.Data
{
    [Serializable]
    public class ServerConfig
    {
        public string IPAddress;
        public int PortNumber;

        public string DataPath;
        public string ConnectionString;
    }

    public class ConfigManager
    {
        #region Properties

        public static ServerConfig Config { private set; get; }

        #endregion Properties

        #region Methods

        public static void LoadConfig()
        {
            Config = JsonConvert.DeserializeObject<ServerConfig>(File.ReadAllText("config.json"));
        }

        #endregion Methods
    }
}