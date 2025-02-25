using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Game
{
    public class Weapon : Item
    {
        #region Properties

        public override EItemType ItemType => EItemType.ItemTypeEquipment;
        public EEquipmentType EquipmentType => EEquipmentType.EquipmentTypeWeapon;
        public EWeaponType WeaponType { private set; get; } = EWeaponType.WeaponTypeNone;

        public override bool IsStackable => false;

        public int Value { private set; get; } = 0;

        #endregion Properties

        #region Constructor

        public Weapon(int id, int templateID) : base(id, templateID)
        {
            if (DataManager.ItemStatDictionary.TryGetValue(templateID, out ItemStat stat) == false) return;

            WeaponStat weaponStat = stat as WeaponStat;

            if (ReferenceEquals(weaponStat, null) == true) return;
            if (weaponStat.ItemType != EItemType.ItemTypeEquipment) return;
            if (weaponStat.EquipmentType != EEquipmentType.EquipmentTypeWeapon) return;

            Count = 1;
            WeaponType = weaponStat.WeaponType;
            Value = weaponStat.Value;
        }

        #endregion Constructor
    }
}