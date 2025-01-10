using Microsoft.EntityFrameworkCore;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.DataAccessLayer
{
    public class GroupsDb : DbContext, IGroupDb
    {
        public DbSet<Group> Groups { get; set; }

        public GroupsDb(DbContextOptions<GroupsDb> options) : base(options) { }

        public void AddGroup(Group group)
        {
            Groups.Add(group);
            SaveChanges();
        }
    }
}
