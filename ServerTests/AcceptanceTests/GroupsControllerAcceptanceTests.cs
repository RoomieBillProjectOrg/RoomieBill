using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Roomiebill.Server.Controllers;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services.Interfaces;
using Roomiebill.Server.Common;
using Roomiebill.Server.Common.Enums;
using System.Net;
using System.Text.Json;
using Xunit;

namespace ServerTests.AcceptanceTests
{
    public class GroupsControllerAcceptanceTests
    {
        private readonly Mock<IGroupService> _mockGroupService;
        private readonly Mock<IGroupInviteMediatorService> _mockMediatorService;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<HttpMessageHandler> _mockHttpHandler;
        private readonly GeminiService _geminiService;
        private readonly GroupsController _controller;

        public GroupsControllerAcceptanceTests()
        {
            _mockGroupService = new Mock<IGroupService>();
            _mockMediatorService = new Mock<IGroupInviteMediatorService>();
            _mockConfig = new Mock<IConfiguration>();
            _mockHttpHandler = new Mock<HttpMessageHandler>();

            // Create GeminiService with mocked dependencies
            _geminiService = new GeminiService(_mockConfig.Object);
            var client = new HttpClient(_mockHttpHandler.Object);
            var field = typeof(GeminiService).GetField("_httpClient",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(_geminiService, client);

            _controller = new GroupsController(_mockGroupService.Object, _mockMediatorService.Object, _geminiService);
        }

        [Fact]
        public async Task CreateGroup_WithValidData_ShouldSucceed()
        {
            // Arrange
            var newGroupDto = new CreateNewGroupDto 
            { 
                GroupName = "Test Group",
                AdminGroupUsername = "admin",
                GroupMembersEmailsList = new List<string> { "user1@test.com", "user2@test.com" }
            };
            var expectedGroup = new Group { GroupName = "Test Group" };

            _mockMediatorService
                .Setup(s => s.CreateNewGroupSendInvitesAsync(It.IsAny<CreateNewGroupDto>()))
                .ReturnsAsync(expectedGroup);

            // Act
            var result = await _controller.CreateGroup(newGroupDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var group = Assert.IsType<Group>(okResult.Value);
            Assert.Equal(expectedGroup.GroupName, group.GroupName);
        }

        [Fact]
        public async Task CreateGroup_WhenMediatorThrows_ShouldReturnBadRequest()
        {
            // Arrange
            var newGroupDto = new CreateNewGroupDto
            {
                GroupName = "Test Group",
                AdminGroupUsername = "admin",
                GroupMembersEmailsList = new List<string> { "user1@test.com" }
            };

            _mockMediatorService
                .Setup(s => s.CreateNewGroupSendInvitesAsync(It.IsAny<CreateNewGroupDto>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.CreateGroup(newGroupDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(badRequestResult.Value);
            Assert.Equal("Unexpected error", response.Message);
        }

        [Fact]
        public async Task GetUserGroups_WithValidUser_ShouldReturnGroups()
        {
            // Arrange
            int userId = 1;
            var expectedGroups = new List<Group> 
            {
                new Group { GroupName = "Group 1", Admin = new User { Id = userId } },
                new Group { GroupName = "Group 2", Admin = new User { Id = userId } }
            };

            _mockGroupService
                .Setup(s => s.GetUserGroupsAsync(userId))
                .ReturnsAsync(expectedGroups);

            // Act
            var result = await _controller.GetUserGroups(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var groups = Assert.IsType<List<Group>>(okResult.Value);
            Assert.Equal(2, groups.Count);
        }

        [Fact]
        public async Task GetUserGroups_WithNoGroups_ShouldReturnEmptyList()
        {
            // Arrange
            int userId = 2;
            _mockGroupService
                .Setup(s => s.GetUserGroupsAsync(userId))
                .ReturnsAsync(new List<Group>());

            // Act
            var result = await _controller.GetUserGroups(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var groups = Assert.IsType<List<Group>>(okResult.Value);
            Assert.Empty(groups);
        }

        [Fact]
        public async Task GetUserGroups_WhenServiceThrows_ShouldReturnBadRequest()
        {
            // Arrange
            int userId = 1;
            _mockGroupService
                .Setup(s => s.GetUserGroupsAsync(userId))
                .ThrowsAsync(new Exception("User not found"));

            // Act
            var result = await _controller.GetUserGroups(userId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(badRequestResult.Value);
            Assert.Equal("User not found", response.Message);
        }

        [Fact]
        public async Task GetDebtsForUser_WithValidInput_ShouldReturnDebts()
        {
            // Arrange
            int groupId = 1;
            int userId = 1;
            var expectedDebts = new List<DebtDto>
            {
                new DebtDto 
                { 
                    creditor = new User { Username = "user1" }, 
                    debtor = new User { Username = "user2" }, 
                    amount = 50 
                },
                new DebtDto 
                { 
                    creditor = new User { Username = "user1" }, 
                    debtor = new User { Username = "user3" }, 
                    amount = 30 
                }
            };

            _mockGroupService
                .Setup(s => s.GetDebtsForUserAsync(groupId, userId))
                .ReturnsAsync(expectedDebts);

            // Act
            var result = await _controller.GetDebtsForUser(groupId, userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var debts = Assert.IsType<List<DebtDto>>(okResult.Value);
            Assert.Equal(2, debts.Count);
            Assert.Equal(50, debts[0].amount);
        }

        [Fact]
        public async Task GetDebtsForUser_WithNoDebts_ShouldReturnEmptyList()
        {
            // Arrange
            int groupId = 1, userId = 2;
            _mockGroupService
                .Setup(s => s.GetDebtsForUserAsync(groupId, userId))
                .ReturnsAsync(new List<DebtDto>());

            // Act
            var result = await _controller.GetDebtsForUser(groupId, userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var debts = Assert.IsType<List<DebtDto>>(okResult.Value);
            Assert.Empty(debts);
        }

        [Fact]
        public async Task GetDebtsForUser_WhenServiceThrows_ShouldReturnBadRequest()
        {
            // Arrange
            int groupId = 1, userId = 1;
            _mockGroupService
                .Setup(s => s.GetDebtsForUserAsync(groupId, userId))
                .ThrowsAsync(new Exception("Group not found"));

            // Act
            var result = await _controller.GetDebtsForUser(groupId, userId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(badRequestResult.Value);
            Assert.Equal("Group not found", response.Message);
        }

        [Fact]
        public async Task AddExpense_WithValidData_ShouldSucceed()
        {
            // Arrange
            var expenseDto = new ExpenseDto
            {
                GroupId = 1,
                PayerId = 1,
                Amount = 150,
                Description = "Gas Bill",
                Category = Category.Gas,
                ExpenseSplits = new List<ExpenseSplitDto>
                {
                    new ExpenseSplitDto { UserId = 2, Amount = 75 },
                    new ExpenseSplitDto { UserId = 3, Amount = 75 }
                }
            };
            var expectedExpense = new Expense
            {
                Amount = 150,
                Description = "Gas Bill",
                Category = Category.Gas
            };

            _mockGroupService
                .Setup(s => s.AddExpenseAsync(It.IsAny<ExpenseDto>()))
                .ReturnsAsync(expectedExpense);

            // Act
            var result = await _controller.AddExpenseAsync(expenseDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var expense = Assert.IsType<Expense>(okResult.Value);
            Assert.Equal(expectedExpense.Amount, expense.Amount);
            Assert.Equal(expectedExpense.Description, expense.Description);
        }

        [Fact]
        public async Task AddExpense_WithZeroAmount_ShouldSucceed()
        {
            // Arrange
            var expenseDto = new ExpenseDto
            {
                GroupId = 1,
                PayerId = 1,
                Amount = 0,
                Description = "Zero",
                Category = Category.Other,
                ExpenseSplits = new List<ExpenseSplitDto>()
            };
            var expectedExpense = new Expense
            {
                Amount = 0,
                Description = "Zero",
                Category = Category.Other
            };

            _mockGroupService
                .Setup(s => s.AddExpenseAsync(It.IsAny<ExpenseDto>()))
                .ReturnsAsync(expectedExpense);

            // Act
            var result = await _controller.AddExpenseAsync(expenseDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var expense = Assert.IsType<Expense>(okResult.Value);
            Assert.Equal(0, expense.Amount);
        }

        [Fact]
        public async Task AddExpense_WithNullExpenseSplits_ShouldSucceed()
        {
            // Arrange
            var expenseDto = new ExpenseDto
            {
                GroupId = 1,
                PayerId = 1,
                Amount = 100,
                Description = "Test",
                Category = Category.Other,
                ExpenseSplits = null
            };
            var expectedExpense = new Expense
            {
                Amount = 100,
                Description = "Test",
                Category = Category.Other
            };

            _mockGroupService
                .Setup(s => s.AddExpenseAsync(It.IsAny<ExpenseDto>()))
                .ReturnsAsync(expectedExpense);

            // Act
            var result = await _controller.AddExpenseAsync(expenseDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var expense = Assert.IsType<Expense>(okResult.Value);
            Assert.Equal(expectedExpense.Amount, expense.Amount);
        }

        [Fact]
        public async Task AddExpense_WhenServiceThrows_ShouldReturnBadRequest()
        {
            // Arrange
            var expenseDto = new ExpenseDto { GroupId = 1, PayerId = 1, Amount = 100 };
            _mockGroupService
                .Setup(s => s.AddExpenseAsync(It.IsAny<ExpenseDto>()))
                .ThrowsAsync(new Exception("Expense error"));

            // Act
            var result = await _controller.AddExpenseAsync(expenseDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(badRequestResult.Value);
            Assert.Equal("Expense error", response.Message);
        }

        [Fact]
        public async Task GetGeiminiResponseForExpenses_ShouldReturnAnalysis()
        {
            // Arrange
            int groupId = 1;
            var expenses = new List<Expense>
            {
                new Expense { Amount = 100, Category = Category.Gas },
                new Expense { Amount = 200, Category = Category.Electricity }
            };

            var expectedResponse = "AI Analysis: Your gas and electricity expenses are typical for Israel";

            _mockGroupService
                .Setup(s => s.GetExpensesForGroupAsync(groupId))
                .ReturnsAsync(expenses);

            _mockGroupService
                .Setup(s => s.GetFeedbackFromGeminiAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetGeiminiResponseForExpenses(groupId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var analysis = Assert.IsType<string>(okResult.Value);
            Assert.Equal(expectedResponse, analysis);
        }

        [Fact]
        public async Task GetGeiminiResponseForExpenses_WhenExpensesServiceThrows_ShouldReturnBadRequest()
        {
            // Arrange
            int groupId = 1;
            _mockGroupService
                .Setup(s => s.GetExpensesForGroupAsync(groupId))
                .ThrowsAsync(new Exception("No expenses"));

            // Act
            var result = await _controller.GetGeiminiResponseForExpenses(groupId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(badRequestResult.Value);
            Assert.Equal("No expenses", response.Message);
        }

        [Fact]
        public async Task GetGeiminiResponseForExpenses_WhenGeminiThrows_ShouldReturnBadRequest()
        {
            // Arrange
            int groupId = 1;
            var expenses = new List<Expense> { new Expense { Amount = 100 } };
            _mockGroupService
                .Setup(s => s.GetExpensesForGroupAsync(groupId))
                .ReturnsAsync(expenses);
            _mockGroupService
                .Setup(s => s.GetFeedbackFromGeminiAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Gemini error"));

            // Act
            var result = await _controller.GetGeiminiResponseForExpenses(groupId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(badRequestResult.Value);
            Assert.Equal("Gemini error", response.Message);
        }

        [Fact]
        public async Task DeleteGroup_AsAdmin_ShouldSucceed()
        {
            // Arrange
            int groupId = 1;
            int userId = 1;

            _mockGroupService
                .Setup(s => s.DeleteGroupAsync(groupId, userId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteGroup(groupId, userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(okResult.Value);
            Assert.Equal("Group successfully deleted", response.Message);
        }

        [Fact]
        public async Task DeleteGroup_WhenServiceThrows_ShouldReturnBadRequest()
        {
            // Arrange
            int groupId = 1, userId = 1;
            _mockGroupService
                .Setup(s => s.DeleteGroupAsync(groupId, userId))
                .ThrowsAsync(new Exception("Delete failed"));

            // Act
            var result = await _controller.DeleteGroup(groupId, userId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(badRequestResult.Value);
            Assert.Equal("Delete failed", response.Message);
        }

        [Fact]
        public async Task ExitGroup_WithNoDebts_ShouldSucceed()
        {
            // Arrange
            int groupId = 1;
            int userId = 1;

            _mockGroupService
                .Setup(s => s.ExitGroupAsync(userId, groupId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ExitGroup(userId, groupId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(okResult.Value);
            Assert.Equal("Successfully left the group", response.Message);
        }

        [Fact]
        public async Task ExitGroup_WhenServiceThrowsGenericException_ShouldReturnBadRequest()
        {
            // Arrange
            int groupId = 1, userId = 1;
            _mockGroupService
                .Setup(s => s.ExitGroupAsync(userId, groupId))
                .ThrowsAsync(new Exception("Unexpected exit error"));

            // Act
            var result = await _controller.ExitGroup(userId, groupId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(badRequestResult.Value);
            Assert.Equal("Unexpected exit error", response.Message);
        }

        [Fact]
        public async Task ExitGroup_WithDebts_ShouldReturnError()
        {
            // Arrange
            int groupId = 1;
            int userId = 1;

            _mockGroupService
                .Setup(s => s.ExitGroupAsync(userId, groupId))
                .ThrowsAsync(new Exception("Cannot exit group with outstanding debts"));

            // Act
            var result = await _controller.ExitGroup(userId, groupId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(badRequestResult.Value);
            Assert.Equal("Cannot exit group with outstanding debts", response.Message);
        }

        [Fact]
        public async Task CreateGroup_WithEmptyMembers_ShouldSucceed()
        {
            // Arrange
            var newGroupDto = new CreateNewGroupDto
            {
                GroupName = "Empty Group",
                AdminGroupUsername = "admin",
                GroupMembersEmailsList = new List<string>()
            };
            var expectedGroup = new Group { GroupName = "Empty Group" };

            _mockMediatorService
                .Setup(s => s.CreateNewGroupSendInvitesAsync(It.IsAny<CreateNewGroupDto>()))
                .ReturnsAsync(expectedGroup);

            // Act
            var result = await _controller.CreateGroup(newGroupDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var group = Assert.IsType<Group>(okResult.Value);
            Assert.Equal(expectedGroup.GroupName, group.GroupName);
        }

        [Fact]
        public async Task CreateGroup_WithInvalidMembers_ShouldReturnError()
        {
            // Arrange
            var newGroupDto = new CreateNewGroupDto 
            { 
                GroupName = "Test Group",
                AdminGroupUsername = "admin",
                GroupMembersEmailsList = new List<string> { "nonexistent@test.com" }
            };

            _mockMediatorService
                .Setup(s => s.CreateNewGroupSendInvitesAsync(It.IsAny<CreateNewGroupDto>()))
                .ThrowsAsync(new Exception("One or more users not found"));

            // Act
            var result = await _controller.CreateGroup(newGroupDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(badRequestResult.Value);
            Assert.Equal("One or more users not found", response.Message);
        }
    }
}
