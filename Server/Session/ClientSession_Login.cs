using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.DB;

namespace Server
{
    public partial class ClientSession
    {
        public void Login(string uniqueID)
        {
            // TODO : Security check
            if (clientState != EClientState.Disconnected) return;

            // TODO : Add error handling logic for login failures
            using (AppDBContext db = new AppDBContext())
            {
                AccountDB account = db.Accounts
                    .Include(a => a.Players)
                    .Where(a => a.Name == uniqueID).FirstOrDefault();

                if (ReferenceEquals(account, null) == true)
                {
                    LoginResponse loginResponsePacket = new() { ResultCode = 1 };
                    Send(loginResponsePacket);
                }
                else
                {
                    db.Accounts.Add(new AccountDB() { Name = uniqueID });
                    db.SaveChanges();

                    LoginResponse loginResponsePacket = new() { ResultCode = 1 };
                    Send(loginResponsePacket);
                }
            }
        }
    }
}