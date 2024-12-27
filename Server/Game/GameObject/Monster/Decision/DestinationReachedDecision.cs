namespace Server.Game.MonsterAI
{
    /// <summary>
    /// Check if the monster has reached its destination.
    /// </summary>
    public class DestinationReachedDecision : Decision
    {
        #region Constructor

        public DestinationReachedDecision(Monster controller, EMonsterState targetState) : base(controller, targetState) { }

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

            if (controller.TargetPos == controller.Position) return true;
            if (room.Map.FindPath(controller.Position, controller.TargetPos, out List<Pos> path) == false) return true;

            controller.NextPos = path[1];

            return false;
        }

        #endregion Methods
    }
}