using Newtonsoft.Json;

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
        public static Dictionary<int, Data.MonsterStat> MonsterStatDictionary { private set; get; } = new();
        public static Dictionary<int, Data.AttackStat> AttackStatDictionary { private set; get; } = new();
        public static Dictionary<int, Data.ProjectileStat> ProjectileStatDictionary { private set; get; } = new();
        public static Dictionary<int, Data.ItemStat> ItemStatDictionary { private set; get; }

        #endregion Properties

        #region Methods

        public static void LoadData()
        {
            ObjectStatDictionary = LoadJson<Data.ObjectStatData, int, Data.ObjectStat>("ObjectStatData").MakeDictionary();
            MonsterStatDictionary = LoadJson<Data.MonsterStatData, int, Data.MonsterStat>("MonsterStatData").MakeDictionary();
            AttackStatDictionary = LoadJson<Data.AttackStatData, int, Data.AttackStat>("AttackStatData").MakeDictionary();
            ProjectileStatDictionary = LoadJson<Data.ProjectileStatData, int, Data.ProjectileStat>("ProjectileStatData").MakeDictionary();
            ItemStatDictionary = LoadJson<Data.ItemStatData, int, Data.ItemStat>("ItemStatData").MakeDictionary();
        }

        private static Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
        {
            string text = File.ReadAllText($"{Data.ConfigManager.Config.DataPath}/{path}.json");

            return JsonConvert.DeserializeObject<Loader>(text);
        }

        #endregion Methods
    }
}