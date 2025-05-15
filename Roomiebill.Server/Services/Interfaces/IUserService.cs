using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;
using Roomiebill.Server.Facades;

namespace Roomiebill.Server.Services.Interfaces
{
    public interface IUserService
    {
        UserFacade GetUserFacade();
        Task<User> RegisterUserAsync(RegisterUserDto registerUserDto);
        Task VerifyRegisterUserDetailsAsync(RegisterUserDto registerUserDto);
        Task<User> UpdatePasswordAsync(UpdatePasswordDto updatePasswordDto);
        Task<User> LoginAsync(LoginDto loginDto);
        Task LogoutAsync(string username);
        Task<bool> IsUserAdminAsync(string username);
        Task<bool> IsUserLoggedInAsync(string username);
        Task<List<Invite>> GetUserInvitesAsync(string username);
        Task<User> GetUserByIdAsync(int userId);
        Task<User> GetUserByEmailAsync(string email);
        Task AddGroupToUser(string username, int groupId);
    }
}
