using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.Facades
{
    public class GroupFacade
    {
        private readonly IApplicationDbContext _groupDb;
        private ILogger<GroupFacade> _logger;
        private readonly IUserFacade _userFacade;

        public GroupFacade(IApplicationDbContext groupDb, ILogger<GroupFacade> logger, IUserFacade userFacade)
        {
            _groupDb = groupDb;
            _logger = logger;
            _userFacade = userFacade;
        }

        /// <summary>
        /// This method creates a new group with the given details.
        /// If the admin or any of the members do not exist in the system, an exception is thrown.
        /// </summary>
        /// <param name="newGroupDto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Group> CreateNewGroupAsync(CreateNewGroupDto newGroupDto)
        {
            _logger.LogInformation($"Creating new group with the following details: {newGroupDto}");

            // Extract group admin user from UserFacade
            User? admin = await _userFacade.GetUserByUsernameAsync(newGroupDto.AdminGroupUsername);

            // Alert if the admin is not a user in the system
            if (admin == null)
            {
                _logger.LogError($"Error when trying to create new group: admin with username {newGroupDto.AdminGroupUsername} does not exist in the system.");
               throw new Exception($"Error when trying to create new group: admin with username {newGroupDto.AdminGroupUsername} does not exist in the system.");
            }

            // Extract group members from UserFacade using usernames
            List<User> members = new List<User>();
            foreach (string username in newGroupDto.GroupMembersPhoneNumbersList)
            {
                User? member = await _userFacade.GetUserByUsernameAsync(username);

                if(member == null)
                {
                    _logger.LogError($"Error when trying to create new group: member with username {username} does not exist in the system.");
                    throw new Exception($"Error when trying to create new group: member with username {username} does not exist in the system.");
                }

                members.Add(member);
            }

            // Create new group
            Group newGroup = new Group(newGroupDto.GroupName, admin, members);

            // Add group to database
            _groupDb.AddGroup(newGroup);

            return newGroup;
        }

        public async Task InviteToGroupByUsername(string inviter_username, string invited_username, int groupId)
        {
            _logger.LogInformation($"Inviting user with username {invited_username} to group with id {groupId}.");

            User? inviter = await _userFacade.GetUserByUsernameAsync(inviter_username);

            if (inviter == null)
            {
                _logger.LogError($"Error when trying to invite user to group: inviter with username {inviter_username} does not exist in the system.");
                throw new Exception($"Error when trying to invite user to group: inviter with username {inviter_username} does not exist in the system.");
            }

            User? invited = await _userFacade.GetUserByUsernameAsync(invited_username);

            if (invited == null)
            {
                _logger.LogError($"Error when trying to invite user to group: invited with username {invited_username} does not exist in the system.");
                throw new Exception($"Error when trying to invite user to group: invited with username {invited_username} does not exist in the system.");
            }

            Group? group = await _groupDb.GetGroupByIdAsync(groupId, query => query.Include(g => g.Invites).ThenInclude(i => i.Invited));

            if (group == null)
            {
                _logger.LogError($"Error when trying to invite user to group: group with id {groupId} does not exist in the system.");
                throw new Exception($"Error when trying to invite user to group: group with id {groupId} does not exist in the system.");
            }

            if (IsInviteForUserExistInGroup(invited, group))
            {
                _logger.LogError($"Error when trying to invite user to group: user with username {invited_username} is already invited to group with id {groupId}.");
                throw new Exception($"Error when trying to invite user to group: user with username {invited_username} is already invited to group with id {groupId}.");
            }

            Invite invite = new Invite(inviter, invited, group);

            await _userFacade.AddInviteToinvited(invited, invite);
            await AddInviteToGroup(group, invite);

            _logger.LogInformation($"User with username {invited_username} has been invited to group with id {groupId}.");
        }

        public async Task<Group> GetGroupByIdAsync(int groupId)
        {
            _logger.LogInformation($"Getting group with id {groupId}.");

            Group? group = await _groupDb.GetGroupByIdAsync(groupId, null);

            if (group == null)
            {
                _logger.LogError($"Error when trying to get group: group with id {groupId} does not exist in the system.");
                throw new Exception($"Error when trying to get group: group with id {groupId} does not exist in the system.");
            }

            return group;
        }

        #region Help functions

        private bool IsInviteForUserExistInGroup(User invited, Group group)
        {
            return group.Invites.Any(i => i.Invited == invited);
        }

        private async Task AddInviteToGroup(Group group, Invite invite)
        {
            _logger.LogInformation($"Adding invite to group with id {group.Id}.");
            group.AddInvite(invite);
            await _groupDb.UpdateGroupAsync(group);
            _logger.LogInformation($"Invite has been added to group with id {group.Id}.");
        }
        
        #endregion 
    }
}