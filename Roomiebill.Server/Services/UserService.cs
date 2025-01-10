using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Roomiebill.Server.Common;
using Roomiebill.Server.Models;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Facades;

namespace Roomiebill.Server.Services
{
    public class UserService
    {
        private readonly UserFacade _userFacade;

        public UserService(UsersDb usersDb, IPasswordHasher<User> passwordHasher, ILogger<UserFacade> userFacadeLogger)
        {
            _userFacade = new UserFacade(usersDb, passwordHasher, userFacadeLogger);
        }

        public async Task<User> RegisterUserAsync(RegisterUserDto registerUserDto)
        {
            return await _userFacade.RegisterUserAsync(registerUserDto);
        }

        public async Task UpdatePasswordAsync(UpdatePasswordDto updatePasswordDto)
        {
            await _userFacade.UpdatePasswordAsync(updatePasswordDto);
        }
    }
}
