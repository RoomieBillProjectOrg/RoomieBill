using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Roomiebill.Server.Common;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.Facades
{
    public class UserFacade
    {
        private readonly IUsersDb _usersDb;
        private readonly IPasswordHasher<User> _passwordHasher;
        private ILogger<UserFacade> _logger;

        public UserFacade(IUsersDb usersDb, IPasswordHasher<User> passwordHasher, ILogger<UserFacade> logger)
        {
            _usersDb = usersDb;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<User> RegisterUserAsync(RegisterUserDto registerUserDto)
        {
            _logger.LogInformation("Registering user");

            if (registerUserDto.Username == null)
            {
                _logger.LogError("Username is null. Cannot register user.");
                throw new ArgumentNullException(nameof(registerUserDto.Username));
            }

            if (registerUserDto.Email == null)
            {
                _logger.LogError("Email is null. Cannot register user.");
                throw new ArgumentNullException(nameof(registerUserDto.Email));
            }

            if (registerUserDto.Password == null)
            {
                _logger.LogError("Password is null. Cannot register user.");
                throw new ArgumentNullException(nameof(registerUserDto.Password));
            }

            // Check password validate using class PasswordValidator
            var passwordValidator = new PasswordValidator();
            var result = passwordValidator.ValidatePassword(registerUserDto.Password);
            if (!result)
            {
                _logger.LogError(passwordValidator.Error);
                throw new Exception(passwordValidator.Error);
            }

            //Check email validate using class EmailValidator
            var emailValidator = new EmailValidator();
            result = emailValidator.ValidateEmail(registerUserDto.Email);
            if (!result)
            {
                _logger.LogError(emailValidator.Error);
                throw new Exception(emailValidator.Error);
            }

            // Check if the user already exists by username
            var existingUser = _usersDb.GetUserByUsername(registerUserDto.Username);
            if (existingUser != null)
            {
                _logger.LogError("User with this username already exists");
                throw new Exception("User with this username already exists");
            }

            // Check if the user already exists by email
            existingUser = _usersDb.GetUserByEmail(registerUserDto.Email);
            if (existingUser != null)
            {
                _logger.LogError("User with this email already exists");
                throw new Exception("User with this email already exists");
            }

            // Create a new user object from the DTO
            User newUser = new User
            {
                Username = registerUserDto.Username,
                Email = registerUserDto.Email
            };

            // Hash the password
            string passwordHash = _passwordHasher.HashPassword(newUser, registerUserDto.Password);

            // Update the user object with the hashed password
            newUser.PasswordHash = passwordHash;

            _usersDb.AddUser(newUser);

            _logger.LogInformation("User registered successfully");

            return newUser;
        }
    }
}
