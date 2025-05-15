using Roomiebill.Server.DataAccessLayer.Dtos;

namespace Roomiebill.Server.Services.Interfaces
{
    public interface IInviteService
    {
        Task InviteToGroupByEmail(InviteToGroupByEmailDto inviteDetails);
        Task InviteToGroupByUsernamesList(CreateNewGroupDto group, int groupId);
        Task AnswerInviteByUser(AnswerInviteByUserDto answerDetails);
    }
}
