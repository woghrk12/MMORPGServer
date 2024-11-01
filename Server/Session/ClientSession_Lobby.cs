using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
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

        public void CreateCharacter(LobbyCharacterInfo newCharacterInfo)
        {
            if (ClientState != EClientState.Lobby) return;

            using (AppDBContext db = new())
            {
                CreateCharacterResponse packet = new();

                PlayerDB playerDB = db.Players
                    .Where(p => p.Name == newCharacterInfo.Name).FirstOrDefault();

                if (ReferenceEquals(playerDB, null) == false)
                {
                    packet.ResultCode = 1;

                    Send(packet);
                    return;
                }

                // TODO : Search the data sheet based on the information provided by the player
                if (DataManager.ObjectStatDictionary.TryGetValue(1, out Data.ObjectStat stat) == false)
                {
                    packet.ResultCode = 2;

                    Send(packet);
                    return;
                }

                PlayerDB newCharacterDB = new()
                {
                    AccountID = AccountID,
                    Name = newCharacterInfo.Name,
                    Level = 1,
                    CurHp = stat.MaxHpDictionary[1],
                    MaxHp = stat.MaxHpDictionary[1],
                    AttackPower = stat.AttackPowerDictionary[1],
                    Speed = 5f,
                    TotalExp = 0
                };

                db.Players.Add(newCharacterDB);
                db.SaveChanges();

                lobbyCharacterDict.Add(newCharacterDB.ID, newCharacterDB);

                packet.ResultCode = 0;
                packet.NewCharacter.MergeFrom(newCharacterInfo);

                Send(packet);
            }
        }

        #endregion Methods
    }
}