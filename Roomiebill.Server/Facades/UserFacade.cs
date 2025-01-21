using Microsoft.AspNetCore.Identity;
using Roomiebill.Server.Common.Validators;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.Facades
{
    public class UserFacade : IUserFacade
    {
        private readonly IApplicationDbContext _applicaitonDbs;
        private readonly IPasswordHasher<User> _passwordHasher;
        private ILogger<UserFacade> _logger;

        public UserFacade(IApplicationDbContext usersDb, IPasswordHasher<User> passwordHasher, ILogger<UserFacade> logger)
        {
            _applicaitonDbs = usersDb;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user with the given details.
        /// The new user will be added to the database with default values as no system admin and not logged in.
        /// </summary>
        /// <param name="registerUserDto"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<User> RegisterUserAsync(RegisterUserDto registerUserDto)
        {
            
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
            var existingUser = await _applicaitonDbs.GetUserByUsernameAsync(registerUserDto.Username);
            if (existingUser != null)
            {
                _logger.LogError($"User with this username = {registerUserDto.Username} already exists");
                throw new Exception("User with this username already exists");
            }

            // Check if the user already exists by email
            existingUser = _applicaitonDbs.GetUserByEmail(registerUserDto.Email);
            if (existingUser != null)
            {
                _logger.LogError($"User with this email = {registerUserDto.Email} already exists");
                throw new Exception("User with this email already exists");
            }

            // Create a new user object from the DTO
            User newUser = new User(registerUserDto.Username, registerUserDto.Email, registerUserDto.Password);

            // Hash the password
            string passwordHash = _passwordHasher.HashPassword(newUser, registerUserDto.Password);

            // Update the user object with the hashed password
            newUser.PasswordHash = passwordHash;

            _applicaitonDbs.AddUser(newUser);

            _logger.LogInformation($"User registered successfully with details: Username: {registerUserDto.Username}, Email: {registerUserDto.Email}");

            return newUser;
        }

        /// <summary>
        /// Update the password of the user with the given details.
        /// This function checks if the user exists by username, verifies the old password, and then updates the password.
        /// </summary>
        /// <param name="updatePasswordDto"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
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

            if (updatePasswordDto.VerifyNewPassword == null)
            {
                _logger.LogError($"Verify new password is null. Cannot update password for user {updatePasswordDto.Username}");
                throw new ArgumentNullException(nameof(updatePasswordDto.VerifyNewPassword));
            }

            // Check if the new password and verify new password are the same
            if (updatePasswordDto.NewPassword != updatePasswordDto.VerifyNewPassword)
            {
                _logger.LogError("New password and verify new password are not the same");
                throw new Exception("New password and verify new password are not the same");
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
            var existingUser = await _applicaitonDbs.GetUserByUsernameAsync(updatePasswordDto.Username);
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

            await _applicaitonDbs.UpdateUserAsync(existingUser);
            _logger.LogInformation($"User {updatePasswordDto.Username} password updated successfully");

            return existingUser;
        }

        /// <summary>
        /// Login the user with the given details.
        /// This function checks if the user exists by username, verifies the password, and then logs in the user.
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<User> LoginAsync(LoginDto loginDto)
        {
            _logger.LogInformation($"Logging in user {loginDto.Username}");

            if (loginDto.Username == null)
            {
                _logger.LogError($"Username is null. Cannot login user {loginDto.Username}");
                throw new ArgumentNullException(nameof(loginDto.Username));
            }

            if (loginDto.Password == null)
            {
                _logger.LogError($"Password is null. Cannot login user {loginDto.Username}");
                throw new ArgumentNullException(nameof(loginDto.Password));
            }

            // Check if the user exists by username
            var existingUser = await _applicaitonDbs.GetUserByUsernameAsync(loginDto.Username);
            if (existingUser == null)
            {
                _logger.LogError($"User with this username: {loginDto.Username} does not exist");
                throw new Exception("User with this username does not exist");
            }

            // Verify the password
            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(existingUser, existingUser.PasswordHash, loginDto.Password);
            if (passwordVerificationResult != PasswordVerificationResult.Success)
            {
                _logger.LogError($"User with username: {loginDto.Username} entered incorrect password");
                throw new Exception("Password is incorrect");
            }

            existingUser.IsLoggedIn = true;
            await _applicaitonDbs.UpdateUserAsync(existingUser);
            _logger.LogInformation($"User {loginDto.Username} logged in successfully");

            return existingUser;
        }

        /// <summary>
        /// Logout the user with the given username.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task LogoutAsync(string username)
        {
            _logger.LogInformation($"Logging out user {username}");

            if (username == null)
            {
                _logger.LogError($"Username is null. Cannot logout user");
                throw new ArgumentNullException(nameof(username));
            }

            // Check if the user exists by username
            var existingUser = await _applicaitonDbs.GetUserByUsernameAsync(username);

            if (existingUser == null)
            {
                _logger.LogError($"User with this username: {username} does not exist");
                throw new Exception("User with this username does not exist");
            }

            existingUser.IsLoggedIn = false;

            await _applicaitonDbs.UpdateUserAsync(existingUser);

            _logger.LogInformation($"User {username} logged out successfully");
        }

        /// <summary>
        /// This function gets the user by username and returns if the user is an admin in the system.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<bool> IsUserAdminAsync(string username)
        {
            _logger.LogInformation($"Checking if user {username} is an admin");
            if (username == null)
            {
                _logger.LogError($"Username is null. Cannot check if user is an admin");
                throw new ArgumentNullException(nameof(username));
            }
            // Check if the user exists by username
            var existingUser = await _applicaitonDbs.GetUserByUsernameAsync(username);
            if (existingUser == null)
            {
                _logger.LogError($"User with this username: {username} does not exist");
                throw new Exception("User with this username does not exist");
            }
            _logger.LogInformation($"User {username} is an admin: {existingUser.IsSystemAdmin}");
            return existingUser.IsSystemAdmin;
        }

        /// <summary>
        /// This function gets the user by username and returns if the user is logged in.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<bool> IsUserLoggedInAsync(string username)
        {
            _logger.LogInformation($"Checking if user {username} is logged in");
            if (username == null)
            {
                _logger.LogError($"Username is null. Cannot check if user is logged in");
                throw new ArgumentNullException(nameof(username));
            }
            // Check if the user exists by username
            var existingUser = await _applicaitonDbs.GetUserByUsernameAsync(username);
            if (existingUser == null)
            {
                _logger.LogError($"User with this username: {username} does not exist");
                throw new Exception("User with this username does not exist");
            }
            return existingUser.IsLoggedIn;
        }

        public async Task<User?> GetUserByIdAsync(int payerId)
        {
            return await _applicaitonDbs.GetUserByIdAsync(payerId);
        }

        /// <summary>
        /// Add a group to the user and update the user in the database.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task AddGroupToUser(string username, int groupId)
        {
            var user = await _applicaitonDbs.GetUserByUsernameAsync(username);
            if (user == null)
            {
                _logger.LogError($"User with username {username} does not exist");
                throw new Exception("User does not exist");
            }
            var group = await _applicaitonDbs.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                _logger.LogError($"Group with id {groupId} does not exist");
                throw new Exception("Group does not exist");
            }
            user.AddGroup(group);
            await _applicaitonDbs.UpdateUserAsync(user);
        }

        public async Task<List<Invite>> GetUserInvitesAsync(string username)
        {
            return await _applicaitonDbs.GetUserInvitesAsync(username);
        }

        #region Help functions

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _applicaitonDbs.GetUserByUsernameAsync(username);
        }

        #endregion 
    }
}
