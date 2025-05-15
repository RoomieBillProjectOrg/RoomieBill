using FrontendApplication.Models;
using Roomiebill.Server.DataAccessLayer.Dtos;

namespace FrontendApplication.Services.Interfaces
{
    public interface IUserServiceApi
    {
        Task<bool> RegisterUserAsync(RegisterUserDto user);
        Task<UserModel> LoginUserAsync(string username, string password, string firebaseToken);
        Task LogoutUserAsync(string username);
        Task<UserModel> UpdateUserPasswordAsync(UpdatePasswordDto updatePasswordDto);
        Task<GroupModel> CreateNewGroupAsync(CreateNewGroupDto newGroupDto);
        Task<List<InviteModel>> ShowUserInvites(string username);
        Task AnswerInviteAsync(AnswerInviteByUserDto answer);
        Task<VerifiyCodeModel> VerifyUserRegisterDetails(RegisterUserDto user);
    }
}
