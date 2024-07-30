using System;
using System.Collections.Generic;

namespace Server
{
    public class SessionManager
    {
        #region Variables

        private static SessionManager instance = new();

        private object lockObj = new();

        private int sessionID = 0;
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
                ClientSession session = new(++sessionID);
                sessionDict.Add(session.SessionID, session);

                return session;
            }
        }

        public ClientSession Find(int id)
        {
            lock (lockObj)
            {
                return sessionDict.TryGetValue(id, out ClientSession session) == true ? session : null;
            }
        }

        public void Remove(ClientSession session)
        {
            lock (lockObj)
            {
                sessionDict.Remove(session.SessionID);
            }
        }

        #endregion Methods
    }
}
