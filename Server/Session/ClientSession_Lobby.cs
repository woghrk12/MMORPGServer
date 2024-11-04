using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.DB;
using Server.Game;

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
                    AccountDB newAccount = new AccountDB() { Name = uniqueID };
                    AccountID = newAccount.ID;

                    db.Accounts.Add(newAccount);

                    if (db.SaveChangesEx() == false)
                    {
                        Send(new LoginResponse() { ResultCode = 2 });
                        return;
                    }

                    Send(new LoginResponse() { ResultCode = 1 });

                    ClientState = EClientState.Lobby;
                }
                else
                {
                    AccountID = account.ID;

                    LoginResponse packet = new() { ResultCode = 0 };

                    foreach (CharacterDB characterDB in account.Characters)
                    {
                        LobbyCharacterInfo info = new()
                        {
                            Name = characterDB.Name,
                            Level = characterDB.Level
                        };

                        lobbyCharacterDict.Add(characterDB.ID, info);

                        packet.Characters.Add(info);
                    }

                    Send(packet);

                    ClientState = EClientState.Lobby;
                }
            }
        }

        public void CreateCharacter(string name)
        {
            if (ClientState != EClientState.Lobby) return;

            using (AppDBContext db = new())
            {
                CharacterDB characterDB = db.Characters
                    .Where(c => c.Name == name).FirstOrDefault();

                if (ReferenceEquals(characterDB, null) == false)
                {
                    Send(new CreateCharacterResponse() { ResultCode = 1 });

                    return;
                }

                // TODO : Search the data sheet based on the information provided by the player
                if (DataManager.ObjectStatDictionary.TryGetValue(1, out Data.ObjectStat stat) == false)
                {
                    Send(new CreateCharacterResponse() { ResultCode = 2 });

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
                    Speed = 5,
                    TotalExp = 0
                };

                db.Characters.Add(newCharacterDB);
                if (db.SaveChangesEx() == false)
                {
                    Send(new CreateCharacterResponse() { ResultCode = 3 });

                    return;
                }

                LobbyCharacterInfo newCharacterInfo = new()
                {
                    Name = name,
                    Level = 1
                };

                lobbyCharacterDict.Add(newCharacterDB.ID, newCharacterInfo);

                CreateCharacterResponse packet = new()
                {
                    ResultCode = 0,
                    NewCharacter = newCharacterInfo
                };

                Send(packet);
            }
        }

        public void EnterGameRoom(string characterName)
        {
            if (ClientState != EClientState.Lobby) return;

            LobbyCharacterInfo lobbyCharacter = lobbyCharacterDict.Values.FirstOrDefault(c => c.Name.Equals(characterName));
            if (ReferenceEquals(lobbyCharacter, null) == true)
            {
                Send(new CharacterEnterGameRoomResponse() { ResultCode = 1 });
                return;
            }

            using (AppDBContext db = new())
            {
                CharacterDB characterDB = db.Characters.FirstOrDefault(c => c.Name.Equals(characterName));
                if (ReferenceEquals(characterDB, null) == true)
                {
                    Send(new CharacterEnterGameRoomResponse() { ResultCode = 2 });
                    return;
                }

                Player = ObjectManager.Instance.Add<Player>();
                Player.Session = this;

                Player.Name = characterDB.Name;
                Player.Stat.MaxHP = characterDB.MaxHp;
                Player.Stat.CurHP = characterDB.CurHp;
                Player.Stat.AttackPower = characterDB.AttackPower;
                Player.MoveSpeed = characterDB.Speed;
                Player.Position = new Pos(characterDB.CurPosX, characterDB.CurPosY);
                Player.FacingDirection = characterDB.FacingDirection;
            }

            GameRoom room = RoomManager.Instance.Find(1);
            room.Push(room.EnterRoom, Player);

            ClientState = EClientState.Ingame;
        }

        #endregion Methods
    }
}