using System.Net;
using System.Net.Sockets;

namespace ServerCore
{
    public class Connector
    {
        #region Variables

        private Func<Session> sessionFactory = null;

        #endregion Variables

        #region Methods

        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory)
        {
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            SocketAsyncEventArgs args = new();

            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnectCompleted);
            args.RemoteEndPoint = endPoint;
            args.UserToken = socket;

            this.sessionFactory += sessionFactory;

            RegisterConnect(args);
        }

        private void RegisterConnect(SocketAsyncEventArgs args)
        {
            Socket socket = args.UserToken as Socket;

            if (ReferenceEquals(socket, null)) return;

            if (socket.ConnectAsync(args)) return;

            OnConnectCompleted(null, args);
        }

        #region Events

        private void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = sessionFactory?.Invoke();
                
                session.Init(args.ConnectSocket);
                session.OnConnected(args.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine($"OnConnectedCompleted Failed. {args.SocketError}");
            }
        }

        #endregion Events

        #endregion Methods
    }
}