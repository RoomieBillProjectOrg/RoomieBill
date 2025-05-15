using Microsoft.Extensions.Hosting;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.Services.Interfaces
{
    public interface IPaymentReminderService : IHostedService
    {
        // Note: ExecuteAsync is already defined in IHostedService
        // Additional methods specific to PaymentReminderService can be added here
        Task SendReminder(PaymentReminder reminder);
    }
}
