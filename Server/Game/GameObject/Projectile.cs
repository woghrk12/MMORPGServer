namespace Server.Game
{
    public class Projectile : GameObject
    {
        #region Properties

        public ClientSession Session { set; get; }

        #endregion Properties

        #region Constructor

        public Projectile()
        {
            IsCollidable = false;
        }

        #endregion Constructor
    }
}