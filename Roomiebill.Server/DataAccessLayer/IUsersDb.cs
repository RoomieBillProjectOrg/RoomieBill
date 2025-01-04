using Roomiebill.Server.Models;

namespace Roomiebill.Server.DataAccessLayer
{
    public interface IUsersDb 
    {
        public void AddUser(User user);
        public User? GetUserByEmail(string email);
        public User? GetUserByUsername(string username);
    }
}
