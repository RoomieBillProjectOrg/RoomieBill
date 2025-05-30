using Roomiebill.Server.Common.Enums;
using Roomiebill.Server.Models;
using Xunit;

namespace ServerTests.UnitTests
{
    public class GroupTests
    {
        [Fact]
        public void CanUserExitGroup_NoDebts_ShouldReturnTrue()
        {
            // Arrange
            var admin = new User { Id = 1, Username = "admin" };
            var user = new User { Id = 2, Username = "user" };
            var group = new Group("TestGroup", admin, new List<User> { admin, user });

            // Act
            bool canExit = group.CanUserExitGroup(user.Id);

            // Assert
            Assert.True(canExit);
        }

        [Fact]
        public void CanUserExitGroup_HasDebtsOwed_ShouldReturnFalse()
        {
            // Arrange
            var admin = new User { Id = 1, Username = "admin" };
            var user = new User { Id = 2, Username = "user" };
            var group = new Group("TestGroup", admin, new List<User> { admin, user });

            // Add an expense where user owes money
            var expense = new Expense
            {
                Amount = 100,
                PayerId = admin.Id,
                Payer = admin,
                GroupId = 1,
                Group = group,
                ExpenseSplits = new List<ExpenseSplit>
                {
                    new ExpenseSplit { UserId = user.Id, User = user, Amount = 50 },
                    new ExpenseSplit { UserId = admin.Id, User = admin, Amount = 50 }
                },
                Category = Category.Other,
                Description = "Test expense"
            };
            group.AddExpense(expense);

            // Act
            bool canExit = group.CanUserExitGroup(user.Id);

            // Assert
            Assert.False(canExit);
        }

        [Fact]
        public void CanUserExitGroup_IsOwedMoney_ShouldReturnFalse()
        {
            // Arrange
            var admin = new User { Id = 1, Username = "admin" };
            var user = new User { Id = 2, Username = "user" };
            var group = new Group("TestGroup", admin, new List<User> { admin, user });

            // Add an expense where user is owed money
            var expense = new Expense
            {
                Amount = 100,
                PayerId = user.Id,
                Payer = user,
                GroupId = 1,
                Group = group,
                ExpenseSplits = new List<ExpenseSplit>
                {
                    new ExpenseSplit { UserId = admin.Id, User = admin, Amount = 50 },
                    new ExpenseSplit { UserId = user.Id, User = user, Amount = 50 }
                },
                Category = Category.Other,
                Description = "Test expense"
            };
            group.AddExpense(expense);

            // Act
            bool canExit = group.CanUserExitGroup(user.Id);

            // Assert
            Assert.False(canExit);
        }

        [Fact]
        public void CanUserExitGroup_SettledDebts_ShouldReturnTrue()
        {
            // Arrange
            var admin = new User { Id = 1, Username = "admin" };
            var user = new User { Id = 2, Username = "user" };
            var group = new Group("TestGroup", admin, new List<User> { admin, user });

            // Add an expense
            var expense = new Expense
            {
                Amount = 100,
                PayerId = admin.Id,
                Payer = admin,
                GroupId = 1,
                Group = group,
                ExpenseSplits = new List<ExpenseSplit>
                {
                    new ExpenseSplit { UserId = user.Id, User = user, Amount = 50 },
                    new ExpenseSplit { UserId = admin.Id, User = admin, Amount = 50 }
                },
                Category = Category.Other,
                Description = "Test expense"
            };
            group.AddExpense(expense);

            // Settle the debt
            group.SettleDebt(50, admin.Id, user.Id);

            // Act
            bool canExit = group.CanUserExitGroup(user.Id);

            // Assert
            Assert.True(canExit);
        }
    }
}
