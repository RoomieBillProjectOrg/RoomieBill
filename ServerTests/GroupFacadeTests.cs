﻿﻿﻿﻿﻿﻿﻿﻿﻿using Microsoft.Extensions.Logging;
using Moq;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Facades;
using Roomiebill.Server.Models;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Roomiebill.Server.Common.Enums;

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
            _groupFacade = new GroupFacade(_groupDbMock.Object, _loggerMock.Object, _userFacadeMock.Object);
        }

        #region CreateNewGroupAsync

        [Fact]
        public async Task TestCreateNewGroupAsync_WhenDuplicateGroupName_ThenThrowsException()
        {
            // Arrange
            var admin = new User("admin", "admin@bgu.ac.il", "adminPassword!1","https://bit-demo-link.com") { Id = 1 };
            var existingGroups = new List<Group>
            {
                new Group("Test Group", admin, new List<User>())
            };

            CreateNewGroupDto newGroupDto = new CreateNewGroupDto
            {
                GroupName = "Test Group", // Same name as existing group
                AdminGroupUsername = "admin",
                GroupMembersEmailsList = new List<string>()
            };

            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync("admin")).ReturnsAsync(admin);
            _groupDbMock.Setup(x => x.GetUserGroupsAsync(admin.Id)).ReturnsAsync(existingGroups);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _groupFacade.CreateNewGroupAsync(newGroupDto));
            Assert.Equal($"You already have a group named '{newGroupDto.GroupName}'. Please choose a different name.", exception.Message);
        }

        [Fact]
        public async Task TestCreateNewGroupAsync_WhenSuccessfulCreate_ThenReturnsNewGroup()
        {
            // Arrange
            CreateNewGroupDto newGroupDto = new CreateNewGroupDto
            {
                GroupName = "Test Group",
                AdminGroupUsername = "admin",
                GroupMembersEmailsList = new List<string> { "member1", "member2" }
            };

            User admin = new User("admin", "admin@bgu.ac.il", "adminPassword!1","https://bit-demo-link.com") { Id = 1 };
            User member1 = new User("member1", "user1@bgu.ac.il", "user1Password!1","https://bit-demo-link.com");
            User member2 = new User("member2", "user2@bgu.ac.il", "user2Password!1","https://bit-demo-link.com");

            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync("admin")).ReturnsAsync(admin);
            _groupDbMock.Setup(x => x.GetUserGroupsAsync(admin.Id)).ReturnsAsync(new List<Group>());
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync("member1")).ReturnsAsync(member1);
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync("member2")).ReturnsAsync(member2);

            // Act
            Group? result = await _groupFacade.CreateNewGroupAsync(newGroupDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newGroupDto.GroupName, result.GetGroupName());
            Assert.Equal(newGroupDto.AdminGroupUsername, result.GetAdmin().Username);
            Assert.Contains(admin, result.GetMembers());
        }

        [Fact]
        public async Task TestCreateNewGroupAsync_WhenAdminDoesNotExist_ThenThrowsException()
        {
            // Arrange
            CreateNewGroupDto newGroupDto = new CreateNewGroupDto
            {
                GroupName = "Test Group",
                AdminGroupUsername = "admin",
                GroupMembersEmailsList = new List<string> { "member1", "member2" }
            };
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync("admin"))!.ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _groupFacade.CreateNewGroupAsync(newGroupDto));
        }

        [Fact]
        public async Task TestCreateNewGroupAsync_WhenMemberListIsEmpty_ThenReturnsNewGroupWithOnlyAdmin()
        {
            // Arrange
            CreateNewGroupDto newGroupDto = new CreateNewGroupDto
            {
                GroupName = "Test Group",
                AdminGroupUsername = "admin",
                GroupMembersEmailsList = new List<string>()
            };

            User admin = new User("admin", "admin@bgu.ac.il", "adminPassword!1","https://bit-demo-link.com") { Id = 1 };

            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync("admin"))!.ReturnsAsync(admin);
            _groupDbMock.Setup(x => x.GetUserGroupsAsync(admin.Id)).ReturnsAsync(new List<Group>());

            // Act
            Group? result = await _groupFacade.CreateNewGroupAsync(newGroupDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newGroupDto.GroupName, result.GetGroupName());
            Assert.Equal(newGroupDto.AdminGroupUsername, result.GetAdmin().Username);
            Assert.Contains(admin, result.GetMembers());
            Assert.Single(result.GetMembers());
        }

        #endregion

        #region AddExpenseAsync

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
                    new ExpenseSplitDto { UserId = 0, Amount = 20.0 },
                    new ExpenseSplitDto { UserId = 1, Amount = 50.0 },
                    new ExpenseSplitDto { UserId = 2, Amount = 30.0 }
                },
                Category = Category.Other
            };
            User payer = new User("payer", "payer@bgu.ac.il", "payerPassword!1","https://bit-demo-link.com");
            User user1 = new User("user1", "user1@bgu.ac.il", "user1Password!1","https://bit-demo-link.com");
            User user2 = new User("user2", "user2@bgu.ac.il", "user2Password!1","https://bit-demo-link.com");
            payer.Id = 1;
            user1.Id = 0;
            user2.Id = 2;
            var group = new Group("Test Group", payer, new List<User> { user1, user2 });

            _groupDbMock.Setup(x => x.GetGroupByIdAsync(groupId)).ReturnsAsync(group);
            _userFacadeMock.Setup(x => x.GetUserByIdAsync(payerId)).ReturnsAsync(payer);

            _groupFacade = new GroupFacade(_groupDbMock.Object, _loggerMock.Object, _userFacadeMock.Object);

            // Act
            await _groupFacade.AddExpenseAsync(expenseDto);

            // Assert
            Assert.Single(group.Expenses);
            var addedExpense = group.Expenses.First();
            double[] debt = group.getDebtArray();
            double debt01 = group.getDebtBetweenUsers(0, 1);//20
            double debt02 = group.getDebtBetweenUsers(0, 2);//0
            double debt12 = group.getDebtBetweenUsers(1, 2);//0
            double debt10 = group.getDebtBetweenUsers(1, 0); //0
            double debt20 = group.getDebtBetweenUsers(2, 0);//0
            double debt21 = group.getDebtBetweenUsers(2, 1);//30

            Assert.Equal(50, debt01 + debt21);
            Assert.Equal(0, debt10 + debt20 + debt02);
            Assert.Equal(20, debt01);
            Assert.Equal(30, debt21);
            Assert.Equal(expenseDto.Amount, addedExpense.Amount);
            Assert.Equal(expenseDto.Description, addedExpense.Description);
            Assert.Equal(expenseDto.IsPaid, addedExpense.IsPaid);
            Assert.Equal(expenseDto.PayerId, addedExpense.PayerId);
            Assert.Equal(expenseDto.GroupId, addedExpense.GroupId);
            Assert.Equal(expenseDto.ExpenseSplits.Count, addedExpense.ExpenseSplits.Count);
        }

        #endregion

        #region UpdateExpenseAsync

        [Fact]
        public async Task UpdateExpenseAsync_WhenExpenseExists_ShouldUpdateExpense()
        {
            // Arrange
            var groupId = 1;
            var payerId = 1;
            var expenseId = 0;
            var originalExpenseDto = new ExpenseDto
            {
                Id = expenseId,
                Amount = 100.0,
                Description = "Original Dinner",
                IsPaid = false,
                PayerId = payerId,
                GroupId = groupId,
                ExpenseSplits = new List<ExpenseSplitDto>
                {
                    new ExpenseSplitDto { UserId = 2, Amount = 50.0 },
                    new ExpenseSplitDto { UserId = 3, Amount = 50.0 }
                },
                Category = Category.Other
            };

            var updatedExpenseDto = new ExpenseDto
            {
                Id = expenseId,
                Amount = 150.0,
                Description = "Updated Dinner",
                IsPaid = true,
                PayerId = payerId,
                GroupId = groupId,
                ExpenseSplits = new List<ExpenseSplitDto>
                {
                    new ExpenseSplitDto { UserId = 2, Amount = 90.0 },
                    new ExpenseSplitDto { UserId = 3, Amount = 60.0 }
                }
            };

            var payer = new User("payer", "payer@bgu.ac.il", "hashedPassword","https://bit-demo-link.com") { Id = payerId };
            var user2 = new User("user2", "user2@bgu.ac.il", "hashedPassword","https://bit-demo-link.com") { Id = 2 };
            var user3 = new User("user3", "user3@bgu.ac.il", "hashedPassword","https://bit-demo-link.com") { Id = 3 };
            var group = new Group("Test Group", payer, new List<User> { user2, user3 })
            {
                Expenses = new List<Expense>
                {
                    new Expense
                    {
                        Id = expenseId,
                        Amount = originalExpenseDto.Amount,
                        Description = originalExpenseDto.Description,
                        IsPaid = originalExpenseDto.IsPaid,
                        PayerId = originalExpenseDto.PayerId,
                        GroupId = originalExpenseDto.GroupId,
                        ExpenseSplits = originalExpenseDto.ExpenseSplits.Select(splitDto => new ExpenseSplit
                        {
                            UserId = splitDto.UserId,
                            Amount = splitDto.Amount
                        }).ToList()
                    }
                }
            };

            _groupDbMock.Setup(x => x.GetGroupByIdAsync(groupId)).ReturnsAsync(group);
            _userFacadeMock.Setup(x => x.GetUserByIdAsync(payerId)).ReturnsAsync(payer);
            _groupDbMock.Setup(x => x.UpdateGroupAsync(group)).Returns(Task.CompletedTask);

            // Act
            await _groupFacade.UpdateExpenseAsync(originalExpenseDto, updatedExpenseDto);

            // Assert
            var updatedExpense = group.Expenses.First(e => e.Id == expenseId);
            Assert.Equal(updatedExpenseDto.Amount, updatedExpense.Amount);
            Assert.Equal(updatedExpenseDto.Description, updatedExpense.Description);
            Assert.Equal(updatedExpenseDto.IsPaid, updatedExpense.IsPaid);
            Assert.Equal(updatedExpenseDto.PayerId, updatedExpense.PayerId);
            Assert.Equal(updatedExpenseDto.GroupId, updatedExpense.GroupId);
            Assert.Equal(updatedExpenseDto.ExpenseSplits.Count, updatedExpense.ExpenseSplits.Count);
            _groupDbMock.Verify(x => x.UpdateGroupAsync(group), Times.Once);
        }

        [Fact]
        public async Task UpdateExpenseAsync_WhenExpenseDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var groupId = 1;
            var payerId = 1;
            var expenseId = 101;
            var originalExpenseDto = new ExpenseDto { Id = expenseId, GroupId = groupId };
            var updatedExpenseDto = new ExpenseDto { Id = expenseId, GroupId = groupId };
            var payer = new User("payer", "payer@bgu.ac.il", "hashedPassword","https://bit-demo-link.com") { Id = payerId };
            var group = new Group("Test Group", payer, new List<User> { })
            {
                Expenses = new List<Expense> { }
            };

            _groupDbMock.Setup(x => x.GetGroupByIdAsync(groupId)).ReturnsAsync(group);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _groupFacade.UpdateExpenseAsync(originalExpenseDto, updatedExpenseDto));
            Assert.Equal($"Expense with id {expenseId} does not exist in the group.", exception.Message);
        }

        #endregion

        #region AddMemberToGroupAsync

        [Fact]
        public async Task AddMemberToGroupAsync_WhenGroupDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var groupId = 1;
            var userToAdd = new User("newMember", "newMember@bgu.ac.il", "hashedPassword","https://bit-demo-link.com") { Id = 2 };

            _groupDbMock.Setup(x => x.GetGroupByIdAsync(groupId)).ReturnsAsync((Group?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _groupFacade.AddMemberToGroupAsync(userToAdd, new Group()
            {
                Id = groupId
            }));
            Assert.Equal($"Group with id {groupId} does not exist.", exception.Message);
        }

        #endregion

        #region RemoveMemberFromGroupAsync

        [Fact]
        public async Task TestRemoveMemberFromGroup_WhenGroupHasExpenses_ShouldRemoveMemberWithoutAffectingExpenses()
        {
            // Arrange
            var groupId = 1;
            var admin = new User("admin", "admin@bgu.ac.il", "adminPassword!1","https://bit-demo-link.com") { Id = 1 };
            var user2 = new User("user2", "user2@bgu.ac.il", "user2Password!1","https://bit-demo-link.com") { Id = 2 };
            var user3 = new User("user3", "user3@bgu.ac.il", "user3Password!1","https://bit-demo-link.com") { Id = 3 };
            var newMember = new User("newMember", "newMember@bgu.ac.il", "newMemberPassword!1","https://bit-demo-link.com") { Id = 4 };

            var expensedto = new ExpenseDto
            {
                Id = 101,
                Amount = 100.0,
                Description = "Dinner",
                IsPaid = false,
                PayerId = admin.Id,
                GroupId = groupId,
                ExpenseSplits = new List<ExpenseSplitDto>
                {
                    new ExpenseSplitDto { UserId = 2, Amount = 20.0 },
                    new ExpenseSplitDto { UserId = 3, Amount = 80.0 },
                },
                Category = Category.Other
            };

            var group = new Group("Test Group", admin, new List<User> { user2, user3, newMember })
            {
                Id = groupId,
                Expenses = new List<Expense> { }
            };

            _groupDbMock.Setup(x => x.GetGroupByIdAsync(groupId)).ReturnsAsync(group);
            _userFacadeMock.Setup(x => x.GetUserByIdAsync(admin.Id)).ReturnsAsync(admin);
            _userFacadeMock.Setup(x => x.GetUserByIdAsync(newMember.Id)).ReturnsAsync(newMember);
            _groupFacade = new GroupFacade(_groupDbMock.Object, _loggerMock.Object, _userFacadeMock.Object);

            // Act
            await _groupFacade.AddExpenseAsync(expensedto);
            await _groupFacade.RemoveMemberAsync(newMember, groupId);

            // Assert
            var existingExpense = group.Expenses.First();
            Assert.DoesNotContain(group.GetMembers(), u => u.Id == newMember.Id); // New member is removed
            Assert.Equal(3, group.GetMembers().Count); // Total members should now be 3
            Assert.Contains(existingExpense.ExpenseSplits, es => es.UserId == 2 && Math.Abs(es.Amount - 20.0) < 0.01);
            Assert.Contains(existingExpense.ExpenseSplits, es => es.UserId == 3 && Math.Abs(es.Amount - 80.0) < 0.01);
            Assert.DoesNotContain(existingExpense.ExpenseSplits, es => es.UserId == 4);
        }

        #endregion

        # region ExitGroupAsync
        
        [Fact]
        public async Task ExitGroup_UserHasNoDebts_ShouldSucceed()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GroupFacade>>();
            var mockUserFacade = new Mock<IUserFacade>();
            var mockDbContext = new Mock<IApplicationDbContext>();

            var user = new User { Id = 1, Username = "testUser" };
            var admin = new User { Id = 2, Username = "admin" };
            var group = new Group("TestGroup", admin, new List<User> { admin, user });

            mockDbContext.Setup(db => db.GetGroupByIdAsync(1))
                .ReturnsAsync(group);
            mockUserFacade.Setup(uf => uf.GetUserByIdAsync(1))
                .ReturnsAsync(user);

            var groupFacade = new GroupFacade(mockDbContext.Object, mockLogger.Object, mockUserFacade.Object);

            // Act
            await groupFacade.ExitGroupAsync(1, 1);

            // Assert
            mockDbContext.Verify(db => db.UpdateGroupAsync(It.IsAny<Group>()), Times.Once);
            Assert.DoesNotContain(user, group.Members);
        }

        [Fact]
        public async Task ExitGroup_UserHasDebts_ShouldThrowException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GroupFacade>>();
            var mockUserFacade = new Mock<IUserFacade>();
            var mockDbContext = new Mock<IApplicationDbContext>();

            var user = new User { Id = 1, Username = "testUser" };
            var admin = new User { Id = 2, Username = "admin" };
            var group = new Group("TestGroup", admin, new List<User> { admin, user });

            // Add a debt to the user
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

            mockDbContext.Setup(db => db.GetGroupByIdAsync(1))
                .ReturnsAsync(group);
            mockUserFacade.Setup(uf => uf.GetUserByIdAsync(1))
                .ReturnsAsync(user);

            var groupFacade = new GroupFacade(mockDbContext.Object, mockLogger.Object, mockUserFacade.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => groupFacade.ExitGroupAsync(1, 1));
        }

        [Fact]
        public async Task ExitGroup_AdminAttempt_ShouldThrowException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GroupFacade>>();
            var mockUserFacade = new Mock<IUserFacade>();
            var mockDbContext = new Mock<IApplicationDbContext>();

            var admin = new User { Id = 1, Username = "admin" };
            var group = new Group("TestGroup", admin, new List<User> { admin });

            mockDbContext.Setup(db => db.GetGroupByIdAsync(1))
                .ReturnsAsync(group);
            mockUserFacade.Setup(uf => uf.GetUserByIdAsync(1))
                .ReturnsAsync(admin);

            var groupFacade = new GroupFacade(mockDbContext.Object, mockLogger.Object, mockUserFacade.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => groupFacade.ExitGroupAsync(1, 1));
            Assert.Equal("Admin cannot exit the group. Transfer admin role first or delete the group.", exception.Message);
        }

        # endregion

        #region Helper Methods

        private Expense MapToEntity(ExpenseDto dto)
        {
            return new Expense
            {
                Id = dto.Id,
                Amount = dto.Amount,
                Description = dto.Description,
                IsPaid = dto.IsPaid,
                PayerId = dto.PayerId,
                GroupId = dto.GroupId,
                ExpenseSplits = dto.ExpenseSplits.Select(splitDto => new ExpenseSplit
                {
                    UserId = splitDto.UserId,
                    Amount = splitDto.Amount
                }).ToList()
            };
        }
        #endregion
    }
}
