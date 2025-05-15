using Microsoft.AspNetCore.Identity;
using Roomiebill.Server.Models;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Facades;
using Roomiebill.Server.Services.Interfaces;

namespace Roomiebill.Server.Services
{
    public class UserService : IUserService
    {
        public UserFacade _userFacade { get; }

        public UserService(UserFacade userFacade)
        {
            _userFacade = userFacade;
        }

        public async Task<User> RegisterUserAsync(RegisterUserDto registerUserDto)
        {
            return await _userFacade.RegisterUserAsync(registerUserDto);
        }

        public async Task VerifyRegisterUserDetailsAsync(RegisterUserDto registerUserDto)
        {
            await _userFacade.VerifyRegisterUserDetailsAsync(registerUserDto);
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

        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _userFacade.GetUserByIdAsync(userId);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userFacade.GetUserByEmailAsync(email);
        }

        public async Task AddGroupToUser(string username, int groupId)
        {
            await _userFacade.AddGroupToUser(username, groupId);
        }

        public UserFacade GetUserFacade()
        {
            return _userFacade;
        }
    }
}
