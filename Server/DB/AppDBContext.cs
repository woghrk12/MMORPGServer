using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.Data;

namespace Server.DB
{
    public class AppDBContext : DbContext
    {
        #region Variables

        private static readonly ILoggerFactory logger = LoggerFactory.Create(builder => { builder.AddConsole(); });

        private string connectionString = "Server=localhost;Database=master;Trusted_Connection=True;Encrypt=true;TrustServerCertificate=true;Initial Catalog=GameDB";

        #endregion Variables

        #region Properties

        public DbSet<AccountDB> Accounts { set; get; }
        public DbSet<CharacterDB> Characters { set; get; }
        public DbSet<ItemDB> Items { set; get; }

        #endregion Properties

        #region Methods

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseLoggerFactory(logger)
                .UseSqlServer(ReferenceEquals(ConfigManager.Config, null) == false ? ConfigManager.Config.ConnectionString : connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountDB>()
                .HasIndex(a => a.Name)
                .IsUnique();

            modelBuilder.Entity<CharacterDB>()
                .HasIndex(c => c.Name)
                .IsUnique();
        }

        #endregion Methods
    }
}