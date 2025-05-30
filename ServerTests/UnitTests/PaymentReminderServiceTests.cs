using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Roomiebill.Server.Services;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.Models;
using Roomiebill.Server.Common.Enums;
using System.Reflection;

namespace ServerTests.UnitTests
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
            // Use a fixed date for testing
            const int validDay = 15;
            var testDate = new DateTime(2025, 6, validDay);
            var user = new User { Id = 1, Username = "testUser", Email = "test@example.com" };
            var group = new Group("TestGroup", user, new List<User> { user });

            var activeReminder = new PaymentReminder(1, 1, Category.Gas, RecurrencePattern.Monthly, validDay);
            activeReminder.User = user;
            activeReminder.Group = group;
            activeReminder.LastReminderSent = testDate.AddMonths(-1);

            var mockScopeServiceProvider = new Mock<IServiceProvider>();
            mockScopeServiceProvider.Setup(x => x.GetService(typeof(IApplicationDbContext)))
                .Returns(_mockDbContext.Object);
            _mockScope.Setup(s => s.ServiceProvider).Returns(mockScopeServiceProvider.Object);

            var inactiveReminder = new PaymentReminder(1, 1, Category.Water, RecurrencePattern.Monthly, validDay);
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
            // Pass test date as parameter array for the private method
            await (Task)method.Invoke(service, new object[] { testDate });

            // Assert
            _mockDbContext.Verify(db => db.UpdatePaymentReminderAsync(
                It.Is<PaymentReminder>(r =>
                    r.UserId == activeReminder.UserId &&
                    r.LastReminderSent.Date == testDate.Date)),
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

            // Act - pass null as DateTime? parameter
            await (Task)method.Invoke(service, new object[] { null });

            // Assert
            _mockDbContext.Verify(db => db.UpdatePaymentReminderAsync(It.IsAny<PaymentReminder>()), Times.Never);
        }

        [Fact]
        public async Task CheckAndSendReminders_WrongDayForReminder_DoesNotSendEmails()
        {
            // Arrange
            var testDate = new DateTime(2025, 6, 15);
            var user = new User { Id = 1, Username = "testUser", Email = "test@example.com" };
            var group = new Group("TestGroup", user, new List<User> { user });

            // Create reminder for day 16 but test with day 15
            var reminder = new PaymentReminder(1, 1, Category.Gas, RecurrencePattern.Monthly, 16);
            reminder.User = user;
            reminder.Group = group;
            reminder.LastReminderSent = testDate.AddMonths(-1);

            var reminders = new List<PaymentReminder> { reminder };
            _mockDbContext.Setup(db => db.GetActiveRemindersAsync())
                .ReturnsAsync(reminders);

            var service = new PaymentReminderService(_mockLogger.Object, _mockServiceProvider.Object);

            // Use reflection to access and invoke the private CheckAndSendReminders method
            var method = typeof(PaymentReminderService).GetMethod("CheckAndSendReminders",
                BindingFlags.NonPublic | BindingFlags.Instance);

            // Act - use testDate (day 15) to check against reminder set for day 16
            await (Task)method.Invoke(service, new object[] { testDate });

            // Assert
            _mockDbContext.Verify(db => db.UpdatePaymentReminderAsync(It.IsAny<PaymentReminder>()), Times.Never);
        }
    }
}
