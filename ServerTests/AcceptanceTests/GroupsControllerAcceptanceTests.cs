using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Roomiebill.Server.Controllers;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services.Interfaces;
using Roomiebill.Server.Common;
using Roomiebill.Server.Common.Enums;
using Xunit;

namespace ServerTests.AcceptanceTests
{
    public class GroupsControllerAcceptanceTests
    {
        [Fact]
        public async Task CreateGroup_WithValidData_ShouldSucceed()
        {
            // Arrange
            var groupDto = new CreateNewGroupDto
            {
                GroupName = "Test Group",
                AdminGroupUsername = "admin",
                GroupMembersEmailsList = new List<string> { "member1@test.com", "member2@test.com" }
            };

            // Act
            var controller = CreateController();
            var result = await controller.CreateGroup(groupDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var group = Assert.IsType<Group>(okResult.Value);
            Assert.Equal(groupDto.GroupName, group.GroupName);
        }

        [Fact]
        public async Task GetUserGroups_WithValidUserId_ShouldReturnGroups()
        {
            // Arrange
            int userId = 1;

            // Act
            var controller = CreateController();
            var result = await controller.GetUserGroups(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var groups = Assert.IsType<List<Group>>(okResult.Value);
            Assert.NotNull(groups);
        }

        [Fact]
        public async Task GetGroup_WithValidId_ShouldReturnGroup()
        {
            // Arrange
            int groupId = 1;

            // Act
            var controller = CreateController();
            var result = await controller.GetGroup(groupId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var group = Assert.IsType<Group>(okResult.Value);
            Assert.NotNull(group);
        }

        [Fact]
        public async Task AddExpense_WithValidData_ShouldSucceed()
        {
            // Arrange
            var expenseDto = new ExpenseDto
            {
                GroupId = 1,
                PayerId = 1,
                Amount = 100.50,
                Description = "Groceries",
                Category = Category.Other,
                ExpenseSplits = new List<ExpenseSplitDto> 
                {
                    new ExpenseSplitDto { UserId = 1, Amount = 33.50 },
                    new ExpenseSplitDto { UserId = 2, Amount = 33.50 },
                    new ExpenseSplitDto { UserId = 3, Amount = 33.50 }
                }
            };

            // Act
            var controller = CreateController();
            var result = await controller.AddExpenseAsync(expenseDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var expense = Assert.IsType<Expense>(okResult.Value);
            Assert.Equal(expenseDto.Amount, expense.Amount);
        }

        [Fact]
        public async Task GetDebtsForUser_WithValidIds_ShouldReturnDebts()
        {
            // Arrange
            int groupId = 1;
            int userId = 1;

            // Act
            var controller = CreateController();
            var result = await controller.GetDebtsForUser(groupId, userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var debts = Assert.IsType<List<DebtDto>>(okResult.Value);
            Assert.NotNull(debts);
        }

        [Fact]
        public async Task SnoozeMemberToPay_WithValidData_ShouldSucceed()
        {
            // Arrange
            var snoozeDto = new SnoozeToPayDto
            {
                snoozeToUsername = "user2",
                snoozeInfo = "Snoozed for 7 days"
            };

            // Act
            var controller = CreateController();
            var result = await controller.SnoozeMemberToPay(snoozeDto);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task GetGeiminiResponseForExpenses_WithValidGroupId_ShouldReturnAnalysis()
        {
            // Arrange
            int groupId = 1;

            // Act
            var controller = CreateController();
            var result = await controller.GetGeiminiResponseForExpenses(groupId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task DeleteGroup_AsAdmin_ShouldSucceed()
        {
            // Arrange
            int groupId = 1;
            int adminUserId = 1;

            // Act
            var controller = CreateController();
            var result = await controller.DeleteGroup(groupId, adminUserId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(okResult.Value);
            Assert.Equal("Group successfully deleted", response.Message);
        }

        [Fact]
        public async Task ExitGroup_WithValidIds_ShouldSucceed()
        {
            // Arrange
            int userId = 2;
            int groupId = 1;

            // Act
            var controller = CreateController();
            var result = await controller.ExitGroup(userId, groupId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(okResult.Value);
            Assert.Equal("Successfully left the group", response.Message);
        }

        private GroupsController CreateController()
        {
            var groupService = new Mock<IGroupService>();
            var groupInviteMediatorService = new Mock<IGroupInviteMediatorService>();
            var configuration = new Mock<IConfiguration>();
            var geminiService = new GeminiService(configuration.Object);

            // Set up test group
            var testGroup = new Group("Test Group", new User(), new List<User>());
            var testExpense = new Expense
            {
                Amount = 100.50,
                Description = "Groceries",
                Category = Category.Other
            };

            // Configure group service
            groupService.Setup(s => s.GetGroupAsync(It.IsAny<int>()))
                .ReturnsAsync(testGroup);
            groupService.Setup(s => s.GetUserGroupsAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<Group> { testGroup });
            groupService.Setup(s => s.AddExpenseAsync(It.IsAny<ExpenseDto>()))
                .ReturnsAsync(testExpense);
            groupService.Setup(s => s.GetDebtsForUserAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<DebtDto>());
            groupService.Setup(s => s.GetFeedbackFromGeminiAsync(It.IsAny<string>()))
                .ReturnsAsync("Analysis of expenses...");

            // Configure group invite mediator service
            groupInviteMediatorService.Setup(s => s.CreateNewGroupSendInvitesAsync(It.IsAny<CreateNewGroupDto>()))
                .ReturnsAsync(testGroup);

            return new GroupsController(groupService.Object, groupInviteMediatorService.Object, geminiService);
        }
    }
}
