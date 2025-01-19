using System.Linq.Expressions;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.DataAccessLayer
{
    public interface IApplicationDbContext
    {
        public User? GetUserByEmail(string email);

        public Task<User?> GetUserByUsernameAsync(string username);

        public void AddUser(User user);

        public Task UpdateUserAsync(User user);

        public Task UpdateGroupAsync(Group group);

        public Task<Group?> GetGroupByIdAsync(int groupId);

        public Task AddGroupAsync(Group group);

        public Group? GetGroupById(int id);

        public Task AddExpenseAsync(Expense expense);

        public Task<User?> GetUserByIdAsync(int payerId);

        public Task<Invite?> GetInviteByIdAsync(int inviteId);

        public Task UpdateInviteAsync(Invite invite);

        public Task<Invite?> AddInviteAsync(Invite invite);
    }
}
