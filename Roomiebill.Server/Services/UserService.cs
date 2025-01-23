using Microsoft.AspNetCore.Identity;
using Roomiebill.Server.Models;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Facades;

namespace Roomiebill.Server.Services
{
    public class UserService
    {
        public UserFacade _userFacade { get; }

        public UserService(IApplicationDbContext usersDb, IPasswordHasher<User> passwordHasher, ILogger<UserFacade> userFacadeLogger)
        {
            _userFacade = new UserFacade(usersDb, passwordHasher, userFacadeLogger);
        }

        public async Task<User> RegisterUserAsync(RegisterUserDto registerUserDto)
        {
            return await _userFacade.RegisterUserAsync(registerUserDto);
        }

        public async Task<User> UpdatePasswordAsync(UpdatePasswordDto updatePasswordDto)
        {
            return await _userFacade.UpdatePasswordAsync(updatePasswordDto);
        }

        public async Task<User> LoginAsync(LoginDto loginDto)
        {
            return await _userFacade.LoginAsync(loginDto);
        }

        public async Task LogoutAsync(string username)
        {
            await _userFacade.LogoutAsync(username);
        }

        public async Task<bool> IsUserAdminAsync(string username)
        {
            return await _userFacade.IsUserAdminAsync(username);
        }

        public async Task<bool> IsUserLoggedInAsync(string username)
        {
            return await _userFacade.IsUserLoggedInAsync(username);
        }

        public async Task<List<Invite>> GetUserInvitesAsync(string username)
        {
            return await _userFacade.GetUserInvitesAsync(username);
        }
    }
}
