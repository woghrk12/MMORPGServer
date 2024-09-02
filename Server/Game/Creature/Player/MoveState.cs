namespace Server.Game.PlayerState
{
    public class MoveState : MoveState<Player>
    {
        #region Constructor

        public MoveState(Player controller) : base(controller) { }

        #endregion Constructor

        #region Methods

        protected override void SetNextPos()
        {
            Player controller = this.controller;
            if (ReferenceEquals(controller, null) == true) return;

            controller.MoveDirection = controller.InputDirection;

            GameRoom room = controller.Room;
            if (ReferenceEquals(room, null) == true) return;

            room.MoveCreature(controller.ID, controller.MoveDirection);
        }

        #endregion Methods
    }
}