using Server.DB;

namespace Server
{
    public static class Extensions
    {
        public static bool SaveChangesEx(this AppDBContext db)
        {
            try
            {
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}