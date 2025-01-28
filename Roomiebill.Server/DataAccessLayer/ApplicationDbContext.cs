using Microsoft.EntityFrameworkCore;
using Roomiebill.Server.Facades;
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
        public Group? GetGroupById(int id)
        {
            return Groups.FirstOrDefault(g => g.Id == id);
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

        public async Task UpdateGroupAsync(Group group)
        {
            try
            {
                Groups.Update(group);
                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log or display the exception details
                Console.WriteLine($"Error updating group: {ex.Message}");
                throw; // Re-throw the exception to propagate it
            }
        }

        /* Group methods */
        // public async Task<Group?> GetGroupByIdAsync(int groupId)
        // {
        //     return await Groups.FirstOrDefaultAsync(g => g.Id == groupId);
        // }

        //Include all the related fields that griup should have
        public async Task<Group?> GetGroupByIdAsync(int groupId)
        {
            Group? group = await Groups
            .Include(g => g.Admin)
            .Include(g => g.Members)
            .Include(g => g.Expenses)
            .Include(g => g.Invites)
            .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null) {
                return null;
            }
            group.Expenses = await Expenses
            .Include(e => e.Payer)
            .Include(e => e.ExpenseSplits)
            .Where(e => e.GroupId == groupId)
            .ToListAsync();

            group.expenseHandler = new ExpenseHandler(group.Members);

            //group.EnlargeDebtArraySize(group.Members.Count, 0);

            return group;
        }

        public async Task AddGroupAsync(Group group)
        {
            await Groups.AddAsync(group);
            await SaveChangesAsync();
        }
        public async Task AddExpenseAsync(Expense expense)
        {
            await Expenses.AddAsync(expense);
            await SaveChangesAsync();
        }

        public async Task<User?> GetUserByIdAsync(int payerId)
        {
            return await Users.FirstOrDefaultAsync(u => u.Id == payerId);
        }

        public async Task<Invite?> GetInviteByIdAsync(int inviteId)
        {
            return await Invites
            .Include(i => i.Group)
            .FirstOrDefaultAsync(i => i.Id == inviteId);
        }

        public async Task UpdateInviteAsync(Invite invite)
        {
            Invites.Update(invite);
            await SaveChangesAsync();
        }

        public async Task<Invite> AddInviteAsync(Invite invite)
        {
            await Invites.AddAsync(invite);
            await SaveChangesAsync();
            return invite;
        }

        public async Task<List<Group>> GetUserGroupsAsync(int UserId)
        {
            return await Groups
            .Include(g => g.Members)
            .Where(g => g.Members.Any(m => m.Id == UserId))
            .ToListAsync();
        }

        public async Task<List<Invite>> GetUserInvitesAsync(string username)
        {
            return await Invites
            .Include(i => i.Inviter)
            .Include(i => i.Group)
            .Include(i => i.Invited)
            .Where(i => i.Invited.Username == username)
            .ToListAsync();
        }

        public async Task<int> GetNextExpenseIdAsync(){
            int maxId = await Expenses.MaxAsync(e => (int?)e.Id) ?? 0;
            return maxId + 1;
        }

        public async Task<int> GetNextExpenseSplitIdAsync(){
            int maxId = await ExpenseSplits.MaxAsync(e => (int?)e.Id) ?? 0;
            return maxId + 1;
        }
    }
}
