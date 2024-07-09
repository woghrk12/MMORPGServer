using System.Collections.Generic;

namespace Server
{
    public class SessionManager
    {
        #region  Variables

        private static SessionManager instance = new();

        private object lockObj = new();

        private int sessionId = 0;
        private Dictionary<int, ClientSession> sessionDict = new();

        #endregion Variables

        #region Properties

        public static SessionManager Instance => instance;

        #endregion Properties

        #region Methods

        public ClientSession Generate()
        {
            lock (lockObj)
            {
                int sessionId = ++this.sessionId;

                ClientSession session = new();
                session.SessionId = sessionId;

                sessionDict.Add(sessionId, session);

                Console.WriteLine($"Connected : {sessionId}");

                return session;
            }
        }

        public ClientSession Find(int sessionId)
        {
            lock (lockObj)
            {
                if (sessionDict.TryGetValue(sessionId, out ClientSession session) == false)
                {
                    return null;
                }

                return session;
            }
        }

        public void Remove(ClientSession session)
        {
            lock (lockObj)
            {
                if (sessionDict.ContainsValue(session) == false) return;

                sessionDict.Remove(session.SessionId);
            }
        }

        #endregion Methods
    }
}