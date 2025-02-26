using Google.Protobuf.Protocol;

namespace Server.Game.MonsterAI
{
    /// <summary>
    /// Check if there is no target or if there is no character within a specified range to select as the target.
    /// </summary>
    public class NoTargetDecision : Decision
    {
        #region Variables

        private int range = 0;

        #endregion Variables

        #region Constructor

        public NoTargetDecision(Monster controller, EMonsterState targetState, int range) : base(controller, targetState)
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
            if (ReferenceEquals(target, null) == false && target.CurState != ECreatureState.Dead)
            {
                if (room.Map.FindPath(controller.Position, target.Position, out List<Pos> path, range) == true) return false;

                controller.Target = null;
                return true;
            }

            Character newTarget = room.FindCharacter(c => c.CurState != ECreatureState.Dead && room.Map.FindPath(controller.Position, c.Position, out List<Pos> path, range));
            if (ReferenceEquals(newTarget, null) == true) return true;

            controller.Target = newTarget;
            controller.TargetPos = newTarget.Position;

            return false;
        }

        #endregion Methods
    }
}