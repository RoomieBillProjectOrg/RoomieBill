using Roomiebill.Server.Facades;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;
using Roomiebill.Server.Exceptions;
using System.Collections.Generic;
using Xunit;

namespace Roomiebill.Tests
{
    public class ExpenseHandlerTests
    {
        [Fact]
        public void AddExpense_ShouldUpdateDebtArray()
        {
            // Arrange
            List<User> users = new List<User>
            {
                new User { Id = 1, Username = "User 1" },
                new User { Id = 2, Username = "User 2" },
                new User { Id = 3, Username = "User 3" }
            };
            ExpenseHandler expenseHandler = new ExpenseHandler(users);
            int[] debtArray = new int[3];

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
            expenseHandler.AddExpense(expenseDto, debtArray);

            // Assert
            Assert.Equal(0, expenseHandler.GetDebtBetween(0, 1, debtArray)); // User 1 owes User 2: 0
            Assert.Equal(0, expenseHandler.GetDebtBetween(0, 2, debtArray)); // User 1 owes User 3: 0
            Assert.Equal(30, expenseHandler.GetDebtBetween(1, 0, debtArray)); // User 2 owes User 1: 30
            Assert.Equal(20, expenseHandler.GetDebtBetween(2, 0, debtArray)); // User 3 owes User 1: 20
        }

        [Fact]
        public void GetAllDebts_ShouldReturnCorrectDebts()
        {
            // Arrange
            List<User> users = new List<User>
            {
                new User { Id = 1, Username = "User 1" },
                new User { Id = 2, Username = "User 2" },
                new User { Id = 3, Username = "User 3" }
            };
            ExpenseHandler expenseHandler = new ExpenseHandler(users);
            int[] debtArray = new int[3];

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

            expenseHandler.AddExpense(expenseDto, debtArray);

            // Act
            Dictionary<(int, int), int> debts = expenseHandler.GetAllDebts(debtArray);

            // Assert
            Assert.Equal(6, debts.Count);
            Assert.Equal(30, debts[(1, 0)]); // User 2 owes User 1: 30
            Assert.Equal(20, debts[(2, 0)]); // User 3 owes User 1: 20
        }

        [Fact]
        public void ReduceDebtArraySize_ShouldThrowExceptionForUnsettledDebts()
        {
            // Arrange
            List<User> users = new List<User>
            {
                new User { Id = 1, Username = "User 1" },
                new User { Id = 2, Username = "User 2" },
                new User { Id = 3, Username = "User 3" }
            };
            ExpenseHandler expenseHandler = new ExpenseHandler(users);
            int[] debtArray = new int[3];

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

            expenseHandler.AddExpense(expenseDto, debtArray);

            // Act & Assert
            List<int> removedUsers = new List<int> { 2 };
            var exception = Assert.Throws<UnsettledDebtException>(() => expenseHandler.ReduceDebtArraySize(2, 3, removedUsers, debtArray));
            String message = exception.Message;
            // Debugging: Print the message to the console
            Console.WriteLine($"Exception Message: '{message}'");

            Assert.Equal("User 2 owns 30 to user 1.", message);
        }
        [Fact]
        public void ReduceDebtArraySize_ShouldSuccess()
        {
            // Arrange
            List<User> users = new List<User>
            {
                new User { Id = 1, Username = "User 1" },
                new User { Id = 2, Username = "User 2" },
                new User { Id = 3, Username = "User 3" },
                new User { Id = 4, Username = "User 4" }
            };
            ExpenseHandler expenseHandler = new ExpenseHandler(users);
            int userCount = users.Count;
            int size = (userCount * (userCount - 1)) / 2; // size of the debtArray

            int[] debtArray = new int[size];

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
                    { 3, 20.0 },
                    { 4, 0}
                }
            };

            expenseHandler.AddExpense(expenseDto, debtArray);

            // Act & Assert
            List<int> removedUsers = new List<int> { 4 };
            int[] newDebtArray = expenseHandler.ReduceDebtArraySize(3, 4, removedUsers, debtArray);
            userCount -= 1;
            size = (userCount * (userCount - 1)) / 2; // size of the debtArray
            // Debugging: Print the message to the console
            Assert.Equal(size,newDebtArray.Length);
        }
                    
    }
}