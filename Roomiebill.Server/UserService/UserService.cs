using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Roomiebill.Server.DataAccessLayer;

namespace Roomiebill.Server.UserService
{
    public class UserService
    {
        // Add user management logic here (e.g., Register, Login, GetUsers).

        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<User> RegisterUserAsync(RegisterUserDto registerUserDto)
        {
            // Check if the user already exists

            // Hash the password
            var passwordHash = _passwordHasher.HashPassword(null, registerUserDto.Password);

            // Create a new user
            var newUser = new User
            {
                Username = registerUserDto.Username,
                Email = registerUserDto.Email,
                PasswordHash = passwordHash
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return newUser;
        }
    }
}
