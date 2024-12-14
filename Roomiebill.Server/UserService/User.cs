using System.ComponentModel.DataAnnotations;

namespace Roomiebill.Server.UserService
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }  // Store the hashed password

        public User() { }

        public User(string email, string username, string passwordHash)
        {
            Email = email;
            Username = username;
            PasswordHash = passwordHash;
        }
    }
}
