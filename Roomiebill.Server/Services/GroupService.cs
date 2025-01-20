using Microsoft.AspNetCore.Identity;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Facades;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.Services
{
    public class GroupService
    {
        private readonly GroupFacade _groupFacade;
        private readonly UserFacade _userFacade;

        private readonly ILogger<GroupFacade> _groupFacadeLogger;

        public GroupService(IApplicationDbContext groupsDb, ILogger<GroupFacade> groupFacadeLogger, UserService userService)
        {
            _userFacade = userService._userFacade;
            _groupFacade = new GroupFacade(groupsDb, groupFacadeLogger, _userFacade);

            _groupFacadeLogger = groupFacadeLogger;
        }

        public async Task<Group> CreateNewGroupAsync(CreateNewGroupDto group)
        {
            Group newGroup = await _groupFacade.CreateNewGroupAsync(group);

            // Extract group members from UserFacade using usernames
            List<User> members = new List<User>();
            if (group.GroupMembersUsernamesList != null)
            {
                foreach (string username in group.GroupMembersUsernamesList)
                {
                    User? member = await _userFacade.GetUserByUsernameAsync(username);

                    if (member == null)
                    {
                        _groupFacadeLogger.LogError($"Error when trying to create new group: member with username {username} does not exist in the system.");
                        throw new Exception($"Error when trying to create new group: member with username {username} does not exist in the system.");
                    }

                    members.Add(member);
                }
            }

            // Send invites to all group members, except the admin
            foreach (User member in members)
            {
                if (member.Username == group.AdminGroupUsername)
                {
                    continue;
                }
                // TODO: might be better to use the service method here.
                await _groupFacade.InviteToGroupByUsername(group.AdminGroupUsername, member.Username, newGroup.Id);
            }

            // Add the group to the admin's groups list
            await _userFacade.AddGroupToUser(group.AdminGroupUsername, newGroup.Id);

            return newGroup;
        }

        public async Task <List<Group>> GetUserGroupsAsync(int UserId){
            // TODO: maybe add exception catch.
            List<Group> UserGroups = await _groupFacade.GetUserGroupsAsync(UserId);
            return UserGroups;
        }

        public async Task InviteToGroupByUsername(InviteToGroupByUsernameDto inviteDetails)
        {
            if (!await _userFacade.IsUserLoggedInAsync(inviteDetails.InviterUsername))
            {
                throw new Exception($"User with username {inviteDetails.InviterUsername} is not logged in.");
            } 
            await _groupFacade.InviteToGroupByUsername(inviteDetails.InviterUsername, inviteDetails.InvitedUsername, inviteDetails.GroupId);
        }

        public async Task AnswerInviteByUser(AnswerInviteByUserDto answerDetails)
        {
            if (!await _userFacade.IsUserLoggedInAsync(answerDetails.InvitedUsername))
            {
                throw new Exception($"User with username {answerDetails.InvitedUsername} is not logged in.");
            }
            await _userFacade.AnswerInviteByUser(answerDetails.InviteId, answerDetails.IsAccepted);
            await _groupFacade.AnswerInviteByUser(answerDetails.InviteId);
        }

        public async Task<Expense> AddExpenseAsync(ExpenseDto expense)
        {
            return await _groupFacade.AddExpenseAsync(expense);
        }


    }
}
