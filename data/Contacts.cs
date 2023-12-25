using Microsoft.EntityFrameworkCore;
using WebAPIs.Model;

namespace WebAPIs.data
{
    public class ContactsAPIDbContext : DbContext
    {
        public ContactsAPIDbContext(DbContextOptions options) : base(options) { }
        public DbSet<Contacts> Contacts
        {
            get;
            set;
        }
    }
}
