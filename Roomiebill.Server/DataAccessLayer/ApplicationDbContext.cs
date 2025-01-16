using Microsoft.EntityFrameworkCore;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.DataAccessLayer
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        // Empty constructor for tests
        public ApplicationDbContext() { }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Invite> Invites { get; set; }
        //public DbSet<Expense> Expenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define the many-to-many relationship between Group and Members.
            // User can be part of many groups and group can have many users, although group can have only one admin.
            modelBuilder.Entity<Group>()
                .HasOne(g => g.Admin)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict); // Optional: Prevent cascading delete

            modelBuilder.Entity<Group>()
                .HasMany(g => g.Members)
                .WithMany(u => u.GroupsUserIsMemberAt);
            
            // Make so user can have many invites and each invite can have only one invited
            modelBuilder.Entity<Invite>()
                .HasOne(i => i.Inviter)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict); // Optional: Prevent cascading delete

            modelBuilder.Entity<Invite>()
                .HasOne(i => i.Invited)
                .WithMany(u => u.Invites)
                .OnDelete(DeleteBehavior.Restrict); // Optional: Prevent cascading delete

            modelBuilder.Entity<Invite>()
                .HasOne(i => i.Group)
                .WithMany(g => g.Invites)
                .OnDelete(DeleteBehavior.Restrict); // Optional: Prevent cascading delete
        }
        /* User methods */
        public User? GetUserByEmail(string email)
        {
            return Users.FirstOrDefault(u => u.Email == email);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public void AddUser(User user)
        {
            Users.Add(user);
            SaveChanges();
        }

        public async Task UpdateUserAsync(User user)
        {
            Users.Update(user);
            await SaveChangesAsync();
        }

        /* Group methods */
        public async Task<Group?> GetGroupByIdAsync(int groupId)
        {
            return await Groups.FirstOrDefaultAsync(g => g.Id == groupId);
        }
        public void AddGroup(Group group)
        {
            Groups.Add(group);
            SaveChanges();
        }
    }
}
