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

                if (member == null)
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

            Group? group = await _groupDb.GetGroupByIdAsync(groupId, query => query.Include(g => g.Invites).ThenInclude(i => i.Invited)
                .Include(g => g.Members));

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

            if (!IsUserInGroup(inviter, group))
            {
                _logger.LogError($"Error when trying to invite user to group: user with username {inviter_username} is not a member of group with id {groupId}.");
                throw new Exception($"Error when trying to invite user to group: user with username {inviter_username} is not a member of group with id {groupId}.");
            }

            if (IsUserInGroup(invited, group))
            {
                _logger.LogError($"Error when trying to invite user to group: user with username {invited_username} is already a member of group with id {groupId}.");
                throw new Exception($"Error when trying to invite user to group: user with username {invited_username} is already a member of group with id {groupId}.");
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

        private bool IsUserInGroup(User user, Group group)
        {
            return group.Members.Contains(user);
        }
        
        #endregion 

        public async Task<Expense> AddExpenseAsync(ExpenseDto expenseDto)
        {
            // Extract group from database
            Group? group = _groupDb.GetGroupById(expenseDto.GroupId);

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

            return newExpense;
        }
        public async Task<Expense> UpdateExpenseAsync(ExpenseDto oldExpenseDto, ExpenseDto updatedExpenseDto)
        {
            // Extract the group from the database
            Group? group = await Task.Run(() => _groupDb.GetGroupById(updatedExpenseDto.GroupId));

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

            _logger.LogInformation($"Expense with id {updatedExpenseDto.Id} updated successfully.");

            return updatedExpense;
        }
        public async Task AddMemberAsync(User user, int groupId)
        {
            // Extract the group from the database
            Group? group = await Task.Run(() => _groupDb.GetGroupById(groupId));

            // Alert if the group does not exist
            if (group == null)
            {
                _logger.LogError($"Error when trying to add member: group with id {groupId} does not exist.");
                throw new Exception($"Group with id {groupId} does not exist.");
            }
            // User newUser = MapToEntity(user);

            // Add the user to the group
            group.AddMember(user);

            _logger.LogInformation($"User with id {user.Id} added to group with id {groupId} successfully.");

        }

        public async Task RemoveMemberAsync(User user, int groupId){
            // Extract the group from the database
            Group? group = await Task.Run(() => _groupDb.GetGroupById(groupId));

            // Alert if the group does not exist
            if (group == null)
            {
                _logger.LogError($"Error when trying to remove member: group with id {groupId} does not exist.");
                throw new Exception($"Group with id {groupId} does not exist.");
            }

            // Remove the user from the group
            group.RemoveMember(user);

            _logger.LogInformation($"User with id {user.Id} removed from group with id {groupId} successfully.");
        }
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

    }
}