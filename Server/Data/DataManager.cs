using Google.Protobuf.Protocol;
using Newtonsoft.Json;
using Server.Game;

namespace Server
{
    public interface ILoader<Key, Value>
    {
        public Dictionary<Key, Value> MakeDictionary();
    }

    public class DataManager
    {
        #region Properties

        public static Dictionary<int, Data.ObjectStat> ObjectStatDictionary { private set; get; } = new();
        public static Dictionary<int, Data.AttackStat> AttacklStatDictionary { private set; get; } = new();
        public static Dictionary<int, Data.ProjectileStat> ProjectileStatDictionary { private set; get; } = new();

        #endregion Properties

        #region Methods

        public static void LoadData()
        {
            ObjectStatDictionary = LoadJson<Data.ObjectStatData, int, Data.ObjectStat>("ObjectStatData").MakeDictionary();
            AttacklStatDictionary = LoadJson<Data.AttackStatData, int, Data.AttackStat>("AttackStatData").MakeDictionary();
            ProjectileStatDictionary = LoadJson<Data.ProjectileStatData, int, Data.ProjectileStat>("ProjectileStatData").MakeDictionary();
        }

        public static void SendData(Player player)
        {
            List<Data.ObjectStat> objectStatList = [.. ObjectStatDictionary.Values];

            StatDataBroadcast objectStatPacket = new()
            {
                DataType = EStatType.ObjectData,
                Data = JsonConvert.SerializeObject(objectStatList)
            };

            Console.WriteLine($"Send ObjectStatData to {player.ID} player. Data Type : {EStatType.ObjectData}. Data Count : {objectStatList.Count}");
            player.Session.Send(objectStatPacket);

            List<Data.AttackStat> attackStatList = [.. AttacklStatDictionary.Values];

            StatDataBroadcast attackStatPacket = new()
            {
                DataType = EStatType.AttackData,
                Data = JsonConvert.SerializeObject(attackStatList)
            };

            Console.WriteLine($"Send ObjectStatData to {player.ID} player. Data Type : {EStatType.AttackData}. Data Count : {attackStatList.Count}");
            player.Session.Send(attackStatPacket);

            List<Data.ProjectileStat> projectileStatList = [.. ProjectileStatDictionary.Values];

            StatDataBroadcast projectileStatPacket = new()
            {
                DataType = EStatType.ProjectileData,
                Data = JsonConvert.SerializeObject(projectileStatList)
            };

            Console.WriteLine($"Send ObjectStatData to {player.ID} player. Data Type : {EStatType.ProjectileData}. Data Count : {projectileStatList.Count}");
            player.Session.Send(projectileStatPacket);
        }

        private static Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
        {
            string text = File.ReadAllText($"{Data.ConfigManager.Config.DataPath}/{path}.json");

            return JsonConvert.DeserializeObject<Loader>(text);
        }

        #endregion Methods
    }
}