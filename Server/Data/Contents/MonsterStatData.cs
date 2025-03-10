namespace Server.Data
{
    [Serializable]
    public class MonsterStat
    {
        public int ID;
        public string Name;
        public int MaxHP;
        public int AttackPower;
        public int PatrolRange;
        public int DetectionRange;
        public int ChaseRange;
        public int AttackID;
    }

    [Serializable]
    public class MonsterStatData : ILoader<int, MonsterStat>
    {
        #region Variables

        public List<MonsterStat> MonsterStatList = new();

        #endregion Variables

        #region Methods

        public Dictionary<int, MonsterStat> MakeDictionary()
        {
            Dictionary<int, MonsterStat> dictionary = new();

            foreach (MonsterStat stat in MonsterStatList)
            {
                dictionary.Add(stat.ID, stat);
            }

            return dictionary;
        }

        #endregion Methods
    }
}