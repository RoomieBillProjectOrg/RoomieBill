using Roomiebill.Server.Models;

namespace Roomiebill.Server.DataAccessLayer
{
    public interface IApplicationDbContext
    {
        public User? GetUserByEmail(string email);

        public User? GetUserByUsername(string username);

        public void AddUser(User user);

        public void UpdateUser(User user);

        public void AddGroup(Group group);

        public Group? GetGroupById(int id);

        public void AddExpense(Expense expense);
        public User? GetUserById(int payerId);

    }
}
