using Roomiebill.Server.Facades;
using Roomiebill.Server.DataAccessLayer.Dtos;

namespace ServerTests

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

        [Fact]
        public void SettleDebt_ShouldSetDebtToZero()
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
            expenseFacade.SettleDebt(1, 0);

            // Assert
            Assert.Equal(0, expenseFacade.GetDebtBetween(1, 0)); // User 2 owes User 1: 0
        }

        [Fact]
        public void UpdateExpense_ShouldUpdateDebtArray()
        {
            // Arrange
            List<int> userIds = new List<int> { 1, 2, 3 };
            ExpenseFacade expenseFacade = new ExpenseFacade(userIds);

            ExpenseDto oldExpenseDto = new ExpenseDto
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

            ExpenseDto newExpenseDto = new ExpenseDto
            {
                Id = 1,
                Amount = 200,
                Description = "Updated Dinner",
                IsPaid = true,
                PayerId = 1,
                SplitBetween = new Dictionary<int, double>
                {
                    { 1, 50.0 },
                    { 2, 25.0 },
                    { 3, 25.0 }
                }
            };

            expenseFacade.CreateExpense(oldExpenseDto);

            // Act
            expenseFacade.UpdateExpense(oldExpenseDto, newExpenseDto);

            // Assert
            Assert.Equal(0, expenseFacade.GetDebtBetween(0, 1)); // User 1 owes User 2: 0
            Assert.Equal(0, expenseFacade.GetDebtBetween(0, 2)); // User 1 owes User 3: 0
            Assert.Equal(50, expenseFacade.GetDebtBetween(1, 0)); // User 2 owes User 1: 50
            Assert.Equal(50, expenseFacade.GetDebtBetween(2, 0)); // User 3 owes User 1: 50
        }

        [Fact]
        public void GetTotalDebtPerUser_ShouldReturnCorrectDebt()
        {
            // Arrange
            List<int> userIds = new List<int> { 1, 2, 3 };
            ExpenseFacade expenseFacade = new ExpenseFacade(userIds);

            ExpenseDto expenseDto1 = new ExpenseDto
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

            ExpenseDto expenseDto2 = new ExpenseDto
            {
                Id = 2,
                Amount = 200,
                Description = "Lunch",
                IsPaid = true,
                PayerId = 2,
                SplitBetween = new Dictionary<int, double>
                {
                    { 1, 50.0 },
                    { 2, 30.0 },
                    { 3, 20.0 }
                }
            };

            expenseFacade.CreateExpense(expenseDto1);
            expenseFacade.CreateExpense(expenseDto2);

            // Act
        
            double debt1To2 = expenseFacade.GetDebtBetween(0, 1);
            double debt1To3 = expenseFacade.GetDebtBetween(0, 2);
            double debt2To1 = expenseFacade.GetDebtBetween(1, 0);
            double debt2To3 = expenseFacade.GetDebtBetween(1, 2);
            double debt3To1 = expenseFacade.GetDebtBetween(2, 0);
            double debt3To2 = expenseFacade.GetDebtBetween(2, 1);

            // Assert
            Assert.Equal(70,debt1To2);// User 1 owes 70 to user 2
            Assert.Equal(0,debt1To3);// User 1 owes nothing to user 3
            Assert.Equal(0,debt2To1);// User 2 owes nothing to user 1
            Assert.Equal(0,debt2To3);// User 2 owes nothing to user 3
            Assert.Equal(20,debt3To1);// User 3 owes 20 to user 1
            Assert.Equal(40,debt3To2);// User 3 owes 70 to user 2

        }
        [Fact]

        public void GetTotalDebtForUser_ShouldReturnCorrectTotalDebt()
        {
            // Arrange
            List<int> userIds = new List<int> { 1, 2, 3 };
            ExpenseFacade expenseFacade = new ExpenseFacade(userIds);

            ExpenseDto expenseDto1 = new ExpenseDto
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

            ExpenseDto expenseDto2 = new ExpenseDto
            {
                Id = 2,
                Amount = 200,
                Description = "Lunch",
                IsPaid = true,
                PayerId = 2,
                SplitBetween = new Dictionary<int, double>
                {
                    { 1, 50.0 },
                    { 2, 30.0 },
                    { 3, 20.0 }
                }
            };

            expenseFacade.CreateExpense(expenseDto1);
            expenseFacade.CreateExpense(expenseDto2);

            // Act
            double totalDebtUser1 = expenseFacade.GetTotalDebtOwedToUser(1);
            double totalDebtUser2 = expenseFacade.GetTotalDebtOwedToUser(2);
            double totalDebtUser3 = expenseFacade.GetTotalDebtOwedToUser(3);
           
            // Assert
            Assert.Equal(20, totalDebtUser1); // User 1 owes nothing
            Assert.Equal(110, totalDebtUser2); // User 2 owes 30
            Assert.Equal(0, totalDebtUser3); // User 3 owes 20
        }

        [Fact]
        public void GetTotalDebtUserOwes_ShouldReturnCorrectTotalDebt()
        {
            // Arrange
            List<int> userIds = new List<int> { 1, 2, 3 };
            ExpenseFacade expenseFacade = new ExpenseFacade(userIds);

            ExpenseDto expenseDto1 = new ExpenseDto
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

            ExpenseDto expenseDto2 = new ExpenseDto
            {
                Id = 2,
                Amount = 200,
                Description = "Lunch",
                IsPaid = true,
                PayerId = 2,
                SplitBetween = new Dictionary<int, double>
                {
                    { 1, 50.0 },
                    { 2, 30.0 },
                    { 3, 20.0 }
                }
            };

            expenseFacade.CreateExpense(expenseDto1);
            expenseFacade.CreateExpense(expenseDto2);

            // Act
            double totalDebtUser1Owes = expenseFacade.GetTotalDebtUserOwes(1);
            double totalDebtUser2Owes = expenseFacade.GetTotalDebtUserOwes(2);
            double totalDebtUser3Owes = expenseFacade.GetTotalDebtUserOwes(3);

            // Assert
            Assert.Equal(70, totalDebtUser1Owes); // User 1 owes 70
            Assert.Equal(0, totalDebtUser2Owes); // User 2 owes nothing
            Assert.Equal(60, totalDebtUser3Owes); // User 3 owes 60
        }


        [Fact]
        public void GetAllDebtsFromDict_ShouldReturnCorrectDebts()
        {
            // Arrange
            List<int> userIds = new List<int> { 1, 2, 3 };
            ExpenseFacade expenseFacade = new ExpenseFacade(userIds);

            ExpenseDto expenseDto1 = new ExpenseDto
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

            ExpenseDto expenseDto2 = new ExpenseDto
            {
                Id = 2,
                Amount = 200,
                Description = "Lunch",
                IsPaid = true,
                PayerId = 2,
                SplitBetween = new Dictionary<int, double>
                {
                    { 1, 50.0 },
                    { 2, 30.0 },
                    { 3, 20.0 }
                }
            };

            expenseFacade.CreateExpense(expenseDto1);
            expenseFacade.CreateExpense(expenseDto2);

            // Act
            Dictionary<(int, int), int> debts = expenseFacade.GetAllDebts();

            // Assert
            Assert.Equal(6, debts.Count);
            Assert.Equal(70, debts[(0, 1)]); // User 1 owes User 2: 70
            Assert.Equal(0, debts[(0, 2)]); // User 1 owes User 3: 0
            Assert.Equal(0, debts[(1, 0)]); // User 2 owes User 1: 0
            Assert.Equal(0, debts[(1, 2)]); // User 2 owes User 3: 0
            Assert.Equal(20, debts[(2, 0)]); // User 3 owes User 1: 20
            Assert.Equal(40, debts[(2, 1)]); // User 3 owes User 2: 50
        }
        
    }
}