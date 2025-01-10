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
        Task<User> IsUserLoggedInAsync(string username);
        Task<User> GetUserByUsernameAsync(string username);
    }
}
