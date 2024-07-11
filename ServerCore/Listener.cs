using System.Net;
using System.Net.Sockets;

namespace ServerCore
{
    public class Listener
    {
        #region  Variables

        private Socket listenSocket = null;

        private Func<Session> sessionFactory = null;

        #endregion Variables

        #region  Methods

        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory, int register = 10, int backlog = 100)
        {
            listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.sessionFactory += sessionFactory;

            listenSocket.Bind(endPoint);

            // Backlog : the maximum number of clients requesting connection to the server
            listenSocket.Listen(backlog);

            for (int index = 0; index < register; index++)
            {
                SocketAsyncEventArgs args = new();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
                RegisterAccept(args);
            }
        }

        private void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

            if (listenSocket.AcceptAsync(args)) return;

            OnAcceptCompleted(null, args);
        }

        #region Events

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = sessionFactory?.Invoke();
                session.Init(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }

            RegisterAccept(args);
        }

        #endregion Events

        #endregion Methods
    }
}