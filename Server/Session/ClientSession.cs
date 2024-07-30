using System.Net;
using ServerCore;

namespace Server
{
    public class ClientSession : PacketSession
    {
        #region Properties

        public int SessionID { private set; get; }

        #endregion Properties

        #region Constructor

        public ClientSession(int sessionID)
        {
            SessionID = sessionID;
        }

        #endregion Constructor

        #region Methods

        #region Events

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Session ID : {SessionID}\nOnConnected : {endPoint}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this);

            Console.WriteLine($"Session ID : {SessionID}\nOnDisconnected : {endPoint}");
        }

        public override void OnReceivePacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnReceivePacket(this, buffer);
        }

        public override void OnSend(int numBytes)
        {

        }

        #endregion Events

        #endregion Methods
    }
}