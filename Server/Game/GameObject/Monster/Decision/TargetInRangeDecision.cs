namespace Server.Game.MonsterAI
{
    /// <summary>
    /// Determines if the target is within a specified range of the monster.
    /// </summary>
    public class TargetInRangeDecision : Decision
    {
        #region Variables

        private int range = 0;

        #endregion Variables

        #region Constructor

        public TargetInRangeDecision(Monster controller, EMonsterState targetState, int range) : base(controller, targetState)
        {
            this.range = range;
        }

        #endregion Constructor

        #region Methods

        public override bool Decide()
        {
            GameRoom room = controller.Room;
            if (ReferenceEquals(room, null) == true) return false;

            Character target = controller.Target;
            if (ReferenceEquals(target, null) == true) return false;

            if (ReferenceEquals(room, target.Room) == false)
            {
                controller.Target = null;
                return false;
            }

            if (Utility.CalculateDistance(controller.Position, target.Position) > range) return false;

            controller.TargetPos = target.Position;
            return true;
        }

        #endregion Methods
    }
}