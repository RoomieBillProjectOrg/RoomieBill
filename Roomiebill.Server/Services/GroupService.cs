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

        public GroupService(IApplicationDbContext groupsDb, ILogger<GroupFacade> groupFacadeLogger, UserService userService)
        {
            _userFacade = userService._userFacade;
            _groupFacade = new GroupFacade(groupsDb, groupFacadeLogger, _userFacade);
        }

        public async Task<Group> CreateNewGroupAsync(CreateNewGroupDto group)
        {
            return await _groupFacade.CreateNewGroupAsync(group);
        }

        public async Task InviteToGroupByUsername(InviteToGroupByUsernameDto inviteDetails)
        {
            if (!await _userFacade.IsUserLoggedInAsync(inviteDetails.InviterUsername))
            {
                throw new Exception($"User with username {inviteDetails.InviterUsername} is not logged in.");
            } 
            await _groupFacade.InviteToGroupByUsername(inviteDetails.InviterUsername, inviteDetails.InvitedUsername, inviteDetails.GroupId);
        }

        public async Task<Expense> AddExpenseAsync(ExpenseDto expense)
        {
            return await _groupFacade.AddExpenseAsync(expense);
        }


    }
}
