
namespace Server
{
    public class GameRoom
    {
        private object lockObj = new();

        private List<ClientSession> sessionList = new();

        public void Enter(ClientSession session)
        {
            lock (lockObj)
            {
                sessionList.Add(session);

                session.Room = this;
            }
        }

        public void Leave(ClientSession session)
        {
            lock (lockObj)
            {
                sessionList.Remove(session);
            }
        }

        public void BroadCast(ClientSession clientSession, string chat)
        {
            ServerChat packet = new();
            packet.playerId = clientSession.SessionId;
            packet.chat = chat;

            ArraySegment<byte> segment = packet.Write();

            lock (lockObj)
            {
                foreach (ClientSession session in sessionList)
                {
                    session.Send(segment);
                }
            }
        }
    }
}