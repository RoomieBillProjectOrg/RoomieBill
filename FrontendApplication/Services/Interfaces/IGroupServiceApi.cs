using FrontendApplication.Models;

namespace FrontendApplication.Services.Interfaces
{
    public interface IGroupServiceApi
    {
        Task<List<GroupModel>> GetUserGroups(UserModel user);
        Task<GroupModel> GetGroup(int groupId);
        Task<List<DebtModel>> GetDebtsForUserAsync(int groupId, int userId);
        Task<List<DebtModel>> GetDebtsOwedByUserAsync(int groupId, int userId);
        Task addExpenseAsync(ExpenseModel expenseDto);
        Task InviteUserToGroupByEmailAsync(InviteToGroupByEmailDto inviteDto);
        Task<List<ExpenseModel>> GetExpensesForGroupAsync(int groupId);
        Task SnoozeMember(SnoozeToPayDto snoozeInfo);
        Task<string> GetGeiminiResponseForExpenses(int groupId);
        Task DeleteGroupAsync(int groupId, int userId);
        Task ExitGroupAsync(int userId, int groupId);
    }
}
