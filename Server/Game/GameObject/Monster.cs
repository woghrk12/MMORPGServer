using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Monster : GameObject
    {
        #region Constructor

        public Monster(int id) : base(id)
        {
            ObjectType = EGameObjectType.Monster;
        }

        #endregion Constructor
    }
}