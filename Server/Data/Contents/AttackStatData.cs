using Google.Protobuf.Protocol;

namespace Server.Data
{
    [Serializable]
    public class AttackStat
    {
        public int ID;
        public string Name;
        public string AnimationKey;
        public float Cooldown;
        public float PostDelay;
        public Dictionary<int, int> CoeffDictionary;
        public EAttackType AttackType;
        public int Range;
        public int ProjectileID;
    }

    [Serializable]
    public class AttackStatData : ILoader<int, AttackStat>
    {
        #region Variables

        public List<AttackStat> AttackStatList = new();

        #endregion Variables

        #region Methods

        public Dictionary<int, AttackStat> MakeDictionary()
        {
            Dictionary<int, AttackStat> dictionary = new();

            foreach (AttackStat stat in AttackStatList)
            {
                dictionary.Add(stat.ID, stat);
            }

            return dictionary;
        }

        #endregion Methods
    }
}