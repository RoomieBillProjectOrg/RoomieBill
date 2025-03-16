using System;
using System.Collections.Generic;
using Xunit;
using Roomiebill.Server.Facades;
using Roomiebill.Server.Models;

namespace ServerTests
{
    public class ExpenseHandlerTests
    {
        [Fact]
        public void Test_AddExpense()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1 },
                new User { Id = 2 },
                new User { Id = 3 }
            };
            var expenseHandler = new ExpenseHandler(users);
            var debtArray = new int[3];
            var expense = new Expense
            {
                PayerId = 1,
                Amount = 100,
                ExpenseSplits = new List<ExpenseSplit>
                {
                    new ExpenseSplit { UserId = 2, Percentage = 50 },
                    new ExpenseSplit { UserId = 3, Percentage = 50 }
                }
            };

            // Act
            expenseHandler.AddExpense(expense, debtArray);

            // Assert
            Assert.Equal(-50, debtArray[expenseHandler.GetIndex(0, 1)]);
            Assert.Equal(-50, debtArray[expenseHandler.GetIndex(0, 2)]);
        }

        [Fact]
        public void Test_GetDebtBetween()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1 },
                new User { Id = 2 }
            };
            var expenseHandler = new ExpenseHandler(users);
            var debtArray = new int[1];
            debtArray[0] = -50;

            // Act
            var debt = expenseHandler.GetDebtBetween(1, 0, debtArray);

            // Assert
            Assert.Equal(50, debt);
        }

        [Fact]
        public void Test_SettleDebt()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 0 },
                new User { Id = 1 }
            };
            var expenseHandler = new ExpenseHandler(users);
            var debtArray = new int[1];
            debtArray[0] = -50;

            // Act
            expenseHandler.SettleDebt(1, 0, debtArray);

            // Assert
            Assert.Equal(0, debtArray[0]);
        }

        [Fact]
        public void Test_UpdateExpense()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1 },
                new User { Id = 2 },
                new User { Id = 3 }
            };
            var expenseHandler = new ExpenseHandler(users);
            var debtArray = new int[3];
            var oldExpense = new Expense
            {
                PayerId = 1,
                Amount = 100,
                ExpenseSplits = new List<ExpenseSplit>
                {
                    new ExpenseSplit { UserId = 2, Percentage = 50 },
                    new ExpenseSplit { UserId = 3, Percentage = 50 }
                }
            };
            var newExpense = new Expense
            {
                PayerId = 1,
                Amount = 200,
                ExpenseSplits = new List<ExpenseSplit>
                {
                    new ExpenseSplit { UserId = 2, Percentage = 50 },
                    new ExpenseSplit { UserId = 3, Percentage = 50 }
                }
            };

            // Act
            expenseHandler.UpdateExpense(oldExpense, newExpense, debtArray);

            // Assert
            Assert.Equal(-50, debtArray[expenseHandler.GetIndex(0, 1)]);
            Assert.Equal(-50, debtArray[expenseHandler.GetIndex(0, 2)]);
        }

        [Fact]
        public void Test_GetAllDebts()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1 },
                new User { Id = 2 },
                new User { Id = 3 }
            };
            var expenseHandler = new ExpenseHandler(users);
            var debtArray = new int[3];
            debtArray[expenseHandler.GetIndex(0, 1)] = -50;
            debtArray[expenseHandler.GetIndex(0, 2)] = -30;

            // Act
            var debts = expenseHandler.GetAllDebts(debtArray);

            // Assert
            Assert.Equal(50, debts[(1, 0)]);
            Assert.Equal(30, debts[(2, 0)]);
        }

        [Fact]
        public void Test_SettleAllDebts()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 0 },
                new User { Id = 1 },
                new User { Id = 2 }
            };
            var expenseHandler = new ExpenseHandler(users);
            var debtArray = new int[3];
            debtArray[expenseHandler.GetIndex(0, 1)] = -50;
            debtArray[expenseHandler.GetIndex(0, 2)] = -30;

            // Act
            expenseHandler.SettleAllDebts(0, debtArray);

            // Assert
            Assert.Equal(0, debtArray[expenseHandler.GetIndex(0, 1)]);
            Assert.Equal(0, debtArray[expenseHandler.GetIndex(0, 2)]);
        }

        [Fact]
        public void Test_DeleteExpense()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1 },
                new User { Id = 2 },
                new User { Id = 3 }
            };
            var expenseHandler = new ExpenseHandler(users);
            var debtArray = new int[3];
            var expense = new Expense
            {
                PayerId = 1,
                Amount = 100,
                ExpenseSplits = new List<ExpenseSplit>
                {
                    new ExpenseSplit { UserId = 2, Percentage = 50 },
                    new ExpenseSplit { UserId = 3, Percentage = 50 }
                }
            };
            expenseHandler.AddExpense(expense, debtArray);

            // Act
            expenseHandler.DeleteExpense(expense, debtArray);

            // Assert
            Assert.Equal(0, debtArray[expenseHandler.GetIndex(1, 0)]);
            Assert.Equal(0, debtArray[expenseHandler.GetIndex(2, 0)]);
        }

        [Fact]
        public void Test_GetTotalDebtOwedToUser()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 0 },
                new User { Id = 1 },
                new User { Id = 2 }
            };
            var expenseHandler = new ExpenseHandler(users);
            var debtArray = new int[3];
            debtArray[expenseHandler.GetIndex(1, 0)] = -50;
            debtArray[expenseHandler.GetIndex(2, 0)] = -30;

            // Act
            var totalDebt = expenseHandler.GetTotalDebtOwedToUser(0, debtArray);

            // Assert
            Assert.Equal(80, totalDebt);
        }

        [Fact]
        public void Test_GetTotalDebtUserOwes()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 0 },
                new User { Id = 1 },
                new User { Id = 2 }
            };
            var expenseHandler = new ExpenseHandler(users);
            var debtArray = new int[3];
            debtArray[expenseHandler.GetIndex(0, 1)] = -50;
            debtArray[expenseHandler.GetIndex(0, 2)] = -30;

            // Act
            var totalDebt = expenseHandler.GetTotalDebtUserOwes(0, debtArray);

            // Assert
            Assert.Equal(0, totalDebt);
        }
    }
}
