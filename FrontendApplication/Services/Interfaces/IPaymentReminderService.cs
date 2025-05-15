using FrontendApplication.Models;

namespace FrontendApplication.Services.Interfaces
{
    public interface IPaymentReminderService
    {
        Task<List<PaymentReminderModel>> GetUserReminders(int userId);
        Task<PaymentReminderModel> CreateReminder(CreatePaymentReminderRequest request);
        Task<PaymentReminderModel> UpdateReminder(UpdatePaymentReminderRequest request);
        Task DeleteReminder(int reminderId);
    }
}
