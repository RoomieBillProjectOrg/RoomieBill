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
            _logger.LogInformation($"Register user with details: Username: {registerUserDto.Username}, Email: {registerUserDto.Email}");

            if (registerUserDto.Username == null)
            {
                _logger.LogError($"Username is null. Cannot register user with details: Username: {registerUserDto.Username}, Email: {registerUserDto.Email}");
                throw new ArgumentNullException(nameof(registerUserDto.Username));
            }

            if (registerUserDto.Email == null)
            {
                _logger.LogError($"Email is null. Cannot register user with details: Username: {registerUserDto.Username}, Email: {registerUserDto.Email}");
                throw new ArgumentNullException(nameof(registerUserDto.Email));
            }

            if (registerUserDto.Password == null)
            {
                _logger.LogError($"Password is null. Cannot register user with details: Username: {registerUserDto.Username}, Email: {registerUserDto.Email}");
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
                _logger.LogError($"User with this username = {registerUserDto.Username} already exists");
                throw new Exception("User with this username already exists");
            }

            // Check if the user already exists by email
            existingUser = _usersDb.GetUserByEmail(registerUserDto.Email);
            if (existingUser != null)
            {
                _logger.LogError($"User with this email = {registerUserDto.Email} already exists");
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

            _logger.LogInformation($"User registered successfully with details: Username: {registerUserDto.Username}, Email: {registerUserDto.Email}");

            return newUser;
        }

        public async Task<User> UpdatePasswordAsync(UpdatePasswordDto updatePasswordDto)
        {
            _logger.LogInformation($"Updating user {updatePasswordDto.Username} password");

            if (updatePasswordDto.Username == null)
            {
                _logger.LogError($"Username is null. Cannot update password for user {updatePasswordDto.Username}");
                throw new ArgumentNullException(nameof(updatePasswordDto.Username));
            }

            if (updatePasswordDto.CurrentPassword == null)
            {
                _logger.LogError($"Old password is null. Cannot update password for user {updatePasswordDto.Username}");
                throw new ArgumentNullException(nameof(updatePasswordDto.CurrentPassword));
            }

            if (updatePasswordDto.NewPassword == null)
            {
                _logger.LogError($"New password is null. Cannot update password for user {updatePasswordDto.Username}");
                throw new ArgumentNullException(nameof(updatePasswordDto.NewPassword));
            }

            // Check password validate using class PasswordValidator
            var passwordValidator = new PasswordValidator();
            var result = passwordValidator.ValidatePassword(updatePasswordDto.NewPassword);
            if (!result)
            {
                _logger.LogError(passwordValidator.Error);
                throw new Exception(passwordValidator.Error);
            }

            // Check if the user exists by username
            var existingUser = _usersDb.GetUserByUsername(updatePasswordDto.Username);
            if (existingUser == null)
            {
                _logger.LogError($"User with this username: {updatePasswordDto.Username} does not exist");
                throw new Exception("User with this username does not exist");
            }
            // Verify the old password
            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(existingUser, existingUser.PasswordHash, updatePasswordDto.CurrentPassword);
            if (passwordVerificationResult != PasswordVerificationResult.Success)
            {
                _logger.LogError("Old password is incorrect");
                throw new Exception("Old password is incorrect");
            }
            // Hash the new password
            string passwordHash = _passwordHasher.HashPassword(existingUser, updatePasswordDto.NewPassword);
            // Update the user object with the hashed password
            existingUser.PasswordHash = passwordHash;
            _usersDb.UpdateUser(existingUser);
            _logger.LogInformation($"User {updatePasswordDto.Username} password updated successfully");
            return existingUser;
        }
    }
}
