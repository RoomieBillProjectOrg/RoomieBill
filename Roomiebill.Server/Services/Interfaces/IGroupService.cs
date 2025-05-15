using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Facades;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.Services.Interfaces
{
    public interface IGroupService
    {
        Task<Group> CreateNewGroupAsync(CreateNewGroupDto group);
        Task<List<Group>> GetUserGroupsAsync(int UserId);
        Task<Expense> AddExpenseAsync(ExpenseDto expense);
        Task<List<DebtDto>> GetDebtsForUserAsync(int groupId, int userId);
        Task<Group> GetGroupByIdAsync(int groupId);
        Task<List<DebtDto>> GetDebtsOwedByUserAsync(int groupId, int userId);
        Task<Group> GetGroupAsync(int id);
        Task SettleDebtAsync(decimal amount, User creditor, User debtor, int groupId);
        Task<List<Expense>> GetExpensesForGroupAsync(int groupId);
        Task SnoozeMemberToPayAsync(SnoozeToPayDto snoozeInfo);
        Task<string> GetFeedbackFromGeminiAsync(string prompt);
        Task<string> ExtractDataFromTextWithGeminiAsync(string prompt);
        Task ExitGroupAsync(int userId, int groupId);
        Task DeleteGroupAsync(int groupId, int requestingUserId);
        public GroupFacade GetGroupFacade();
    }
}
