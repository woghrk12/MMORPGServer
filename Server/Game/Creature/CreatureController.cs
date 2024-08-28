using Google.Protobuf.Protocol;

namespace Server.Game.Creature
{
    public class CreatureController
    {
        #region Variables

        private Dictionary<ECreatureState, CreatureState> stateDictionary = new();
        private CreatureState curState = null;


        #endregion Variables
    }
}