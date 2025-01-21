
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.Facades
{
    public interface IGroupFacade
    {
        Task<Group> CreateNewGroupAsync(CreateNewGroupDto newGroupDto);

        Task<Group> GetGroupByIdAsync(int groupId);

        Task<Expense> AddExpenseAsync(ExpenseDto expenseDto);

        Task<Expense> UpdateExpenseAsync(ExpenseDto oldExpenseDto, ExpenseDto updatedExpenseDto);

        Task AddMemberToGroupAsync(User user, Group group);

        Task RemoveMemberAsync(User user, int groupId);

        Task<List<Group>> GetUserGroupsAsync(int userId);

        bool IsUserInGroup(User user, Group group);
    }
}