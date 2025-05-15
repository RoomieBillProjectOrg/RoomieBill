using Microsoft.AspNetCore.Mvc;
using Moq;
using Roomiebill.Server.Controllers;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services;
using Xunit;

namespace ServerTests
{
    public class GroupsControllerTests
    {
        private readonly Mock<GroupService> _mockGroupService;
        private readonly Mock<GroupInviteMediatorService> _mockMediatorService;
        private readonly GroupsController _controller;

        public GroupsControllerTests()
        {
            _mockGroupService = new Mock<GroupService>();
            _mockMediatorService = new Mock<GroupInviteMediatorService>();
            _controller = new GroupsController(_mockGroupService.Object, _mockMediatorService.Object, null);
        }

        [Fact]
        public async Task TestThatWhenCreatingGroupThenReturnsNewGroup()
        {
            CreateNewGroupDto groupDto = new CreateNewGroupDto
            {
                GroupName = "Test Group",
                AdminGroupUsername = "admin"
            };
            Group newGroup = new Group { Id = 1, GroupName = groupDto.GroupName };

            _mockMediatorService.Setup(s => s.CreateNewGroupSendInvitesAsync(groupDto))
                              .ReturnsAsync(newGroup);

            IActionResult result = await _controller.CreateGroup(groupDto);
            
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(newGroup, okResult.Value);
        }

        [Fact]
        public async Task TestThatWhenGettingUserGroupsThenReturnsGroups()
        {
            int userId = 1;
            List<Group> groups = new List<Group>
            {
                new Group { Id = 1, GroupName = "Group 1" },
                new Group { Id = 2, GroupName = "Group 2" }
            };

            _mockGroupService.Setup(s => s.GetUserGroupsAsync(userId))
                          .ReturnsAsync(groups);

            IActionResult result = await _controller.GetUserGroups(userId);
            
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(groups, okResult.Value);
        }

        [Fact]
        public async Task TestThatWhenGettingGroupByIdThenReturnsGroup()
        {
            int groupId = 1;
            Group group = new Group { Id = groupId, GroupName = "Test Group" };

            _mockGroupService.Setup(s => s.GetGroupAsync(groupId))
                          .ReturnsAsync(group);

            IActionResult result = await _controller.GetGroup(groupId);
            
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(group, okResult.Value);
        }

        [Fact]
        public async Task TestThatWhenGettingDebtsForUserThenReturnsDebts()
        {
            int groupId = 1;
            int userId = 1;
            User creditor = new User { Id = userId };
            User debtor = new User { Id = 2 };
            List<DebtDto> debts = new List<DebtDto>
            {
                new DebtDto { creditor = creditor, debtor = debtor, amount = 100 },
                new DebtDto { creditor = creditor, debtor = new User { Id = 3 }, amount = 200 }
            };

            _mockGroupService.Setup(s => s.GetDebtsForUserAsync(groupId, userId))
                          .ReturnsAsync(debts);

            IActionResult result = await _controller.GetDebtsForUser(groupId, userId);
            
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(debts, okResult.Value);
        }

        [Fact]
        public async Task TestThatWhenGettingDebtsOwedByUserThenReturnsDebts()
        {
            int groupId = 1;
            int userId = 1;
            User debtor = new User { Id = userId };
            List<DebtDto> debts = new List<DebtDto>
            {
                new DebtDto { creditor = new User { Id = 2 }, debtor = debtor, amount = 100 },
                new DebtDto { creditor = new User { Id = 3 }, debtor = debtor, amount = 200 }
            };

            _mockGroupService.Setup(s => s.GetDebtsOwedByUserAsync(groupId, userId))
                          .ReturnsAsync(debts);

            IActionResult result = await _controller.GetDebtsOwedByUser(groupId, userId);
            
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(debts, okResult.Value);
        }

        [Fact]
        public async Task TestThatWhenAddingExpenseThenReturnsExpense()
        {
            ExpenseDto expenseDto = new ExpenseDto
            {
                GroupId = 1
            };
            Expense newExpense = new Expense { Id = 1 };

            _mockGroupService.Setup(s => s.AddExpenseAsync(expenseDto))
                          .ReturnsAsync(newExpense);

            IActionResult result = await _controller.AddExpenseAsync(expenseDto);
            
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(newExpense, okResult.Value);
        }

        [Fact]
        public async Task TestThatWhenGettingExpensesForGroupThenReturnsExpenses()
        {
            int groupId = 1;
            List<Expense> expenses = new List<Expense>
            {
                new Expense { Id = 1 },
                new Expense { Id = 2 }
            };

            _mockGroupService.Setup(s => s.GetExpensesForGroupAsync(groupId))
                          .ReturnsAsync(expenses);

            IActionResult result = await _controller.GetExpensesForGroup(groupId);
            
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expenses, okResult.Value);
        }

        [Fact]
        public async Task TestThatWhenSnoozingPaymentThenReturnsSuccess()
        {
            SnoozeToPayDto snoozeInfo = new SnoozeToPayDto
            {
                snoozeToUsername = "user1",
                snoozeInfo = "Reminder snoozed"
            };

            _mockGroupService.Setup(s => s.SnoozeMemberToPayAsync(snoozeInfo))
                          .Returns(Task.CompletedTask);

            IActionResult result = await _controller.SnoozeMemberToPay(snoozeInfo);
            
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task TestThatWhenDeletingGroupThenReturnsSuccess()
        {
            int groupId = 1;
            int userId = 1;

            _mockGroupService.Setup(s => s.DeleteGroupAsync(groupId, userId))
                          .Returns(Task.CompletedTask);

            IActionResult result = await _controller.DeleteGroup(groupId, userId);
            
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("successfully", (okResult.Value as dynamic).Message.ToString());
        }

        [Fact]
        public async Task TestThatWhenExitingGroupThenReturnsSuccess()
        {
            int groupId = 1;
            int userId = 1;

            _mockGroupService.Setup(s => s.ExitGroupAsync(userId, groupId))
                          .Returns(Task.CompletedTask);

            IActionResult result = await _controller.ExitGroup(userId, groupId);
            
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("successfully", (okResult.Value as dynamic).Message.ToString());
        }

        [Fact]
        public async Task TestThatWhenGettingGeminiAnalysisThenReturnsFeedback()
        {
            int groupId = 1;
            List<Expense> expenses = new List<Expense>
            {
                new Expense { Id = 1 },
                new Expense { Id = 2 }
            };
            string feedback = "Analysis feedback";

            _mockGroupService.Setup(s => s.GetExpensesForGroupAsync(groupId))
                          .ReturnsAsync(expenses);
            _mockGroupService.Setup(s => s.GetFeedbackFromGeminiAsync(It.IsAny<string>()))
                          .ReturnsAsync(feedback);

            IActionResult result = await _controller.GetGeiminiResponseForExpenses(groupId);
            
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(feedback, okResult.Value);
        }

        [Fact]
        public async Task TestThatWhenExceptionOccursThenReturnsBadRequest()
        {
            int groupId = 1;
            string errorMessage = "Error occurred";
            
            _mockGroupService.Setup(s => s.GetExpensesForGroupAsync(groupId))
                          .ThrowsAsync(new Exception(errorMessage));

            IActionResult result = await _controller.GetExpensesForGroup(groupId);
            
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, (badRequest.Value as dynamic).Message);
        }
    }
}
