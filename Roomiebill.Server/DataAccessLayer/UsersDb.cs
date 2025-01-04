using Microsoft.EntityFrameworkCore;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.DataAccessLayer
{
    public class UsersDb : DbContext, IUsersDb
    {
        public DbSet<User> Users { get; set; }

        public UsersDb(DbContextOptions<UsersDb> options) : base(options) { }

        public void AddUser(User user)
        {
            Users.Add(user);
            SaveChanges();
        }

        public User? GetUserByEmail(string email)
        {
            return Users.FirstOrDefault(u => u.Email == email);
        }

        public User? GetUserByUsername(string username)
        {
            return Users.FirstOrDefault(u => u.Username == username);
        }
    }
}
