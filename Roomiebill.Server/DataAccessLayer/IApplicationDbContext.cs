using Roomiebill.Server.Models;

namespace Roomiebill.Server.DataAccessLayer
{
    public interface IApplicationDbContext
    {
        public User? GetUserByEmail(string email);

        public Task<User?> GetUserByUsernameAsync(string username);

        public void AddUser(User user);

        public Task UpdateUserAsync(User user);

        public Task<Group?> GetGroupByIdAsync(int groupId);

        public void AddGroup(Group group);

    }
}
