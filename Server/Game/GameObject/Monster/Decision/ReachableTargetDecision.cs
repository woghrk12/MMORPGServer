namespace Server.Game.MonsterAI
{
    /// <summary>
    /// Determines if there is a target within a specified range for which a navigable path exists.
    /// </summary>
    public class ReachableTargetDecision : Decision
    {
        #region Variables

        private int range = 0;

        #endregion Variables

        #region Constructor

        public ReachableTargetDecision(Monster controller, EMonsterState targetState, int range) : base(controller, targetState)
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
                return false;
            }

            Character target = controller.Target;
            if (ReferenceEquals(target, null) == true) return false;

            if (room.Map.FindPath(controller.Position, target.Position, out List<Pos> path, range) == true)
            {
                controller.TargetPos = target.Position;
                return true;
            }

            controller.Target = null;
            return false;
        }

        #endregion Methods
    }
}