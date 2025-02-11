using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Game
{
    public class Weapon : Item
    {
        #region Properties

        public override EItemType Type => EItemType.ItemTypeWeapon;

        public override bool IsStackable => false;

        public EWeaponType WeaponType { private set; get; } = EWeaponType.WeaponTypeNone;

        public int Value { private set; get; } = 0;

        #endregion Properties

        #region Constructor

        public Weapon(int id, int templateID) : base(id, templateID)
        {
            if (DataManager.ItemStatDictionary.TryGetValue(templateID, out ItemStat stat) == false) return;
            if (stat.ItemType != EItemType.ItemTypeWeapon) return;

            WeaponStat weaponStat = stat as WeaponStat;

            Count = 1;
            WeaponType = weaponStat.Type;
            Value = weaponStat.Value;
        }

        #endregion Constructor
    }
}