using Roomiebill.Server.Common.Notification;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.Services
{
    public class PaymentReminderService : BackgroundService
    {
        private readonly ILogger<PaymentReminderService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public PaymentReminderService(
            ILogger<PaymentReminderService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendReminders();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing payment reminders");
                }

                // Wait until midnight
                var now = DateTime.UtcNow;
                var nextRun = now.Date.AddDays(1);
                var delay = nextRun - now;
                await Task.Delay(delay, stoppingToken);
            }
        }

        private async Task CheckAndSendReminders(DateTime? currentDate = null)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var activeReminders = await dbContext.GetActiveRemindersAsync();

            foreach (var reminder in activeReminders)
            {
                if (reminder.ShouldSendReminder(currentDate))
                {
                    await SendReminder(reminder);
                    reminder.UpdateLastReminderSent(currentDate);
                    await dbContext.UpdatePaymentReminderAsync(reminder);
                }
            }
        }

        private async Task SendReminder(PaymentReminder reminder)
        {
            var subject = $"Payment Reminder - {reminder.Category} for {reminder.Group.GroupName}";
            var body = $"Hi {reminder.User.Username},\n\n" +
                      $"This is a reminder to pay your {reminder.Category} bill for the group {reminder.Group.GroupName}.\n\n" +
                      $"Best regards,\nRoomieBill";

            await EmailNotificationHandler.SendEmailAsync(reminder.User.Email, subject, body);
            _logger.LogInformation(
                "Sent payment reminder for {Category} to user {UserId} in group {GroupId}",
                reminder.Category,
                reminder.UserId,
                reminder.GroupId
            );
        }
    }
}
