using Microsoft.EntityFrameworkCore;
using Roomiebill.Server.BillingService;
using Roomiebill.Server.UserService;
using Roomiebill.Server.GroupService;

namespace Roomiebill.Server.DataAccessLayer
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Expense> Expenses { get; set; }

    }
}
