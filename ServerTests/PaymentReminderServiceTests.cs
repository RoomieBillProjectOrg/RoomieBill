using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Roomiebill.Server.Services;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.Models;
using Roomiebill.Server.Common.Enums;
using System.Reflection;

namespace ServerTests
{
    public class PaymentReminderServiceTests
    {
        private readonly Mock<ILogger<PaymentReminderService>> _mockLogger;
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<IServiceScope> _mockScope;
        private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
        private readonly Mock<IApplicationDbContext> _mockDbContext;

        public PaymentReminderServiceTests()
        {
            _mockLogger = new Mock<ILogger<PaymentReminderService>>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockScope = new Mock<IServiceScope>();
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _mockDbContext = new Mock<IApplicationDbContext>();

            _mockScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);
            _mockScopeFactory.Setup(f => f.CreateScope()).Returns(_mockScope.Object);
            _mockServiceProvider.Setup(s => s.GetService(typeof(IServiceScopeFactory)))
                .Returns(_mockScopeFactory.Object);
            _mockServiceProvider.Setup(s => s.GetService(typeof(IApplicationDbContext)))
                .Returns(_mockDbContext.Object);
        }

        [Fact]
        public async Task CheckAndSendReminders_WithActiveReminders_SendsEmailsForDueReminders()
        {
            // Arrange
            var today = DateTime.UtcNow;
            var user = new User { Id = 1, Username = "testUser", Email = "test@example.com" };
            var group = new Group("TestGroup", user, new List<User> { user });
            
            var activeReminder = new PaymentReminder(1, 1, Category.Gas, RecurrencePattern.Monthly, today.Day);
            activeReminder.User = user;
            activeReminder.Group = group;
            activeReminder.LastReminderSent = today.AddMonths(-1);

            var inactiveReminder = new PaymentReminder(1, 1, Category.Water, RecurrencePattern.Monthly, today.Day);
            inactiveReminder.IsActive = false;
            inactiveReminder.User = user;
            inactiveReminder.Group = group;

            var reminders = new List<PaymentReminder> { activeReminder };
            _mockDbContext.Setup(db => db.GetActiveRemindersAsync())
                .ReturnsAsync(reminders);

            var service = new PaymentReminderService(_mockLogger.Object, _mockServiceProvider.Object);

            // Use reflection to access and invoke the private CheckAndSendReminders method
            var method = typeof(PaymentReminderService).GetMethod("CheckAndSendReminders", 
                BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            await (Task)method.Invoke(service, null);

            // Assert
            _mockDbContext.Verify(db => db.UpdatePaymentReminderAsync(
                It.Is<PaymentReminder>(r => 
                    r.UserId == activeReminder.UserId && 
                    r.LastReminderSent.Date == today.Date)), 
                Times.Once);
        }

        [Fact]
        public async Task CheckAndSendReminders_NoActiveReminders_DoesNotSendEmails()
        {
            // Arrange
            _mockDbContext.Setup(db => db.GetActiveRemindersAsync())
                .ReturnsAsync(new List<PaymentReminder>());

            var service = new PaymentReminderService(_mockLogger.Object, _mockServiceProvider.Object);

            // Use reflection to access and invoke the private CheckAndSendReminders method
            var method = typeof(PaymentReminderService).GetMethod("CheckAndSendReminders", 
                BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            await (Task)method.Invoke(service, null);

            // Assert
            _mockDbContext.Verify(db => db.UpdatePaymentReminderAsync(It.IsAny<PaymentReminder>()), Times.Never);
        }

        [Fact]
        public async Task CheckAndSendReminders_WrongDayForReminder_DoesNotSendEmails()
        {
            // Arrange
            var today = DateTime.UtcNow;
            var user = new User { Id = 1, Username = "testUser", Email = "test@example.com" };
            var group = new Group("TestGroup", user, new List<User> { user });
            
            var reminder = new PaymentReminder(1, 1, Category.Gas, RecurrencePattern.Monthly, today.Day == 1 ? 2 : 1);
            reminder.User = user;
            reminder.Group = group;
            reminder.LastReminderSent = today.AddMonths(-1);

            var reminders = new List<PaymentReminder> { reminder };
            _mockDbContext.Setup(db => db.GetActiveRemindersAsync())
                .ReturnsAsync(reminders);

            var service = new PaymentReminderService(_mockLogger.Object, _mockServiceProvider.Object);

            // Use reflection to access and invoke the private CheckAndSendReminders method
            var method = typeof(PaymentReminderService).GetMethod("CheckAndSendReminders", 
                BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            await (Task)method.Invoke(service, null);

            // Assert
            _mockDbContext.Verify(db => db.UpdatePaymentReminderAsync(It.IsAny<PaymentReminder>()), Times.Never);
        }
    }
}
