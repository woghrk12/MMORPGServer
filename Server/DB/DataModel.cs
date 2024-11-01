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
        public ICollection<CharacterDB> Characters { set; get; }
    }

    [Table("Character")]
    public class CharacterDB
    {
        [Key]
        public int ID { set; get; }

        [ForeignKey("Account")]
        public int AccountID { set; get; }
        public AccountDB Account { set; get; }

        public string Name { set; get; }

        public int Level { set; get; }
        public int CurHp { set; get; }
        public int MaxHp { set; get; }
        public int AttackPower { set; get; }
        public float Speed { set; get; }
        public int TotalExp { set; get; }
    }
}