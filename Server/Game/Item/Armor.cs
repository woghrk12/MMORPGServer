using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Game
{
    public class Armor : Item
    {
        #region Properties

        public override EItemType Type => EItemType.ItemTypeArmor;

        public override bool IsStackable => false;

        public EArmorType ArmorType { private set; get; } = EArmorType.ArmorTypeNone;

        public int Value { private set; get; } = 0;

        #endregion Properties

        #region Constructor

        public Armor(int id, int templateID) : base(id, templateID)
        {
            if (DataManager.ItemStatDictionary.TryGetValue(templateID, out ItemStat stat) == false) return;
            if (stat.ItemType != EItemType.ItemTypeArmor) return;

            ArmorStat armorStat = stat as ArmorStat;

            Count = 1;
            ArmorType = armorStat.Type;
            Value = armorStat.Value;
        }

        #endregion Constructor
    }
}