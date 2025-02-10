using Google.Protobuf.Protocol;

namespace Server.Data
{
    [Serializable]
    public class ItemStat
    {
        public int ID;
        public string Name;

        public EItemType ItemType;
    }

    public class WeaponStat : ItemStat
    {
        public EWeaponType Type;
        public int Value;
    }

    public class ArmorStat : ItemStat
    {
        public EArmorType Type;
        public int Value;
    }

    public class ConsumableStat : ItemStat
    {
        public EConsumableType Type;
        public int Value;
        public int MaxCount;
    }

    [Serializable]
    public class ItemStatData : ILoader<int, ItemStat>
    {
        #region Variables

        public List<WeaponStat> WeaponStatList = new();
        public List<ArmorStat> ArmorStatList = new();
        public List<ConsumableStat> ConsumableStatList = new();

        #endregion Variables

        #region Methods

        public Dictionary<int, ItemStat> MakeDictionary()
        {
            Dictionary<int, ItemStat> dictionary = new();

            foreach (WeaponStat stat in WeaponStatList)
            {
                stat.ItemType = EItemType.ItemTypeWeapon;
                dictionary.Add(stat.ID, stat);
            }

            foreach (ArmorStat stat in ArmorStatList)
            {
                stat.ItemType = EItemType.ItemTypeArmor;
                dictionary.Add(stat.ID, stat);
            }

            foreach (ConsumableStat stat in ConsumableStatList)
            {
                stat.ItemType = EItemType.ItemTypeConsumable;
                dictionary.Add(stat.ID, stat);
            }

            return dictionary;
        }

        #endregion Methods
    }
}