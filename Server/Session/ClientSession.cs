using System.Net;
using System.Runtime.InteropServices;
using Google.Protobuf;
using Google.Protobuf.Protocol;
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
            Console.WriteLine($"OnConnected : {endPoint}");
            // Proto Test
            PlayerMoveBrodcast packet = new PlayerMoveBrodcast()
            {
                PlayerID = SessionID,
                Direction = 1
            };

            ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];
            Array.Copy(BitConverter.GetBytes(size + 4), 0, sendBuffer, 0, sizeof(ushort));
            ushort protocolID = (ushort)EMessageID.PlayerMoveBrodcast;
            Array.Copy(BitConverter.GetBytes(protocolID), 0, sendBuffer, 2, sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

            Send(new ArraySegment<byte>(sendBuffer));
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this);

            Console.WriteLine($"OnDisconnected : {endPoint}");
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