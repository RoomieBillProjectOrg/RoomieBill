using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Facades;

namespace Roomiebill.Server.Services
{
    public class InviteService
    {
        private readonly InviteFacade _inviteFacade;
        private readonly IUserFacade _userFacade;

        public InviteService(IApplicationDbContext inviteDb, UserService userService, GroupService groupService, ILogger<InviteFacade> _logger)
        {
            _inviteFacade = new InviteFacade(inviteDb, _logger, userService._userFacade, groupService._groupFacade);
            _userFacade = userService._userFacade;
        }

        public async Task InviteToGroupByUsername(InviteToGroupByUsernameDto inviteDetails)
        {
            if (!await _userFacade.IsUserLoggedInAsync(inviteDetails.InviterUsername))
            {
                throw new Exception($"User with username {inviteDetails.InviterUsername} is not logged in.");
            } 
            await _inviteFacade.InviteToGroupByUsername(inviteDetails.InviterUsername, inviteDetails.InvitedUsername, inviteDetails.GroupId);
        }

        public async Task InviteToGroupByUsernamesList(CreateNewGroupDto group, int groupId)
        {
            if (!await _userFacade.IsUserLoggedInAsync(group.AdminGroupUsername))
            {
                throw new Exception($"User with username {group.AdminGroupUsername} is not logged in.");
            }
            if (group.GroupMembersUsernamesList == null)
            {
                throw new Exception("Group members list is empty.");
            }
            await _inviteFacade.InviteToGroupByUsernamesList(group.AdminGroupUsername, group.GroupMembersUsernamesList, groupId);
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