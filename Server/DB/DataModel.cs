using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.DB
{
    [Table("Account")]
    public class AccountDB
    {
        [Key]
        public int ID { set; get; }

        public string Name { set; get; }
        public ICollection<PlayerDB> Players { set; get; }
    }

    [Table("Player")]
    public class PlayerDB
    {
        [Key]
        public int ID { set; get; }

        public string Name { set; get; }
        public AccountDB Account { set; get; }
    }
}