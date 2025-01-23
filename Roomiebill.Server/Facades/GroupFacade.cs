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
            Expense newExpense = MapToEntity(expenseDto);
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
            Expense updatedExpense = MapToEntity(updatedExpenseDto);

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
        public async Task AddMemberToGroupAsync(User user, Group group)
        {
            // Add the user to the group
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

        #region Help functions

        private User MapToEntity(RegisterUserDto dto)
        {
            if(dto == null)
            {
                return null;
            }
            else
            return new User
            {
                Id = dto.Id,
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = dto.Password
            };
        }

        private Expense MapToEntity(ExpenseDto dto)
        {
            return new Expense
            {
                Id = dto.Id,
                Amount = dto.Amount,
                Description = dto.Description,
                IsPaid = dto.IsPaid,
                PayerId = dto.PayerId,
                GroupId = dto.GroupId,
                ExpenseSplits = dto.ExpenseSplits.Select(splitDto => new ExpenseSplit
                {
                    UserId = splitDto.UserId,
                    Percentage = splitDto.Percentage
                }).ToList()
            };
        }

        #endregion
    }
}