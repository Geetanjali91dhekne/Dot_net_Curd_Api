using CurdApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CurdApi.Data
{
    public class CurdApiDbContext : DbContext
    {
        public CurdApiDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
    }
}
