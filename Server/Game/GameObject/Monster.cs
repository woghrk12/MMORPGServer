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
        }

        #endregion Constructor
    }
}