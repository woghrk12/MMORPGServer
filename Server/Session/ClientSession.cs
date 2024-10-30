using Google.Protobuf.Protocol;
using Server.Game;
using System.Net.Sockets;

namespace Server
{
    public partial class ClientSession
    {
        #region Properties

        public int SessionID { private set; get; }

        public EClientState clientState { private set; get; } = EClientState.Disconnected;

        public Player Player { private set; get; }

        #endregion Properties

        #region Constructor

        public ClientSession(int sessionID)
        {
            SessionID = sessionID;
        }

        #endregion Constructor

        #region Methods

        public void Init(Socket socket)
        {
            this.socket = socket;

            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveCompleted);
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            OnConnected(this.socket.RemoteEndPoint);

            RegisterRecv();
        }


        #endregion Methods
    }
}