using ExchangeRatesPlatform.Model;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRatesPlatform
{
    class CurrencyContext : DbContext
    {
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Rate> Rates { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //https://stackoverflow.com/questions/17993680/referenced-projects-and-config-files я хотел вынести в конфиг, но облажался с структурой проекта
            optionsBuilder.UseSqlServer(@"connectionString");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Rate>(entity => {
                entity.HasIndex("CurrencyCode", "Date")
                .IsUnique();
            });
        }
    }
}
