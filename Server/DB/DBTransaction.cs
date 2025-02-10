
using Microsoft.EntityFrameworkCore;
using Server.Game;

namespace Server.DB
{
    public class DBTransaction : TaskQueue
    {
        #region Properties

        public static DBTransaction Instance { get; } = new();

        #endregion Properties

        #region Methods

        public static void SavePlayerStatus(Character character, GameRoom room)
        {
            if (ReferenceEquals(character, null) == true || ReferenceEquals(room, null) == true) return;

            CharacterDB characterDB = new();
            characterDB.ID = character.CharacterID;

            // The problem that occur when executing the DB save logic in the character class.
            // 1) If the server goes down, all unsaved data will be lost
            // 2) It completely blocks the code flow

            // Solution : Delegate the DB logic to be handled by another thread
            Instance.Push(() =>
            {
                using (AppDBContext db = new())
                {
                    db.Entry(characterDB).State = EntityState.Unchanged;
                    db.Entry(characterDB).Property(nameof(characterDB.CurHp)).IsModified = true;

                    if (db.SaveChangesEx() == true)
                    {
                        room.Push(() => Console.WriteLine($"Cur Hp saved. {characterDB.CurHp}"));
                    }
                }
            });
        }

        #endregion Methods
    }
}