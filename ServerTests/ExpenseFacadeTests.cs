using Roomiebill.Server.Facades;
using Roomiebill.Server.DataAccessLayer.Dtos;


namespace Roomiebill.Tests
{
    public class ExpenseFacadeTests
    {
        [Fact]
        public void CreateExpense_ShouldUpdateDebtArray()
        {
            // Arrange
            List<int> userIds = new List<int> { 1, 2, 3 };
            ExpenseFacade expenseFacade = new ExpenseFacade(userIds);

            ExpenseDto expenseDto = new ExpenseDto
            {
                Id = 1,
                Amount = 100,
                Description = "Dinner",
                IsPaid = true,
                PayerId = 1,
                SplitBetween = new Dictionary<int, double>
                {
                    { 1, 50.0 },
                    { 2, 30.0 },
                    { 3, 20.0 }
                }
            };

            // Act
            expenseFacade.CreateExpense(expenseDto);

            // Assert
            Assert.Equal(0, expenseFacade.GetDebtBetween(0, 1)); // User 1 owes User 2: 0
            Assert.Equal(0, expenseFacade.GetDebtBetween(0, 2)); // User 1 owes User 3: 0
            Assert.Equal(30, expenseFacade.GetDebtBetween(1, 0)); // User 2 owes User 1: 30
            Assert.Equal(20, expenseFacade.GetDebtBetween(2, 0)); // User 3 owes User 1: 20
        }

        [Fact]
        public void DeleteExpense_ShouldReverseDebtArray()
        {
            // Arrange
            List<int> userIds = new List<int> { 1, 2, 3 };
            ExpenseFacade expenseFacade = new ExpenseFacade(userIds);

            ExpenseDto expenseDto = new ExpenseDto
            {
                Id = 1,
                Amount = 100,
                Description = "Dinner",
                IsPaid = true,
                PayerId = 1,
                SplitBetween = new Dictionary<int, double>
                {
                    { 1, 50.0 },
                    { 2, 30.0 },
                    { 3, 20.0 }
                }
            };

            expenseFacade.CreateExpense(expenseDto);

            // Act
            expenseFacade.DeleteExpense(expenseDto);

            // Assert
            Assert.Equal(0, expenseFacade.GetDebtBetween(0, 1)); // User 1 owes User 2: 0
            Assert.Equal(0, expenseFacade.GetDebtBetween(0, 2)); // User 1 owes User 3: 0
            Assert.Equal(0, expenseFacade.GetDebtBetween(1, 0)); // User 2 owes User 1: 0
            Assert.Equal(0, expenseFacade.GetDebtBetween(2, 0)); // User 3 owes User 1: 0
        }

        [Fact]
        public void GetAllDebts_ShouldReturnCorrectDebts()
        {
            // Arrange
            List<int> userIds = new List<int> { 1, 2, 3 };
            ExpenseFacade expenseFacade = new ExpenseFacade(userIds);

            ExpenseDto expenseDto = new ExpenseDto
            {
                Id = 1,
                Amount = 100,
                Description = "Dinner",
                IsPaid = true,
                PayerId = 1,
                SplitBetween = new Dictionary<int, double>
                {
                    { 1, 50.0 },
                    { 2, 30.0 },
                    { 3, 20.0 }
                }
            };

            expenseFacade.CreateExpense(expenseDto);

            // Act
            Dictionary<(int, int), int> debts = expenseFacade.GetAllDebts();

            // Assert
            Assert.Equal(2, debts.Count);
            Assert.Equal(30, debts[(1, 0)]); // User 2 owes User 1: 30
            Assert.Equal(20, debts[(2, 0)]); // User 3 owes User 1: 20
        }
    }
}