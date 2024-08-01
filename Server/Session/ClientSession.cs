using Google.Protobuf;
using System.Net;
using ServerCore;
using Google.Protobuf.Protocol;

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

        public void Send(IMessage packet)
        {
            string packetName = packet.Descriptor.Name;
            EMessageID packetID = (EMessageID)Enum.Parse(typeof(EMessageID), packetName);
            ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];

            Array.Copy(BitConverter.GetBytes(size + 4), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort)packetID), 0, sendBuffer, 2, sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

            Send(new ArraySegment<byte>(sendBuffer));
        }

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