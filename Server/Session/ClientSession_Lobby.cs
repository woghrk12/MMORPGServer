using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
                            CharacterID = characterDB.ID,
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
                if (DataManager.CharacterStatDictionary.TryGetValue(1, out Data.CharacterStat stat) == false)
                {
                    Send(new CreateCharacterResponse() { ResultCode = 2 });

                    return;
                }

                CharacterDB newCharacterDB = new()
                {
                    AccountID = AccountID,
                    Name = name,
                    Level = 1,
                    TotalExp = 0,
                    MaxHp = stat.MaxHpDictionary[1],
                    CurHp = stat.MaxHpDictionary[1],
                    AttackPower = stat.AttackPowerDictionary[1],
                    Speed = 5,
                    CurPosX = 0,
                    CurPosY = 0,
                    FacingDirection = EMoveDirection.Right
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

        public void EnterGameRoom(int characterID)
        {
            if (ClientState != EClientState.Lobby) return;

            LobbyCharacterInfo lobbyCharacter = lobbyCharacterDict.Values.FirstOrDefault(c => c.CharacterID.Equals(characterID));
            if (ReferenceEquals(lobbyCharacter, null) == true)
            {
                Send(new CharacterEnterGameRoomResponse() { ResultCode = 1 });
                return;
            }

            using (AppDBContext db = new())
            {
                CharacterDB characterDB = db.Characters.FirstOrDefault(c => c.ID.Equals(characterID));
                if (ReferenceEquals(characterDB, null) == true)
                {
                    Send(new CharacterEnterGameRoomResponse() { ResultCode = 2 });
                    return;
                }

                Character = ObjectManager.Instance.Add<Character>();

                Character.Session = this;
                Character.CharacterID = characterID;

                Character.Name = characterDB.Name;
                Character.Level = characterDB.Level;
                Character.TotalExp = characterDB.TotalExp;
                Character.MaxHp = characterDB.MaxHp;
                Character.CurHp = characterDB.CurHp;
                Character.AttackPower = characterDB.AttackPower;
                Character.MoveSpeed = characterDB.Speed;
                Character.Position = new Pos(characterDB.CurPosX, characterDB.CurPosY);
                Character.FacingDirection = characterDB.FacingDirection;

                List<ItemDB> itemDBList = db.Items
                    .Where(i => i.OwnerID == characterID)
                    .ToList();

                foreach (ItemDB itemDB in itemDBList)
                {
                    Item item = Item.MakeItem(itemDB);

                    if (ReferenceEquals(item, null) == true) continue;

                    Character.Inventory.AddItem(item);
                }
            }

            GameRoom room = RoomManager.Instance.Find(1);
            room.Push(room.EnterRoom, Character);

            ClientState = EClientState.Ingame;
        }

        #endregion Methods
    }
}