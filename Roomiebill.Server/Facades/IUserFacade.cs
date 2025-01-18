using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.Facades
{
    public interface IUserFacade
    {
        Task<User> RegisterUserAsync(RegisterUserDto registerUserDto);
        Task<User> UpdatePasswordAsync(UpdatePasswordDto updatePasswordDto);
        Task<User> LoginAsync(LoginDto loginDto);
        Task<bool> IsUserAdminAsync(string username);
        Task<bool> IsUserLoggedInAsync(string username);
        Task AddInviteToinvited(User invited, Invite inv);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByIdAsync(int payerId);
    }
}
