using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class MoveState<T> : CreatureState where T : Creature
    {
        #region Constructor

        protected T controller = null;

        #endregion Constructor

        #region Properties

        public sealed override ECreatureState StateID => ECreatureState.Move;

        #endregion Properties

        #region Constructor

        public MoveState(T controller)
        {
            this.controller = controller;
        }

        #endregion Constructor

        #region Methods

        protected virtual void SetNextPos() { }

        public override void OnUpdate(float deltaTime)
        {
            Vector2 destPos = new Vector2(controller.CellPos.X, controller.CellPos.Y);
            Vector2 moveVector = destPos - controller.CurPos;

            if (moveVector.SqrMagnitude < controller.MoveSpeed * deltaTime * controller.MoveSpeed * deltaTime)
            {
                controller.CurPos = destPos;
                SetNextPos();
            }
            else
            {
                controller.CurPos += controller.MoveSpeed * deltaTime * moveVector.Normalized;
            }
        }

        #endregion Methods
    }
}