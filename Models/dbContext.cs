using Microsoft.EntityFrameworkCore;
 
namespace CSVtoDB.Models
{
    public class dbContext : DbContext
    {
        public DbSet<Employee> employees {get; set;}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=csvReader.db");
        }
    }
}
