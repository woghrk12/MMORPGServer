using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Monster : GameObject
    {
        #region Constructor

        public Monster()
        {
            ObjectType = EGameObjectType.Monster;
            IsCollidable = true;

            if (DataManager.ObjectStatDictionary.TryGetValue(2, out Data.ObjectStat statData) == false) return;

            // TODO : The stat needs to be adjusted based on the player's level
            Stat.CurHP = Stat.MaxHP = statData.MaxHpDictionary[1];
            Stat.AttackPower = statData.AttackPowerDictionary[1];
        }

        #endregion Constructor
    }
}