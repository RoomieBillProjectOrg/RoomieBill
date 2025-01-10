using Roomiebill.Server.Models;

namespace Roomiebill.Server.DataAccessLayer
{
    public interface IUsersDb 
    {
        public User? GetUserByEmail(string email);
        public User? GetUserByUsername(string username);
        public void AddUser(User user);
        public void UpdateUser(User user);
    }
}
