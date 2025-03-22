
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.Facades
{
    public interface IInviteFacade
    {
        Task InviteToGroupByEmail(string inviter_username, string email, int groupId);

        Task InviteToGroupByEmailsList(string inviter_username, List<string> invited_usernames, int groupId);

        Task AnswerInviteByUser(int inviteId, bool isAccepted);
    }
}