using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Game
{
    public class Consumable : Item
    {
        #region Properties

        public override EItemType Type => EItemType.ItemTypeConsumable;

        public override bool IsStackable => MaxCount > 1;

        public EConsumableType ConsumableType { private set; get; } = EConsumableType.ConsumableTypeNone;

        public int Value { private set; get; } = 0;

        public int MaxCount { private set; get; } = 0;

        #endregion Properties

        #region Constructor

        public Consumable(int id, int templateID, int count = 1) : base(id, templateID)
        {
            if (DataManager.ItemStatDictionary.TryGetValue(templateID, out ItemStat stat) == false) return;
            if (stat.ItemType != EItemType.ItemTypeConsumable) return;

            ConsumableStat consumableStat = stat as ConsumableStat;

            Count = count;
            ConsumableType = consumableStat.Type;
            Value = consumableStat.Value;
            MaxCount = consumableStat.MaxCount;
        }

        #endregion Constructor
    }
}