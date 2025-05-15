using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Roomiebill.Server.Common.Notification;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Facades;
using Roomiebill.Server.Services.Interfaces;

namespace Roomiebill.Server.Services
{
    public class InviteService : IInviteService
    {
        private readonly InviteFacade _inviteFacade;
        private readonly IUserFacade _userFacade;

        public InviteService(IApplicationDbContext inviteDb, IUserService userService, IGroupService groupService, ILogger<InviteFacade> _logger)
        {
            _userFacade = userService.GetUserFacade();
            _inviteFacade = new InviteFacade(inviteDb, _logger, _userFacade, groupService.GetGroupFacade());
        }

        public async Task InviteToGroupByEmail(InviteToGroupByEmailDto inviteDetails)
        {
            if (!await _userFacade.IsUserLoggedInAsync(inviteDetails.InviterUsername))
            {
                throw new Exception($"User with username {inviteDetails.InviterUsername} is not logged in.");
            }
            await _inviteFacade.InviteToGroupByEmail(inviteDetails.InviterUsername, inviteDetails.Email, inviteDetails.GroupId);
        }

        public async Task InviteToGroupByUsernamesList(CreateNewGroupDto group, int groupId)
        {
            if (!await _userFacade.IsUserLoggedInAsync(group.AdminGroupUsername))
            {
                throw new Exception($"User with username {group.AdminGroupUsername} is not logged in.");
            }
            if (group.GroupMembersEmailsList == null)
            {
                throw new Exception("Group members list is empty.");
            }
            await _inviteFacade.InviteToGroupByEmailsList(group.AdminGroupUsername, group.GroupMembersEmailsList, groupId);
        }

        public async Task AnswerInviteByUser(AnswerInviteByUserDto answerDetails)
        {
            if (!await _userFacade.IsUserLoggedInAsync(answerDetails.InvitedUsername))
            {
                throw new Exception($"User with username {answerDetails.InvitedUsername} is not logged in.");
            }
            await _inviteFacade.AnswerInviteByUser(answerDetails.InviteId, answerDetails.IsAccepted);
        }
    }
}
