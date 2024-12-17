namespace Server.Game.MonsterAI
{
    /// <summary>
    /// Checks if no navigable path to the target exists within a specified range.
    /// </summary>
    public class PathOutOfRangeDecision : Decision
    {
        #region Variables

        private int range = 0;

        #endregion Variables

        #region Constructor

        public PathOutOfRangeDecision(Monster controller, EMonsterState targetState, int range) : base(controller, targetState)
        {
            this.range = range;
        }

        #endregion Constructor

        #region Methods

        public override bool Decide()
        {
            GameRoom room = controller.Room;
            if (ReferenceEquals(room, null) == true)
            {
                controller.Target = null;
                return true;
            }

            Character target = controller.Target;
            if (ReferenceEquals(target, null) == true) return true;

            if (ReferenceEquals(room, target.Room) == false)
            {
                controller.Target = null;
                return true;
            }

            if (room.Map.FindPath(controller.Position, target.Position, out List<Pos> path, range) == false)
            {
                controller.Target = null;
                return true;
            }

            controller.TargetPos = target.Position;
            return false;
        }

        #endregion Methods
    }
}