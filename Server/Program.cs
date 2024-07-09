using System.Net;
using ServerCore;

namespace Server
{
    public class Program
    {
        private static Listener listener = new();

        public static GameRoom Room = new();

        private static void Main(string[] args)
        {
            PacketManager.Instance.Register();

            // DNF (Domain Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new(ipAddr, 7777);

            listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });

            while (true) ;
        }
    }
}