using Microsoft.Extensions.Logging;
using Moq;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Facades;
using Roomiebill.Server.Models;

namespace ServerTests

{
    public class GroupFacadeTests
    {
        private readonly Mock<IApplicationDbContext> _groupDbMock;
        private readonly Mock<ILogger<GroupFacade>> _loggerMock;
        private readonly Mock<IUserFacade> _userFacadeMock;
        private GroupFacade _groupFacade;

        public GroupFacadeTests()
        {
            _groupDbMock = new Mock<IApplicationDbContext>();
            _loggerMock = new Mock<ILogger<GroupFacade>>();
            _userFacadeMock = new Mock<IUserFacade>();
        }

        #region CreateNewGroupAsync

        [Fact]
        public async Task TestCreateNewGroupAsync_WhenSuccessfulCreate_ThenReturnsNewGroup()
        {
            // Arrange
            CreateNewGroupDto newGroupDto = new CreateNewGroupDto
            {
                AdminGroupUsername = "admin",
                GroupMembersPhoneNumbersList = new List<string> { "member1", "member2" }
            };

            User admin = new User("admin", "admin@bgu.ac.il", "adminPassword!1");
            User member1 = new User("member1", "user1@bgu.ac.il", "user1Password!1");
            User member2 = new User("member2", "user2@bgu.ac.il", "user2Password!1");

            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync("admin")).ReturnsAsync(admin);
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync("member1")).ReturnsAsync(member1);
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync("member2")).ReturnsAsync(member2);

            _groupFacade = new GroupFacade(_groupDbMock.Object, _loggerMock.Object, _userFacadeMock.Object);

            // Act
            Group? result = await _groupFacade.CreateNewGroupAsync(newGroupDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newGroupDto.GroupName, result.GetGroupName());
            Assert.Equal(newGroupDto.AdminGroupUsername, result.GetAdmin().Username);
            Assert.Equal(newGroupDto.GroupMembersPhoneNumbersList.Count, result.GetMembers().Count);
            Assert.Contains(result.GetMembers(), x => x.Username == "member1");
            Assert.Contains(result.GetMembers(), x => x.Username == "member2");
        }

        [Fact]
        public async Task TestCreateNewGroupAsync_WhenAdminDoesNotExist_ThenThrowsException()
        {
            // Arrange
            CreateNewGroupDto newGroupDto = new CreateNewGroupDto
            {
                AdminGroupUsername = "admin",
                GroupMembersPhoneNumbersList = new List<string> { "member1", "member2" }
            };
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync("admin"))!.ReturnsAsync((User?)null);
            _groupFacade = new GroupFacade(_groupDbMock.Object, _loggerMock.Object, _userFacadeMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _groupFacade.CreateNewGroupAsync(newGroupDto));
        }

        [Fact]
        public async Task TestCreateNewGroupAsync_WhenMemberDoesNotExist_ThenThrowsException()
        {
            // Arrange
            CreateNewGroupDto newGroupDto = new CreateNewGroupDto
            {
                AdminGroupUsername = "admin",
                GroupMembersPhoneNumbersList = new List<string> { "member1", "member2" }
            };

            User admin = new User("admin", "admin@bgu.ac.il", "adminPassword!1");

            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync("admin"))!.ReturnsAsync(admin);
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync("member1"))!.ReturnsAsync((User?)null);

            _groupFacade = new GroupFacade(_groupDbMock.Object, _loggerMock.Object, _userFacadeMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _groupFacade.CreateNewGroupAsync(newGroupDto));
        }

        [Fact]
        public async Task TestCreateNewGroupAsync_WhenMemberListIsEmpty_ThenReturnsNewGroupWithOnlyAdmin()
        {
            // Arrange
            CreateNewGroupDto newGroupDto = new CreateNewGroupDto
            {
                AdminGroupUsername = "admin",
                GroupMembersPhoneNumbersList = new List<string>()
            };

            User admin = new User("admin", "admin@bgu.ac.il", "adminPassword!1");

            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync("admin"))!.ReturnsAsync(admin);

            _groupFacade = new GroupFacade(_groupDbMock.Object, _loggerMock.Object, _userFacadeMock.Object);

            // Act
            Group? result = await _groupFacade.CreateNewGroupAsync(newGroupDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newGroupDto.GroupName, result.GetGroupName());
            Assert.Equal(newGroupDto.AdminGroupUsername, result.GetAdmin().Username);
            Assert.Empty(result.GetMembers());
        }

         [Fact]
        public async Task TestAddExpense_WhenAllGood_ShouldAddExpense()
        {
            // Arrange
            var groupId = 1;
            var payerId = 1;
            var expenseDto = new ExpenseDto
            {
                Amount = 100.0,
                Description = "Dinner",
                IsPaid = false,
                PayerId = payerId,
                GroupId = groupId,
                ExpenseSplits = new List<ExpenseSplitDto>
                {
                    new ExpenseSplitDto { UserId = 0, Percentage = 20.0 },
                    new ExpenseSplitDto { UserId = 2, Percentage = 30.0 }
                }
            };
            User payer = new User("payer","payer@bgu.ac.il","payerPassword!1");
            User user1 = new User("user1", "user1@bgu.ac.il", "user1Password!1");
            User user2 = new User("user2", "user2@bgu.ac.il", "user2Password!1");
            payer.Id = 1;
            user1.Id = 0;
            user2.Id = 2;
            var group = new Group("Test Group", payer,new List<User> { payer, user1, user2 });

            _groupDbMock.Setup(x => x.GetGroupById(groupId)).Returns(group);
            _userFacadeMock.Setup(x => x.GetUserByIdAsync(payerId)).ReturnsAsync(payer);
            // _groupDbMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            _groupFacade = new GroupFacade(_groupDbMock.Object, _loggerMock.Object, _userFacadeMock.Object);

            // Act
            await _groupFacade.AddExpenseAsync(expenseDto);

            // Assert
            Assert.Single(group.Expenses);
            var addedExpense = group.Expenses.First();
            int [] debt = group.getDebtArray();
            int debt01 = group.getDebtBetweenUsers(0,1);//20
            int debt02 = group.getDebtBetweenUsers(0,2);//0
            int debt12 = group.getDebtBetweenUsers(1,2);//0
            int debt10 = group.getDebtBetweenUsers(1,0); //0
            int debt20 = group.getDebtBetweenUsers(2,0);//0
            int debt21 = group.getDebtBetweenUsers(2,1);//30
            Assert.Equal(expenseDto.Amount, addedExpense.Amount);
            Assert.Equal(expenseDto.Description, addedExpense.Description);
            Assert.Equal(expenseDto.IsPaid, addedExpense.IsPaid);
            Assert.Equal(expenseDto.PayerId, addedExpense.PayerId);
            Assert.Equal(expenseDto.GroupId, addedExpense.GroupId);
            Assert.Equal(expenseDto.ExpenseSplits.Count, addedExpense.ExpenseSplits.Count);
        }



        #endregion
    }
}   