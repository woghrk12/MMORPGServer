using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Server.DB;

namespace Server
{
    public partial class ClientSession
    {
        #region Properties

        public int AccountID { private set; get; }
        public Dictionary<int, PlayerDB> lobbyCharacterDict { private set; get; } = new();

        #endregion Properties

        #region Methods

        public void Login(string uniqueID)
        {
            // TODO : Security check
            if (ClientState != EClientState.Connected) return;

            lobbyCharacterDict.Clear();

            // TODO : Add error handling logic for login failures
            using (AppDBContext db = new AppDBContext())
            {
                AccountDB account = db.Accounts
                    .Include(a => a.Players)
                    .Where(a => a.Name == uniqueID).FirstOrDefault();

                if (ReferenceEquals(account, null) == true)
                {
                    AccountID = account.ID;

                    LoginResponse loginResponsePacket = new() { ResultCode = 1 };

                    foreach (PlayerDB playerDB in account.Players)
                    {
                        LobbyCharacterInfo lobbyCharacterInfo = new()
                        {
                            Name = playerDB.Name,
                            Level = playerDB.Level
                        };

                        lobbyCharacterDict.Add(playerDB.ID, playerDB);

                        loginResponsePacket.Characters.Add(lobbyCharacterInfo);
                    }

                    Send(loginResponsePacket);

                    ClientState = EClientState.Lobby;
                }
                else
                {
                    AccountDB newAccount = new AccountDB() { Name = uniqueID };
                    AccountID = newAccount.ID;

                    db.Accounts.Add(newAccount);
                    db.SaveChanges();

                    LoginResponse loginResponsePacket = new() { ResultCode = 1 };

                    Send(loginResponsePacket);

                    ClientState = EClientState.Lobby;
                }
            }
        }

        #endregion Methods
    }
}