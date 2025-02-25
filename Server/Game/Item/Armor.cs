using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Game
{
    public class Armor : Item
    {
        #region Properties

        public override EItemType ItemType => EItemType.ItemTypeEquipment;
        public EEquipmentType EquipmentType => EEquipmentType.EquipmentTypeArmor;
        public EArmorType ArmorType { private set; get; } = EArmorType.ArmorTypeNone;

        public override bool IsStackable => false;

        public int Value { private set; get; } = 0;

        #endregion Properties

        #region Constructor

        public Armor(int id, int templateID) : base(id, templateID)
        {
            if (DataManager.ItemStatDictionary.TryGetValue(templateID, out ItemStat stat) == false) return;

            ArmorStat armorStat = stat as ArmorStat;

            if (ReferenceEquals(armorStat, null) == true) return;
            if (armorStat.ItemType != EItemType.ItemTypeEquipment) return;
            if (armorStat.EquipmentType != EEquipmentType.EquipmentTypeArmor) return;

            Count = 1;
            ArmorType = armorStat.ArmorType;
            Value = armorStat.Value;
        }

        #endregion Constructor
    }
}