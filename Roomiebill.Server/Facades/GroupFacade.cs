using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Roomiebill.Server.Common.Enums;
using Roomiebill.Server.Common.Notificaiton;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.Facades
{
    public class GroupFacade : IGroupFacade
    {
        private readonly IApplicationDbContext _applicationDbs;
        private ILogger<GroupFacade> _logger;
        private readonly IUserFacade _userFacade;

        public GroupFacade() {}
        public GroupFacade(IApplicationDbContext groupDb, ILogger<GroupFacade> logger, IUserFacade userFacade)
        {
            _applicationDbs = groupDb;
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

            // Check if user already has a group with the same name
            var userGroups = await GetUserGroupsAsync(admin.Id);
            if (userGroups.Any(g => g.GroupName.Equals(newGroupDto.GroupName, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogError($"Error when trying to create new group: user already has a group named '{newGroupDto.GroupName}'");
                throw new Exception($"You already have a group named '{newGroupDto.GroupName}'. Please choose a different name.");
            }

            // Create new group
            Group newGroup = new Group(newGroupDto.GroupName, admin, new List<User>());

            // Add group to database
            await _applicationDbs.AddGroupAsync(newGroup);

            return newGroup;
        }

        /// <summary>
        /// This method gets a group by its id. If the group does not exist, an exception is thrown.
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Group> GetGroupByIdAsync(int groupId)
        {
            _logger.LogInformation($"Getting group with id {groupId}.");

            Group? group = await _applicationDbs.GetGroupByIdAsync(groupId);

            if (group == null)
            {
                _logger.LogError($"Error when trying to get group: group with id {groupId} does not exist in the system.");
                throw new Exception($"Error when trying to get group: group with id {groupId} does not exist in the system.");
            }

            return group;
        }

        public bool IsUserInGroup(User user, Group group)
        {
            return group.Members.Contains(user) || group.Admin == user;
        }

        /// <summary>
        /// This method adds an expense to a group. If the group or the payer do not exist in the system, an exception is thrown.
        /// </summary>
        /// <param name="expenseDto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Expense> AddExpenseAsync(ExpenseDto expenseDto)
        {
            // Extract group from database
            Group? group = await _applicationDbs.GetGroupByIdAsync(expenseDto.GroupId);

            // Alert if the group does not exist
            if (group == null)
            {
                _logger.LogError($"Error when trying to add expense: group with id {expenseDto.GroupId} does not exist in the system.");
                throw new Exception($"Error when trying to add expense: group with id {expenseDto.GroupId} does not exist in the system.");
            }

            // Extract user from database
            User? user = await _userFacade.GetUserByIdAsync(expenseDto.PayerId);

            // Alert if the user does not exist
            if (user == null)
            {
                _logger.LogError($"Error when trying to add expense: user with id {expenseDto.PayerId} does not exist in the system.");
                throw new Exception($"Error when trying to add expense: user with id {expenseDto.PayerId} does not exist in the system.");
            }
            Expense newExpense = await MapToEntity(expenseDto);
            await _applicationDbs.UpdateGroupAsync(group);
            await AddExpenseSpiltsList(newExpense, expenseDto);
            
            // Add expense to group
            group.AddExpense(newExpense);

            await _applicationDbs.UpdateGroupAsync(group);

            return newExpense;
        }

        /// <summary>
        /// This method updates an expense in a group. If the expense does not exist in the group, an exception is thrown.
        /// </summary>
        /// <param name="oldExpenseDto"></param>
        /// <param name="updatedExpenseDto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Expense> UpdateExpenseAsync(ExpenseDto oldExpenseDto, ExpenseDto updatedExpenseDto)
        {
            // Extract the group from the database
            Group? group = await _applicationDbs.GetGroupByIdAsync(updatedExpenseDto.GroupId);

            // Alert if the group does not exist
            if (group == null)
            {
                _logger.LogError($"Error when trying to update expense: group with id {updatedExpenseDto.GroupId} does not exist.");
                throw new Exception($"Group with id {updatedExpenseDto.GroupId} does not exist.");
            }

            // Extract the old expense from the group
            Expense? oldExpense = group.Expenses.FirstOrDefault(e => e.Id == oldExpenseDto.Id);

            // Alert if the old expense does not exist
            if (oldExpense == null)
            {
                _logger.LogError($"Error when trying to update expense: expense with id {oldExpenseDto.Id} does not exist in the group.");
                throw new Exception($"Expense with id {oldExpenseDto.Id} does not exist in the group.");
            }

            // Map the updated DTO to an entity
            Expense updatedExpense = await MapToEntity(updatedExpenseDto);

            // Add the expense splits to the updated expense
            await AddExpenseSpiltsList(updatedExpense, updatedExpenseDto);

            // Use the Group's updateExpense method
            group.updateExpense(oldExpense, updatedExpense);

            await _applicationDbs.UpdateGroupAsync(group);

            _logger.LogInformation($"Expense with id {updatedExpenseDto.Id} updated successfully.");

            return updatedExpense;
        }

        /// <summary>
        /// This method adds a user to a group. If the user is already a member of the group, an exception is thrown.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public async Task AddMemberToGroupAsync(User user, Group groupToLook)
        {
            // Add the user to the group
            Group group = await _applicationDbs.GetGroupByIdAsync(groupToLook.Id);
            if (group == null)
            {
                _logger.LogError($"Error when trying to add member: group with id {groupToLook.Id} does not exist.");
                throw new Exception($"Group with id {groupToLook.Id} does not exist.");
            }
            group.AddMember(user);
            await _applicationDbs.UpdateGroupAsync(group);

            // Add the group to the user
            user.AddGroup(group);
            await _applicationDbs.UpdateUserAsync(user);
            
            NotificationsHandle.SendNotificationByTopicAsync("Group Notification", $"{user.Username} joined the group.", $"Group_{group.Id}");

            _logger.LogInformation($"User with id {user.Id} added to group successfully.");
        }

        /// <summary>
        /// This method removes a user from a group. If the user is not a member of the group, an exception is thrown.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task RemoveMemberAsync(User user, int groupId){
            // Extract the group from the database
            Group? group = await _applicationDbs.GetGroupByIdAsync(groupId);

            // Alert if the group does not exist
            if (group == null)
            {
                _logger.LogError($"Error when trying to remove member: group with id {groupId} does not exist.");
                throw new Exception($"Group with id {groupId} does not exist.");
            }

            // Remove the user from the group
            group.RemoveMember(user);

            await _applicationDbs.UpdateGroupAsync(group);

            _logger.LogInformation($"User with id {user.Id} removed from group with id {groupId} successfully.");
        }

        public async Task<List<Group>> GetUserGroupsAsync(int UserId){
            List<Group> UserGroups = await _applicationDbs.GetUserGroupsAsync(UserId);
            if (UserGroups == null){
                _logger.LogError($"Error when trying to retrieve user {UserId} groups.");
                throw new Exception($"Couldn't load groups for user {UserId}");
            }
            _logger.LogInformation($"User with id {UserId} retrieved his groups successfully.");
            return UserGroups;
        }

        //GetDebtsForUserAsync
        public async Task<List<DebtDto>> GetDebtsForUserAsync(int groupId, int userId)
        {
            Group? group = await _applicationDbs.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                _logger.LogError($"Error when trying to get debts for user: group with id {groupId} does not exist.");
                throw new Exception($"Group with id {groupId} does not exist.");
            }

            User? user = await _userFacade.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogError($"Error when trying to get debts for user: user with id {userId} does not exist.");
                throw new Exception($"User with id {userId} does not exist.");
            }

            List<DebtDto> debts = group.GetDebtsForUser(userId);
            return debts;
        }
        //GetDebtsOwedByUserAsync
        public async Task<List<DebtDto>> GetDebtsOwedByUserAsync(int groupId, int userId)
        {
            Group? group = await _applicationDbs.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                _logger.LogError($"Error when trying to get debts owed by user: group with id {groupId} does not exist.");
                throw new Exception($"Group with id {groupId} does not exist.");
            }

            User? user = await _userFacade.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogError($"Error when trying to get debts owed by user: user with id {userId} does not exist.");
                throw new Exception($"User with id {userId} does not exist.");
            }

            List<DebtDto> debts = group.GetDebtsOwedByUser(userId);
            return debts;
        }
        

        public async Task SettleDebtAsync(decimal amount, User creditor, User debtor, int groupId){
            Group group = await GetGroupByIdAsync(groupId);
            group.SettleDebt(amount, creditor.Id, debtor.Id);
            await _applicationDbs.UpdateGroupAsync(group);
            _logger.LogInformation($"User {debtor.Username} returned his debt of {amount} NIS to {creditor.Username} successfully.");
        }

        public async Task snoozeToUsernameAsync(string username, string snoozeInfo)
        {
            User userToSnooze = await _userFacade.GetUserByUsernameAsync(username);
            if (userToSnooze == null)
            {
                _logger.LogError($"Error when trying to snooze user: user with username {username} does not exist.");
                throw new Exception($"User with username {username} does not exist.");
            }
            NotificationsHandle.SendNotificationByTokenAsync("Pay Reminder", snoozeInfo, userToSnooze.FirebaseToken);
        }

        public async Task DeleteGroupAsync(int groupId, int requestingUserId)
        {
            // Get the group
            Group? group = await GetGroupByIdAsync(groupId);
            
            // Check if requesting user is admin
            if (group.Admin.Id != requestingUserId)
            {
                _logger.LogError($"Error when trying to delete group: user with id {requestingUserId} is not the admin of group with id {groupId}.");
                throw new Exception($"Only the group admin can delete the group.");
            }

            // Check if group can be deleted (no debts)
            if (!group.CanDeleteGroup())
            {
                _logger.LogError($"Error when trying to delete group: group with id {groupId} has unsettled debts.");
                throw new Exception($"Cannot delete group while there are unsettled debts.");
            }

            // Delete any pending invites for the group
            await _applicationDbs.DeleteInvitesByGroupIdAsync(groupId);

            // Delete the group
            await _applicationDbs.DeleteGroupAsync(groupId);

            _logger.LogInformation($"Group with id {groupId} and all its pending invites deleted successfully by admin (id: {requestingUserId}).");
        }

        #region Help functions

        private async Task<Expense> MapToEntity(ExpenseDto dto)
        {
            // Get the next available expense id from db
            //int nextId = await _applicationDbs.GetNextExpenseIdAsync();
            Expense e = new Expense
            {
                Amount = dto.Amount,
                Description = dto.Description,
                IsPaid = dto.IsPaid,
                PayerId = dto.PayerId,
                Payer = await _userFacade.GetUserByIdAsync(dto.PayerId),
                GroupId = dto.GroupId,
                Group = await GetGroupByIdAsync(dto.GroupId),
                Category = dto.Category,
                StartMonth = dto.StartMonth,
                EndMonth = dto.EndMonth,
                ReceiptString = dto.ReceiptString
            };
            
            return e;
        }

        private async Task AddExpenseSpiltsList(Expense e, ExpenseDto dto)
        {
            List<ExpenseSplit> expenseSplits = new List<ExpenseSplit>();

            //int nextSplitId = await _applicationDbs.GetNextExpenseSplitIdAsync();
            foreach (ExpenseSplitDto es in dto.ExpenseSplits){
                expenseSplits.Add(new ExpenseSplit
                    {   
                        ExpenseId = e.Id,
                        Expense = e,
                        UserId = es.UserId,
                        User = await _userFacade.GetUserByIdAsync(es.UserId),
                        Amount = es.Amount
                    });
            }
            e.ExpenseSplits = expenseSplits;
        }

        internal async Task<List<Expense>> GetExpensesForGroupAsync(int groupId){
            Group group = await GetGroupByIdAsync(groupId);
            return (List<Expense>)group.Expenses;
        }

        public async Task ExitGroupAsync(int userId, int groupId)
        {
            // Get user and group
            User? user = await _userFacade.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogError($"Error when trying to exit group: user with id {userId} does not exist.");
                throw new Exception($"User with id {userId} does not exist.");
            }

            Group? group = await GetGroupByIdAsync(groupId);
            if (group == null)
            {
                _logger.LogError($"Error when trying to exit group: group with id {groupId} does not exist.");
                throw new Exception($"Group with id {groupId} does not exist.");
            }

            // Check if user is in group
            if (!IsUserInGroup(user, group))
            {
                _logger.LogError($"Error when trying to exit group: user with id {userId} is not a member of group with id {groupId}.");
                throw new Exception($"User is not a member of this group.");
            }

            // Check if user is admin
            if (group.Admin.Id == userId)
            {
                _logger.LogError($"Error when trying to exit group: user with id {userId} is the admin of group with id {groupId}.");
                throw new Exception($"Admin cannot exit the group. Transfer admin role first or delete the group.");
            }

            // Check if user can exit (no debts)
            if (!group.CanUserExitGroup(userId))
            {
                _logger.LogError($"Error when trying to exit group: user with id {userId} has unsettled debts in group with id {groupId}.");
                throw new Exception($"Cannot exit group with unsettled debts.");
            }

            // Remove user from group
            await RemoveMemberAsync(user, groupId);

            _logger.LogInformation($"User with id {userId} exited group with id {groupId} successfully.");
        }

        #endregion
    }
}
