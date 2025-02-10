using System.Net;
using Server.Data;
using Server.DB;
using Server.Game;

namespace Server
{
    public class Program
    {
        #region Variables

        private static Listener listener = new();

        private static List<System.Timers.Timer> roomTimer = new();

        #endregion Variables

        private static void Main(string[] args)
        {
            ConfigManager.LoadConfig();
            DataManager.LoadData();

            GameRoom room = RoomManager.Instance.Add(1);
            UpdateRoom(room, 50);

            // DNS (Domain Name System)
            IPAddress ipAddr = IPAddress.Parse(ConfigManager.Config.IPAddress);
            IPEndPoint endPoint = new IPEndPoint(ipAddr, ConfigManager.Config.PortNumber);

            listener.Init(endPoint, SessionManager.Instance.Generate);
            Console.WriteLine("Listening...");

            while (true)
            {
                DBTransaction.Instance.Flush();
            }
        }

        private static void UpdateRoom(GameRoom room, int tick = 100)
        {
            var timer = new System.Timers.Timer();
            timer.Interval = tick;
            timer.Elapsed += (s, e) => room.Update();
            timer.AutoReset = true;
            timer.Enabled = true;

            roomTimer.Add(timer);
        }
    }
}