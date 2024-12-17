namespace Server.Game.MonsterAI
{
    /// <summary>
    /// Determines if the target is out of a specified range of the monster.
    /// </summary>
    public class TargetOutOfRangeDecision : Decision
    {
        #region Variables

        private int range = 0;

        #endregion Variables

        #region Constructor

        public TargetOutOfRangeDecision(Monster controller, EMonsterState targetState, int range) : base(controller, targetState)
        {
            this.range = range;
        }

        #endregion Constructor

        #region Methods

        public override bool Decide()
        {
            GameRoom room = controller.Room;
            if (ReferenceEquals(room, null) == true) return true;

            Character target = controller.Target;
            if (ReferenceEquals(target, null) == true) return true;

            if (ReferenceEquals(room, target.Room) == false) return true;

            if (Utility.CalculateDistance(controller.Position, target.Position) <= range)
            {
                controller.TargetPos = target.Position;
                return false;
            }

            return true;
        }

        #endregion Methods
    }
}