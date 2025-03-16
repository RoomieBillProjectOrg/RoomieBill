
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.Facades
{
    public interface IInviteFacade
    {
        Task InviteToGroupByUsername(string inviter_username, string invited_username, int groupId);

        Task InviteToGroupByUsernamesList(string inviter_username, List<string> invited_usernames, int groupId);

        Task AnswerInviteByUser(int inviteId, bool isAccepted);
    }
}