using Microsoft.AspNetCore.Mvc;
using Moq;
using Roomiebill.Server.Common.Enums;
using Roomiebill.Server.Controllers;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.Models;
using Xunit;
using static Roomiebill.Server.Controllers.PaymentRemindersController;

namespace ServerTests.AcceptanceTests
{
    public class PaymentRemindersControllerAcceptanceTests
    {
        [Fact]
        public async Task CreateReminder_WithValidData_ShouldSucceed()
        {
            // Arrange
            var request = new CreateReminderRequest
            {
                UserId = 1,
                GroupId = 1,
                Category = Category.Electricity,
                RecurrencePattern = RecurrencePattern.Monthly,
                DayOfMonth = 15
            };

            var controller = CreateController(
                userExists: true,
                groupExists: true,
                isUserInGroup: true);

            // Act
            var result = await controller.CreateReminder(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var reminder = Assert.IsType<PaymentReminder>(okResult.Value);
            Assert.Equal(request.UserId, reminder.UserId);
            Assert.Equal(request.GroupId, reminder.GroupId);
            Assert.Equal(request.Category, reminder.Category);
        }

        [Fact]
        public async Task CreateReminder_WithInvalidDayOfMonth_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new CreateReminderRequest
            {
                UserId = 1,
                GroupId = 1,
                Category = Category.Electricity,
                RecurrencePattern = RecurrencePattern.Monthly,
                DayOfMonth = 30 // Invalid day
            };

            var controller = CreateController(
                userExists: true,
                groupExists: true,
                isUserInGroup: true);

            // Act
            var result = await controller.CreateReminder(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Day of month must be between 1 and 28", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateReminder_WithNonexistentUser_ShouldReturnNotFound()
        {
            // Arrange
            var request = new CreateReminderRequest
            {
                UserId = 999,
                GroupId = 1,
                Category = Category.Electricity,
                RecurrencePattern = RecurrencePattern.Monthly,
                DayOfMonth = 15
            };

            var controller = CreateController(
                userExists: false,
                groupExists: true,
                isUserInGroup: false);

            // Act
            var result = await controller.CreateReminder(request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateReminder_WithValidData_ShouldSucceed()
        {
            // Arrange
            var request = new UpdateReminderRequest
            {
                Id = 1,
                Category = Category.Water,
                RecurrencePattern = RecurrencePattern.Monthly,
                DayOfMonth = 20,
                IsActive = true
            };

            var controller = CreateController(reminderExists: true);

            // Act
            var result = await controller.UpdateReminder(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var reminder = Assert.IsType<PaymentReminder>(okResult.Value);
            Assert.Equal(request.Category, reminder.Category);
            Assert.Equal(request.DayOfMonth, reminder.DayOfMonth);
        }

        [Fact]
        public async Task GetUserReminders_ShouldReturnActiveReminders()
        {
            // Arrange
            int userId = 1;
            var controller = CreateController(hasActiveReminders: true);

            // Act
            var result = await controller.GetUserReminders(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var reminders = Assert.IsType<List<PaymentReminder>>(okResult.Value);
            Assert.NotEmpty(reminders);
            Assert.All(reminders, r => Assert.Equal(userId, r.UserId));
        }

        [Fact]
        public async Task DeleteReminder_ShouldSoftDelete()
        {
            // Arrange
            int reminderId = 1;
            var controller = CreateController(reminderExists: true);

            // Act
            var result = await controller.DeleteReminder(reminderId);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        private PaymentRemindersController CreateController(
            bool userExists = true,
            bool groupExists = true,
            bool isUserInGroup = true,
            bool reminderExists = true,
            bool hasActiveReminders = true)
        {
            var mockDbContext = new Mock<IApplicationDbContext>();

            if (userExists)
            {
                mockDbContext.Setup(db => db.GetUserByIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(new User { Id = 1, Username = "testuser" });
            }

            if (groupExists)
            {
                var group = new Group { Id = 1 };
                if (isUserInGroup)
                {
                    group.Members = new List<User> { new User { Id = 1 } };
                }
                mockDbContext.Setup(db => db.GetGroupByIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(group);
            }

            if (reminderExists)
            {
                mockDbContext.Setup(db => db.GetPaymentReminderByIdAsync(It.IsAny<int>()))
                    .ReturnsAsync(new PaymentReminder(1, 1, Category.Electricity, RecurrencePattern.Monthly, 15));
            }

            if (hasActiveReminders)
            {
                mockDbContext.Setup(db => db.GetActiveRemindersAsync())
                    .ReturnsAsync(new List<PaymentReminder>
                    {
                        new PaymentReminder(1, 1, Category.Electricity, RecurrencePattern.Monthly, 15),
                        new PaymentReminder(1, 1, Category.Water, RecurrencePattern.Monthly, 20)
                    });
            }

            mockDbContext.Setup(db => db.AddPaymentReminderAsync(It.IsAny<PaymentReminder>()))
                .Returns(Task.CompletedTask);

            mockDbContext.Setup(db => db.UpdatePaymentReminderAsync(It.IsAny<PaymentReminder>()))
                .Returns(Task.CompletedTask);

            return new PaymentRemindersController(mockDbContext.Object);
        }
    }
}
