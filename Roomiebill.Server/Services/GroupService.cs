using Microsoft.AspNetCore.Identity;
using Refit;
using Roomiebill.Server.Common;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Facades;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.Services
{
    public class GroupService
    {
        public readonly GroupFacade _groupFacade;
        private readonly UserFacade _userFacade;
        private readonly ILogger<GroupFacade> _groupFacadeLogger;
        private readonly GeminiService _geminiService;

        public GroupService(IApplicationDbContext groupsDb, ILogger<GroupFacade> groupFacadeLogger, UserService userService, GeminiService geminiService)
        {
            _userFacade = userService._userFacade;
            _groupFacade = new GroupFacade(groupsDb, groupFacadeLogger, _userFacade);
            _groupFacadeLogger = groupFacadeLogger;
            _geminiService = geminiService;
        }

        public async Task<Group> CreateNewGroupAsync(CreateNewGroupDto group)
        {
            Group newGroup = await _groupFacade.CreateNewGroupAsync(group);

            // Extract group members from UserFacade using usernames
            List<User> members = new List<User>();
            if (group.GroupMembersEmailsList != null)
            {
                foreach (string email in group.GroupMembersEmailsList)
                {
                    User? member = await _userFacade.GetUserByEmailAsync(email);

                    if (member == null)
                    {
                        _groupFacadeLogger.LogError($"Error when trying to create new group: member with email {email} does not exist in the system.");
                        throw new Exception($"Error when trying to create new group: member with email {email} does not exist in the system.");
                    }

                    members.Add(member);
                }
            }

            // Add the group to the admin's groups list
            await _userFacade.AddGroupToUser(group.AdminGroupUsername, newGroup.Id);

            return newGroup;
        }

        public async Task<List<Group>> GetUserGroupsAsync(int UserId)
        {
            // TODO: maybe add exception catch.
            List<Group> UserGroups = await _groupFacade.GetUserGroupsAsync(UserId);
            return UserGroups;
        }

        public async Task<Expense> AddExpenseAsync(ExpenseDto expense)
        {
            return await _groupFacade.AddExpenseAsync(expense);
        }
        // GetDebtsForUserAsync use the groupFacade to get the group details and calculate the debts for a user.

        public async Task<List<DebtDto>> GetDebtsForUserAsync(int groupId, int userId)
        {
            return await _groupFacade.GetDebtsForUserAsync(groupId, userId);
        }

        public async Task<Group> GetGroupByIdAsync(int groupId)
        {
            return await _groupFacade.GetGroupByIdAsync(groupId);
        }

        public async Task<List<DebtDto>> GetDebtsOwedByUserAsync(int groupId, int userId)
        {
         return await _groupFacade.GetDebtsOwedByUserAsync(groupId, userId);

        }
        //get group by id
        public async Task<Group> GetGroupAsync(int id)
        {
            return await _groupFacade.GetGroupByIdAsync(id);
        }


        public async Task SettleDebtAsync(decimal amount, User creditor, User debtor, int groupId){
            await _groupFacade.SettleDebtAsync(amount, creditor, debtor, groupId);
        }
        public async Task<List<Expense>> GetExpensesForGroupAsync(int groupId)
        {
            return await _groupFacade.GetExpensesForGroupAsync(groupId);
        }
        public async Task SnoozeMemberToPayAsync(SnoozeToPayDto snoozeInfo)
        {
            await _groupFacade.snoozeToUsernameAsync(snoozeInfo.snoozeToUsername, snoozeInfo.snoozeInfo);
        }
        public async Task<string> GetFeedbackFromGeminiAsync(string prompt)
        {
            return await _geminiService.GetFeedbackFromGeminiAsync(prompt);
        }

        public async Task ExitGroupAsync(int userId, int groupId)
        {
            await _groupFacade.ExitGroupAsync(userId, groupId);
        }

        public async Task DeleteGroupAsync(int groupId, int requestingUserId)
        {
            await _groupFacade.DeleteGroupAsync(groupId, requestingUserId);
        }
    }
}
