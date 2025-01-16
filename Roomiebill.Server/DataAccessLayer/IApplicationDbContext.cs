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

        public Task<Group?> GetGroupByIdAsync(int groupId, Func<IQueryable<Group>, IQueryable<Group>> includeFunc);

        public void AddGroup(Group group);

    }
}
