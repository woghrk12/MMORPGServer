using System;
using System.Net;
using Google.Protobuf;

using ServerCore;

namespace Server
{
    public class Program
    {
        #region Variables

        private static Listener listener = new();

        #endregion Variables

        private static void Main(string[] args)
        {
            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, GlobalDefine.PORT_NUMBER);

            listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine("Listening...");

            while (true)
            {
                TaskTimer.Instance.Flush();
            }
        }
    }
}