using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.DB;

namespace Server
{
    public partial class ClientSession
    {
        #region Properties

        public int AccountID { private set; get; }
        public Dictionary<int, LobbyCharacterInfo> lobbyCharacterDict { private set; get; } = new();

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
                    .Include(a => a.Characters)
                    .Where(a => a.Name == uniqueID).FirstOrDefault();

                if (ReferenceEquals(account, null) == true)
                {
                    AccountID = account.ID;

                    LoginResponse loginResponsePacket = new() { ResultCode = 1 };

                    foreach (CharacterDB characterDB in account.Characters)
                    {
                        LobbyCharacterInfo info = new()
                        {
                            Name = characterDB.Name,
                            Level = characterDB.Level
                        };

                        lobbyCharacterDict.Add(characterDB.ID, info);

                        loginResponsePacket.Characters.Add(info);
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

        public void CreateCharacter(string name)
        {
            if (ClientState != EClientState.Lobby) return;

            using (AppDBContext db = new())
            {
                CreateCharacterResponse packet = new();

                CharacterDB characterDB = db.Characters
                    .Where(c => c.Name == name).FirstOrDefault();

                if (ReferenceEquals(characterDB, null) == false)
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

                CharacterDB newCharacterDB = new()
                {
                    AccountID = AccountID,
                    Name = name,
                    Level = 1,
                    CurHp = stat.MaxHpDictionary[1],
                    MaxHp = stat.MaxHpDictionary[1],
                    AttackPower = stat.AttackPowerDictionary[1],
                    Speed = 5f,
                    TotalExp = 0
                };

                db.Characters.Add(newCharacterDB);
                db.SaveChanges();

                LobbyCharacterInfo newCharacterInfo = new()
                {
                    Name = name,
                    Level = 1
                };

                lobbyCharacterDict.Add(newCharacterDB.ID, newCharacterInfo);

                packet.ResultCode = 0;
                packet.NewCharacter = newCharacterInfo;

                Send(packet);
            }
        }

        #endregion Methods
    }
}