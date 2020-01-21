using Microsoft.EntityFrameworkCore;

namespace GrpcTestWebApi.Common.Context
{
    public class DataContext : DbContext
    {
        public DataContext()
        {
        }

        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=./;Integrated Security=true;Initial Catalog=CbrRates");
        }

        public DbSet<CbrRate> Rates { get; set; }
    }
}
