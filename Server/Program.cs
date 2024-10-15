using System.Net;
using Server.Data;
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
            ConfigManager.LoadConfig();
            DataManager.LoadData();

            RoomManager.Instance.Add(1);

            // DNS (Domain Name System)
            IPAddress ipAddr = IPAddress.Parse(ConfigManager.Config.IPAddress);
            IPEndPoint endPoint = new IPEndPoint(ipAddr, ConfigManager.Config.PortNumber);

            listener.Init(endPoint, SessionManager.Instance.Generate);
            Console.WriteLine("Listening...");

            while (true)
            {
                GameRoom room = RoomManager.Instance.Find(1);

                room.Push(room.Update);
                Thread.Sleep(100);
            }
        }
    }
}