using Microsoft.EntityFrameworkCore;
using CreditCardGenerator.Models;
namespace CreditCardGenerator.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> opt) : base(opt)
        {
        }

        public DbSet<CreditCard> CreditCards { get; set; }
    }
}
