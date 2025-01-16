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
        public DbSet<Expense> Expenses { get; set; }    
        public DbSet<ExpenseSplit> ExpenseSplits { get; set; }  

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
                        // Define the one-to-many relationship between Group and Expenses
            modelBuilder.Entity<Group>()
                .HasMany(g => g.Expenses)
                .WithOne(e => e.Group)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade); // Cascading delete for related expenses

            // Define the one-to-many relationship between Expense and ExpenseSplits
            modelBuilder.Entity<Expense>()
                .HasMany(e => e.ExpenseSplits)
                .WithOne(es => es.Expense)
                .HasForeignKey(es => es.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade); // Cascading delete for splits

            // ExpenseSplit ↔ User
            modelBuilder.Entity<ExpenseSplit>()
                .HasOne(es => es.User)
                .WithMany()
                .HasForeignKey(es => es.UserId)
                .OnDelete(DeleteBehavior.Restrict);

    
        }


        public User? GetUserByEmail(string email)
        {
            return Users.FirstOrDefault(u => u.Email == email);
        }

        public User? GetUserByUsername(string username)
        {
            return Users.FirstOrDefault(u => u.Username == username);
        }

        public void AddUser(User user)
        {
            Users.Add(user);
            SaveChanges();
        }

        public void UpdateUser(User user)
        {
            Users.Update(user);
            SaveChanges();
        }

        public void AddGroup(Group group)
        {
            Groups.Add(group);
            SaveChanges();
        }
        public void AddExpense(Expense expense)
        {
            Expenses.Add(expense);
            SaveChanges();
        }
    }
}
