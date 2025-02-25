using Google.Protobuf.Protocol;

namespace Server.Data
{
    [Serializable]
    public class ItemStat
    {
        public int ID;
        public string Name;
        public EItemType ItemType;
        public string IconPath;
    }

    public class EquipmentStat : ItemStat
    {
        public EEquipmentType EquipmentType;
    }

    // [UNUSED(1)][ITEM_TYPE(3)][EQUIPMENT_TYPE(4)][WEAPON_TYPE(4)][ID(20)]
    public class WeaponStat : EquipmentStat
    {
        public EWeaponType WeaponType;
        public int Value;
    }

    // [UNUSED(1)][ITEM_TYPE(3)][EQUIPMENT_TYPE(4)][ARMOR_TYPE(4)][ID(20)]
    public class ArmorStat : EquipmentStat
    {
        public EArmorType ArmorType;
        public int Value;
    }

    // [UNUSED(1)][ITEM_TYPE(3)][CONSUMABLE_TYPE(8)][ID(20)]
    public class ConsumableStat : ItemStat
    {
        public EConsumableType ConsumableType;
        public int Value;
        public int MaxCount;
    }

    // [UNUSED(1)][ITEM_TYPE(3)][ID(28)]
    public class LootStat : ItemStat
    {
        public int MaxCount;
    }

    [Serializable]
    public class ItemStatData : ILoader<int, ItemStat>
    {
        #region Variables

        public List<WeaponStat> WeaponStatList = new();
        public List<ArmorStat> ArmorStatList = new();
        public List<ConsumableStat> ConsumableStatList = new();
        public List<LootStat> LootStatList = new();

        #endregion Variables

        #region Methods

        public Dictionary<int, ItemStat> MakeDictionary()
        {
            Dictionary<int, ItemStat> dictionary = new();

            foreach (WeaponStat stat in WeaponStatList)
            {
                stat.ItemType = EItemType.ItemTypeEquipment;
                stat.EquipmentType = EEquipmentType.EquipmentTypeWeapon;
                dictionary.Add(stat.ID, stat);
            }

            foreach (ArmorStat stat in ArmorStatList)
            {
                stat.ItemType = EItemType.ItemTypeEquipment;
                stat.EquipmentType = EEquipmentType.EquipmentTypeArmor;
                dictionary.Add(stat.ID, stat);
            }

            foreach (ConsumableStat stat in ConsumableStatList)
            {
                stat.ItemType = EItemType.ItemTypeConsumable;
                dictionary.Add(stat.ID, stat);
            }

            foreach (LootStat stat in LootStatList)
            {
                stat.ItemType = EItemType.ItemTypeLoot;
                dictionary.Add(stat.ID, stat);
            }

            return dictionary;
        }

        #endregion Methods
    }
}