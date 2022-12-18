using Mango.Services.Email.Models;
using Mango.Sevices.Email.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.Email.DbContexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<EmailLog> OrderHeaders { get; set; }
    }
}
