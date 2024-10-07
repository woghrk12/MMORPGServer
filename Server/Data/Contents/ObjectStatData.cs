namespace Server.Data
{
    [Serializable]
    public class ObjectStat
    {
        public int ID;
        public string Name;
        public Dictionary<int, int> MaxHpDictionary;
        public Dictionary<int, int> AttackPowerDictionary;
    }

    [Serializable]
    public class ObjectStatData : ILoader<int, ObjectStat>
    {
        #region Variables

        public List<ObjectStat> ObjectStatList = new();

        #endregion Variables

        #region Methods

        public Dictionary<int, ObjectStat> MakeDictionary()
        {
            Dictionary<int, ObjectStat> dictionary = new();

            foreach (ObjectStat stat in ObjectStatList)
            {
                dictionary.Add(stat.ID, stat);
            }

            return dictionary;
        }

        #endregion Methods
    }
}