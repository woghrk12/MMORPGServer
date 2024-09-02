using System.Net;
using Server.Game;

namespace Server
{
    public class Program
    {
        #region Variables

        private static Listener listener = new();

        #endregion Variables

        private static void Main(string[] args)
        {
            RoomManager.Instance.Add(1);

            // DNS (Domain Name System)
            IPAddress ipAddr = IPAddress.Parse(GlobalDefine.IP_ADDRESS);
            IPEndPoint endPoint = new IPEndPoint(ipAddr, GlobalDefine.PORT_NUMBER);

            listener.Init(endPoint, SessionManager.Instance.Generate);
            Console.WriteLine("Listening...");

            int tickCount = 100;
            while (true)
            {
                RoomManager.Instance.Find(1).OnUpdate(tickCount / 1000f);

                Thread.Sleep(tickCount);
            }
        }
    }
}