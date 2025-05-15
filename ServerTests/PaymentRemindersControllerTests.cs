using Microsoft.AspNetCore.Mvc;
using Moq;
using Roomiebill.Server.Controllers;
using Roomiebill.Server.Models;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.Common.Enums;
using Xunit;

namespace ServerTests
{
    public class PaymentRemindersControllerTests
    {
        private readonly Mock<IApplicationDbContext> _mockDbContext;
        private readonly PaymentRemindersController _controller;

        public PaymentRemindersControllerTests()
        {
            _mockDbContext = new Mock<IApplicationDbContext>();
            _controller = new PaymentRemindersController(_mockDbContext.Object);
        }

        [Fact]
        public async Task TestThatWhenCreatingReminderThenReturnsReminder()
        {
            var request = new PaymentRemindersController.CreateReminderRequest
            {
                UserId = 1,
                GroupId = 1,
                Category = Category.Electricity,
                RecurrencePattern = RecurrencePattern.Monthly,
                DayOfMonth = 1
            };

            User user = new User { Id = request.UserId };
            Group group = new Group { Id = request.GroupId, Members = new List<User> { user } };

            _mockDbContext.Setup(db => db.GetUserByIdAsync(request.UserId))
                         .ReturnsAsync(user);
            _mockDbContext.Setup(db => db.GetGroupByIdAsync(request.GroupId))
                         .ReturnsAsync(group);
            _mockDbContext.Setup(db => db.AddPaymentReminderAsync(It.IsAny<PaymentReminder>()))
                         .Returns(Task.CompletedTask);

            IActionResult result = await _controller.CreateReminder(request);
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.IsType<PaymentReminder>(okResult.Value);
        }

        [Fact]
        public async Task TestThatWhenCreatingReminderWithInvalidDayThenReturnsBadRequest()
        {
            var request = new PaymentRemindersController.CreateReminderRequest
            {
                UserId = 1,
                GroupId = 1,
                Category = Category.Electricity,
                RecurrencePattern = RecurrencePattern.Monthly,
                DayOfMonth = 29
            };

            IActionResult result = await _controller.CreateReminder(request);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Day of month must be between 1 and 28", badRequest.Value);
        }

        [Fact]
        public async Task TestThatWhenUserNotFoundThenReturnsNotFound()
        {
            var request = new PaymentRemindersController.CreateReminderRequest
            {
                UserId = 1,
                GroupId = 1,
                DayOfMonth = 1
            };

            _mockDbContext.Setup(db => db.GetUserByIdAsync(request.UserId))
                         .ReturnsAsync((User)null);

            IActionResult result = await _controller.CreateReminder(request);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task TestThatWhenGroupNotFoundThenReturnsNotFound()
        {
            var request = new PaymentRemindersController.CreateReminderRequest
            {
                UserId = 1,
                GroupId = 1,
                DayOfMonth = 1
            };

            User user = new User { Id = request.UserId };
            _mockDbContext.Setup(db => db.GetUserByIdAsync(request.UserId))
                         .ReturnsAsync(user);
            _mockDbContext.Setup(db => db.GetGroupByIdAsync(request.GroupId))
                         .ReturnsAsync((Group)null);

            IActionResult result = await _controller.CreateReminder(request);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task TestThatWhenUpdatingValidReminderThenReturnsReminder()
        {
            var request = new PaymentRemindersController.UpdateReminderRequest
            {
                Id = 1,
                Category = Category.Electricity,
                RecurrencePattern = RecurrencePattern.Monthly,
                DayOfMonth = 1,
                IsActive = true
            };

            PaymentReminder reminder = new PaymentReminder(1, 1, Category.Electricity, RecurrencePattern.Monthly, 1);
            _mockDbContext.Setup(db => db.GetPaymentReminderByIdAsync(request.Id))
                         .ReturnsAsync(reminder);
            _mockDbContext.Setup(db => db.UpdatePaymentReminderAsync(It.IsAny<PaymentReminder>()))
                         .Returns(Task.CompletedTask);

            IActionResult result = await _controller.UpdateReminder(request);
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.IsType<PaymentReminder>(okResult.Value);
        }

        [Fact]
        public async Task TestThatWhenGettingUserRemindersThenReturnsReminders()
        {
            int userId = 1;
            List<PaymentReminder> reminders = new List<PaymentReminder>
            {
                new PaymentReminder(userId, 1, Category.Electricity, RecurrencePattern.Monthly, 1),
                new PaymentReminder(userId, 1, Category.Water, RecurrencePattern.Monthly, 15)
            };

            _mockDbContext.Setup(db => db.GetActiveRemindersAsync())
                         .ReturnsAsync(reminders);

            IActionResult result = await _controller.GetUserReminders(userId);
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            var returnedReminders = Assert.IsType<List<PaymentReminder>>(okResult.Value);
            Assert.Equal(2, returnedReminders.Count);
        }

        [Fact]
        public async Task TestThatWhenDeletingReminderThenDeactivatesIt()
        {
            int reminderId = 1;
            PaymentReminder reminder = new PaymentReminder(1, 1, Category.Electricity, RecurrencePattern.Monthly, 1);

            _mockDbContext.Setup(db => db.GetPaymentReminderByIdAsync(reminderId))
                         .ReturnsAsync(reminder);
            _mockDbContext.Setup(db => db.UpdatePaymentReminderAsync(It.IsAny<PaymentReminder>()))
                         .Returns(Task.CompletedTask);

            IActionResult result = await _controller.DeleteReminder(reminderId);
            Assert.IsType<OkResult>(result);
            Assert.False(reminder.IsActive);
        }

        [Fact]
        public async Task TestThatWhenDeletingNonexistentReminderThenReturnsNotFound()
        {
            int reminderId = 1;
            _mockDbContext.Setup(db => db.GetPaymentReminderByIdAsync(reminderId))
                         .ReturnsAsync((PaymentReminder)null);

            IActionResult result = await _controller.DeleteReminder(reminderId);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task TestThatWhenUserNotInGroupThenReturnsBadRequest()
        {
            var request = new PaymentRemindersController.CreateReminderRequest
            {
                UserId = 1,
                GroupId = 1,
                DayOfMonth = 1
            };

            User user = new User { Id = request.UserId };
            Group group = new Group { Id = request.GroupId, Members = new List<User>() };

            _mockDbContext.Setup(db => db.GetUserByIdAsync(request.UserId))
                         .ReturnsAsync(user);
            _mockDbContext.Setup(db => db.GetGroupByIdAsync(request.GroupId))
                         .ReturnsAsync(group);

            IActionResult result = await _controller.CreateReminder(request);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User is not a member of the specified group", badRequest.Value);
        }
    }
}
