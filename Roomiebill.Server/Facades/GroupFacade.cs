using Microsoft.AspNetCore.Identity;
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

        public void InviteToGroupByUsername(int inviter_id, string invited_username, int groupId)
        {

        }
    }
}