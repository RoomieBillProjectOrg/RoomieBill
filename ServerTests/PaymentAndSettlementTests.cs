using Moq;
using Xunit;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services;
using Roomiebill.Server.Facades;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Roomiebill.Server.DataAccessLayer;

namespace ServerTests
{
    public class PaymentAndSettlementTests
    {
        private readonly Mock<IApplicationDbContext> _dbContextMock;
        private readonly Mock<ILogger<GroupFacade>> _loggerMock;
        private readonly Mock<IUserFacade> _userFacadeMock;
        private readonly GroupFacade _groupFacade;
        private readonly MockPaymentService _paymentService;

        public PaymentAndSettlementTests()
        {
            _dbContextMock = new Mock<IApplicationDbContext>();
            _loggerMock = new Mock<ILogger<GroupFacade>>();
            _userFacadeMock = new Mock<IUserFacade>();
            _groupFacade = new GroupFacade(_dbContextMock.Object, _loggerMock.Object, _userFacadeMock.Object);
            _paymentService = new MockPaymentService();
        }

        #region Payment Tests

        [Fact]
        public async Task TestProcessPaymentAsync_WhenCalled_ReturnsTrue()
        {
            // Arrange
            var paymentRequest = new PaymentRequest
            {
                Amount = 100,
                PayeeInfo = new User { Id = 1, Username = "Payee" },
                PayerInfo = new User { Id = 2, Username = "Payer" },
                GroupId = 1
            };

            // Act
            var result = await _paymentService.ProcessPaymentAsync(paymentRequest);

            // Assert
            Assert.True(result, "Payment should be processed successfully.");
        }

        #endregion

        #region Settlement Tests

        [Fact]
        public async Task TestSettleDebtAsync_WhenGroupAndUsersExist_SettlesDebtSuccessfully()
        {
            // Arrange
            var creditor = new User { Id = 1, Username = "Creditor" };
            var debtor = new User { Id = 2, Username = "Debtor" };
            var group = new Group("Test Group", creditor, new List<User> { debtor });

            var expense = new Expense
            {
                Id = 1,
                Amount = 50,
                Description = "Test",
                IsPaid = false,
                PayerId = creditor.Id,
                GroupId = group.Id,
                ExpenseSplits = new List<ExpenseSplit>
                {
                    new ExpenseSplit { UserId = debtor.Id, Amount = 50 }
                }
            };

            group.AddExpense(expense);

            _dbContextMock.Setup(db => db.GetGroupByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(group);

            // Act
            await _groupFacade.SettleDebtAsync(50, creditor, debtor, group.Id);

            // Assert
            Assert.True(Math.Abs(group.expenseHandler.GetDebtBetween(0, 1, group.getDebtArray())) < 0.01);
            _dbContextMock.Verify(db => db.UpdateGroupAsync(group), Times.Once);
        }

        [Fact]
        public async Task TestSettleDebtAsync_WhenGroupDoesNotExist_ThrowsException()
        {
            // Arrange
            var creditor = new User { Id = 1, Username = "Creditor" };
            var debtor = new User { Id = 2, Username = "Debtor" };

            _dbContextMock.Setup(db => db.GetGroupByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Group?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _groupFacade.SettleDebtAsync(50, creditor, debtor, 1));
            Assert.Equal("Error when trying to get group: group with id 1 does not exist in the system.", exception.Message);
        }

        #endregion
    }
}
