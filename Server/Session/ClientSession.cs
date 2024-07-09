using System.Net;
using ServerCore;

namespace Server
{
    public class ClientSession : PacketSession
    {
        public int SessionId { set; get; }
        public GameRoom Room { set; get; }

        public override void OnConnected(EndPoint endPoint)
        {
            Program.Room.Enter(this);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this);

            if (ReferenceEquals(Room, null)) return;

            Room.Leave(this);
            Room = null;
        }

        public override void OnReceivePacket(ArraySegment<byte> buffer)
        {

        }

        public override void OnSend(int numBytes)
        {

        }
    }
}