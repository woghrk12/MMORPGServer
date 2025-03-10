namespace Server.Data
{
    [Serializable]
    public class CharacterStat
    {
        public int ID;
        public string Name;
        public Dictionary<int, int> MaxHpDictionary;
        public Dictionary<int, int> AttackPowerDictionary;
    }

    [Serializable]
    public class CharacterStatData : ILoader<int, CharacterStat>
    {
        #region Variables

        public List<CharacterStat> CharacterStatList = new();

        #endregion Variables

        #region Methods

        public Dictionary<int, CharacterStat> MakeDictionary()
        {
            Dictionary<int, CharacterStat> dictionary = new();

            foreach (CharacterStat stat in CharacterStatList)
            {
                dictionary.Add(stat.ID, stat);
            }

            return dictionary;
        }

        #endregion Methods
    }
}