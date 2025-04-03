using Xunit;
using Moq;
using Roomiebill.Server.Controllers;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.Models;
using Roomiebill.Server.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ServerTests
{
    public class PaymentReminderTests
    {
        [Fact]
        public void PaymentReminder_Constructor_ValidData_Success()
        {
            // Arrange
            int userId = 1;
            int groupId = 1;
            var category = Category.Gas;
            var recurrencePattern = RecurrencePattern.Monthly;
            int dayOfMonth = 15;

            // Act
            var reminder = new PaymentReminder(userId, groupId, category, recurrencePattern, dayOfMonth);

            // Assert
            Assert.Equal(userId, reminder.UserId);
            Assert.Equal(groupId, reminder.GroupId);
            Assert.Equal(category, reminder.Category);
            Assert.Equal(recurrencePattern, reminder.RecurrencePattern);
            Assert.Equal(dayOfMonth, reminder.DayOfMonth);
            Assert.True(reminder.IsActive);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(29)]
        [InlineData(-1)]
        [InlineData(32)]
        public void PaymentReminder_Constructor_InvalidDayOfMonth_ThrowsException(int invalidDay)
        {
            // Arrange
            int userId = 1;
            int groupId = 1;
            var category = Category.Gas;
            var recurrencePattern = RecurrencePattern.Monthly;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                new PaymentReminder(userId, groupId, category, recurrencePattern, invalidDay));
        }

        [Theory]
        [InlineData(-2, false)] // Too early
        [InlineData(-1, true)]  // Exactly one month ago
        [InlineData(0, false)]  // Too recent
        public void ShouldSendReminder_Monthly_ChecksExactMonth(int monthsToAdd, bool expectedResult)
        {
            // Arrange
            var today = DateTime.UtcNow;
            var reminder = new PaymentReminder(1, 1, Category.Gas, RecurrencePattern.Monthly, today.Day);
            reminder.LastReminderSent = today.AddMonths(monthsToAdd);

            // Act
            var shouldSend = reminder.ShouldSendReminder();

            // Assert
            Assert.Equal(expectedResult, shouldSend);
        }

        [Theory]
        [InlineData(-3, false)] // Too early
        [InlineData(-2, true)]  // Exactly two months ago
        [InlineData(-1, false)] // Too recent
        [InlineData(0, false)]  // Too recent
        public void ShouldSendReminder_BiMonthly_ChecksExactTwoMonths(int monthsToAdd, bool expectedResult)
        {
            // Arrange
            var today = DateTime.UtcNow;
            var reminder = new PaymentReminder(1, 1, Category.Gas, RecurrencePattern.BiMonthly, today.Day);
            reminder.LastReminderSent = today.AddMonths(monthsToAdd);

            // Act
            var shouldSend = reminder.ShouldSendReminder();

            // Assert
            Assert.Equal(expectedResult, shouldSend);
        }

        [Fact]
        public async Task CreateReminder_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var mockDbContext = new Mock<IApplicationDbContext>();
            var controller = new PaymentRemindersController(mockDbContext.Object);
            
            var user = new User { Id = 1, Username = "testUser" };
            var group = new Group("TestGroup", user, new List<User> { user });
            group.Id = 1;

            mockDbContext.Setup(db => db.GetUserByIdAsync(1))
                .ReturnsAsync(user);
            mockDbContext.Setup(db => db.GetGroupByIdAsync(1))
                .ReturnsAsync(group);

            var request = new PaymentRemindersController.CreateReminderRequest
            {
                UserId = 1,
                GroupId = 1,
                Category = Category.Gas,
                RecurrencePattern = RecurrencePattern.Monthly,
                DayOfMonth = 15
            };

            // Act
            var result = await controller.CreateReminder(request);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            mockDbContext.Verify(db => db.AddPaymentReminderAsync(It.IsAny<PaymentReminder>()), Times.Once);
        }

        [Fact]
        public async Task CreateReminder_InvalidDayOfMonth_ReturnsBadRequest()
        {
            // Arrange
            var mockDbContext = new Mock<IApplicationDbContext>();
            var controller = new PaymentRemindersController(mockDbContext.Object);

            var request = new PaymentRemindersController.CreateReminderRequest
            {
                UserId = 1,
                GroupId = 1,
                Category = Category.Gas,
                RecurrencePattern = RecurrencePattern.Monthly,
                DayOfMonth = 29 // Invalid day
            };

            // Act
            var result = await controller.CreateReminder(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteReminder_ExistingReminder_SoftDeletesAndReturnsOk()
        {
            // Arrange
            var mockDbContext = new Mock<IApplicationDbContext>();
            var controller = new PaymentRemindersController(mockDbContext.Object);
            
            var reminder = new PaymentReminder(1, 1, Category.Gas, RecurrencePattern.Monthly, 15);
            reminder.Id = 1;

            mockDbContext.Setup(db => db.GetPaymentReminderByIdAsync(1))
                .ReturnsAsync(reminder);

            // Act
            var result = await controller.DeleteReminder(1);

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.False(reminder.IsActive);
            mockDbContext.Verify(db => db.UpdatePaymentReminderAsync(It.Is<PaymentReminder>(r => !r.IsActive)), Times.Once);
        }
    }
}
