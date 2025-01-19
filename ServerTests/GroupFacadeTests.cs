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
            _groupFacade = new GroupFacade(_groupDbMock.Object, _loggerMock.Object, _userFacadeMock.Object);
        }

        #region CreateNewGroupAsync

        [Fact]
        public async Task TestCreateNewGroupAsync_WhenSuccessfulCreate_ThenReturnsNewGroup()
        {
            // Arrange
            CreateNewGroupDto newGroupDto = new CreateNewGroupDto
            {
                GroupName = "Test Group",
                AdminGroupUsername = "admin",
                GroupMembersUsernamesList = new List<string> { "member1", "member2" }
            };

            User admin = new User("admin", "admin@bgu.ac.il", "adminPassword!1");
            User member1 = new User("member1", "user1@bgu.ac.il", "user1Password!1");
            User member2 = new User("member2", "user2@bgu.ac.il", "user2Password!1");

            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync("admin")).ReturnsAsync(admin);
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync("member1")).ReturnsAsync(member1);
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync("member2")).ReturnsAsync(member2);

            // Act
            Group? result = await _groupFacade.CreateNewGroupAsync(newGroupDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newGroupDto.GroupName, result.GetGroupName());
            Assert.Equal(newGroupDto.AdminGroupUsername, result.GetAdmin().Username);
            Assert.Equal(newGroupDto.GroupMembersUsernamesList.Count, result.GetMembers().Count);
            Assert.Contains(result.GetMembers(), x => x.Username == "member1");
            Assert.Contains(result.GetMembers(), x => x.Username == "member2");
        }

        [Fact]
        public async Task TestCreateNewGroupAsync_WhenAdminDoesNotExist_ThenThrowsException()
        {
            // Arrange
            CreateNewGroupDto newGroupDto = new CreateNewGroupDto
            {
                GroupName = "Test Group",
                AdminGroupUsername = "admin",
                GroupMembersUsernamesList = new List<string> { "member1", "member2" }
            };
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync("admin"))!.ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _groupFacade.CreateNewGroupAsync(newGroupDto));
        }

        [Fact]
        public async Task TestCreateNewGroupAsync_WhenMemberDoesNotExist_ThenThrowsException()
        {
            // Arrange
            CreateNewGroupDto newGroupDto = new CreateNewGroupDto
            {
                GroupName = "Test Group",
                AdminGroupUsername = "admin",
                GroupMembersUsernamesList = new List<string> { "member1", "member2" }
            };

            User admin = new User("admin", "admin@bgu.ac.il", "adminPassword!1");

            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync("admin"))!.ReturnsAsync(admin);
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync("member1"))!.ReturnsAsync((User?)null);

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
                GroupMembersUsernamesList = new List<string>()
            };

            User admin = new User("admin", "admin@bgu.ac.il", "adminPassword!1");

            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync("admin"))!.ReturnsAsync(admin);

            // Act
            Group? result = await _groupFacade.CreateNewGroupAsync(newGroupDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newGroupDto.GroupName, result.GetGroupName());
            Assert.Equal(newGroupDto.AdminGroupUsername, result.GetAdmin().Username);
            Assert.Empty(result.GetMembers());
        }

        #endregion

        #region InviteToGroupByUsername

        [Fact]
        public async Task TestInviteToGroupByUsername_WhenInviterDoesNotExist_ThenThrowsException()
        {
            // Arrange
            string inviterUsername = "inviter";
            string invitedUsername = "invited";
            int groupId = 1;

            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync(inviterUsername))!.ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _groupFacade.InviteToGroupByUsername(inviterUsername, invitedUsername, groupId));
        }

        [Fact]
        public async Task TestInviteToGroupByUsername_WhenInvitedDoesNotExist_ThenThrowsException()
        {
            // Arrange
            string inviterUsername = "inviter";
            string invitedUsername = "invited";
            int groupId = 1;

            User inviter = new User("inviter", "Metar@bgu.ac.il",  "MetarPassword2@");
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync(inviterUsername))!.ReturnsAsync(inviter);
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync(invitedUsername))!.ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _groupFacade.InviteToGroupByUsername(inviterUsername, invitedUsername, groupId));
        }

        [Fact]
        public async Task TestInviteToGroupByUsername_WhenDoesntExist_ThenThrowsException()
        {
            // Arrange
            string inviterUsername = "inviter";
            string invitedUsername = "invited";
            int groupId = 1;

            User inviter = new User("inviter", "Metar@bgu.ac.il",  "MetarPassword2@");
            User invited = new User("invited", "Metar2@bgu.ac.il",  "MetarPassword2@");
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync(inviterUsername))!.ReturnsAsync(inviter);
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync(invitedUsername))!.ReturnsAsync(invited);

            _groupFacade = new GroupFacade(_groupDbMock.Object, _loggerMock.Object, _userFacadeMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _groupFacade.InviteToGroupByUsername(inviterUsername, invitedUsername, groupId));
        }

        [Fact]
        public async Task TestInviteToGroupByUsername_WhenInviteForInvitedExistInGroup_ThenThrowsException()
        {
            // Arrange
            string inviterUsername = "inviter";
            string invitedUsername = "invited";
            
            User inviter = new User(inviterUsername, "Metar@bgu.ac.il",  "MetarPassword2@");
            User invited = new User(invitedUsername, "Metar2@bgu.ac.il",  "MetarPassword2@");
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync(inviterUsername))!.ReturnsAsync(inviter);
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync(invitedUsername))!.ReturnsAsync(invited);
            Group group = await _groupFacade.CreateNewGroupAsync(new CreateNewGroupDto {
                GroupName = "Test Group",
                AdminGroupUsername = inviterUsername, 
                GroupMembersUsernamesList = new List<string> {} 
            });
            int groupId = group.Id;

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _groupFacade.InviteToGroupByUsername(inviterUsername, invitedUsername, groupId));
        }

        [Fact]
        public async Task TestInviteToGroupByUsername_WhenInviterNotInGroup_ThenThrowsException()
        {
            // Arrange
            string inviterUsername = "inviter";
            string invitedUsername = "invited";
            
            User inviter = new User("inviter", "Metar@bgu.ac.il",  "MetarPassword2@");
            User invited = new User("invited", "Metar2@bgu.ac.il",  "MetarPassword2@");
            User tal = new User("Tal", "Tal@bgu.ac.il",  "MetarPassword2@");
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync(inviterUsername))!.ReturnsAsync(inviter);
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync(invitedUsername))!.ReturnsAsync(invited);
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync("Tal"))!.ReturnsAsync(tal);
            Group group = await _groupFacade.CreateNewGroupAsync(new CreateNewGroupDto
            {
                GroupName = "Test Group",
                AdminGroupUsername = "Tal", 
                GroupMembersUsernamesList = new List<string> {} 
            });
            int groupId = group.Id;

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _groupFacade.InviteToGroupByUsername(inviterUsername, invitedUsername, groupId));
        }

        [Fact]
        public async Task TestInviteToGroupByUsername_WhenInvitedAlreadyInGroup_ThenThrowsException()
        {
            // Arrange
            string inviterUsername = "inviter";
            string invitedUsername = "invited";
            
            User inviter = new User(inviterUsername, "Metar@bgu.ac.il",  "MetarPassword2@");
            User invited = new User(invitedUsername, "Metar2@bgu.ac.il",  "MetarPassword2@");
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync(inviterUsername))!.ReturnsAsync(inviter);
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync(invitedUsername))!.ReturnsAsync(invited);
            Group group = await _groupFacade.CreateNewGroupAsync(new CreateNewGroupDto
            {
                GroupName = "Test Group",
                AdminGroupUsername = inviterUsername, 
                GroupMembersUsernamesList = new List<string> { invitedUsername } 
            });
            int groupId = group.Id;

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _groupFacade.InviteToGroupByUsername(inviterUsername, invitedUsername, groupId));
        }

        [Fact]
        public async Task TestInviteToGroupByUsername_WhenAllExistAndInviterInGroupAndInvitedIsnt_ThenSendInvite()
        {
            // Arrange
            string inviterUsername = "inviter";
            string invitedUsername = "invited";
            
            User inviter = new User(inviterUsername, "Metar@bgu.ac.il",  "MetarPassword2@");
            User invited = new User(invitedUsername, "Metar2@bgu.ac.il",  "MetarPassword2@");

            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync(inviterUsername))!.ReturnsAsync(inviter);
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync(invitedUsername))!.ReturnsAsync(invited);

            Group group = await _groupFacade.CreateNewGroupAsync(new CreateNewGroupDto
            {
                GroupName = "Test Group",
                AdminGroupUsername = inviterUsername, 
                GroupMembersUsernamesList = new List<string> { invitedUsername } 
            });

            _groupDbMock.Setup(x => x.GetGroupByIdAsync(group.Id, It.IsAny<Func<IQueryable<Group>, IQueryable<Group>>>()))!.ReturnsAsync(group);
            int groupId = group.Id;

            // Act 
            await _groupFacade.InviteToGroupByUsername(inviterUsername, invitedUsername, groupId);

            // Assert
            Assert.NotEmpty(group.Invites);
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
                    new ExpenseSplitDto { UserId = 0, Percentage = 20.0 },
                    new ExpenseSplitDto { UserId = 1, Percentage = 50.0 },
                    new ExpenseSplitDto { UserId = 2, Percentage = 30.0}

                }
            };
            User payer = new User("payer", "payer@bgu.ac.il", "payerPassword!1");
            User user1 = new User("user1", "user1@bgu.ac.il", "user1Password!1");
            User user2 = new User("user2", "user2@bgu.ac.il", "user2Password!1");
            payer.Id = 1;
            user1.Id = 0;
            user2.Id = 2;
            var group = new Group("Test Group", payer, new List<User> { payer, user1, user2 });

            _groupDbMock.Setup(x => x.GetGroupById(groupId)).Returns(group);
            _userFacadeMock.Setup(x => x.GetUserByIdAsync(payerId)).ReturnsAsync(payer);
            // _groupDbMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            _groupFacade = new GroupFacade(_groupDbMock.Object, _loggerMock.Object, _userFacadeMock.Object);

            // Act
            await _groupFacade.AddExpenseAsync(expenseDto);

            // Assert
            Assert.Single(group.Expenses);
            var addedExpense = group.Expenses.First();
            int[] debt = group.getDebtArray();
            int debt01 = group.getDebtBetweenUsers(0, 1);//20
            int debt02 = group.getDebtBetweenUsers(0, 2);//0
            int debt12 = group.getDebtBetweenUsers(1, 2);//0
            int debt10 = group.getDebtBetweenUsers(1, 0); //0
            int debt20 = group.getDebtBetweenUsers(2, 0);//0
            int debt21 = group.getDebtBetweenUsers(2, 1);//30
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
        public async Task TestUpdateExpense_WhenValidData_ShouldUpdateExpense()
        {
            // Arrange
            var groupId = 1;
            var payerId = 1;
            var expenseId = 101;

            // Original expense DTO
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
            new ExpenseSplitDto { UserId = 1, Percentage = 25.0 },
            new ExpenseSplitDto { UserId = 2, Percentage = 25.0 },
            new ExpenseSplitDto { UserId = 3, Percentage = 25.0 },
            new ExpenseSplitDto { UserId = 4, Percentage = 25.0 }
        }
            };

            // Updated expense DTO
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
            new ExpenseSplitDto { UserId = 1, Percentage = 60.0 },
            new ExpenseSplitDto { UserId = 2, Percentage = 40.0 }
        }
            };
            User payer = new User("payer", "payer@bgu.ac.il", "payerPassword!1");
            payer.Id = 1;

            // Group with all members
            var group = new Group("Test Group", payer, new List<User>
    {
        payer,
        new User { Id = 2, Username = "user2" },
        new User { Id = 3, Username = "user3" },
        new User { Id = 4, Username = "user4" }
    })
            {
                Expenses = new List<Expense> { }
            };

            // Mocking database and facade dependencies
            _groupDbMock.Setup(x => x.GetGroupById(groupId)).Returns(group);
            _userFacadeMock.Setup(x => x.GetUserByIdAsync(payerId)).ReturnsAsync(payer);
            _groupFacade = new GroupFacade(_groupDbMock.Object, _loggerMock.Object, _userFacadeMock.Object);

            // Act
            await _groupFacade.AddExpenseAsync(originalExpenseDto);
            int debt1 = group.getDebtBetweenUsers(1, 2);//0
            int debt11 = group.getDebtBetweenUsers(1, 3);//0
            int debt111 = group.getDebtBetweenUsers(1, 4);//0
            int debt2 = group.getDebtBetweenUsers(2, 1);//25
            int debt3 = group.getDebtBetweenUsers(3, 1);//25
            int debt4 = group.getDebtBetweenUsers(4, 1);//25

            await _groupFacade.UpdateExpenseAsync(originalExpenseDto, updatedExpenseDto);

            // Assert
            var updatedExpense = group.Expenses.First(e => e.Id == expenseId);
            Assert.Equal(updatedExpenseDto.Amount, updatedExpense.Amount);
            Assert.Equal(updatedExpenseDto.Description, updatedExpense.Description);
            Assert.Equal(updatedExpenseDto.IsPaid, updatedExpense.IsPaid);
            Assert.Equal(updatedExpenseDto.PayerId, updatedExpense.PayerId);
            Assert.Equal(updatedExpenseDto.GroupId, updatedExpense.GroupId);

            // Verify ExpenseSplits are updated
            Assert.Equal(updatedExpenseDto.ExpenseSplits.Count, updatedExpense.ExpenseSplits.Count);
            for (int i = 0; i < updatedExpenseDto.ExpenseSplits.Count; i++)
            {
                Assert.Equal(updatedExpenseDto.ExpenseSplits[i].UserId, updatedExpense.ExpenseSplits.ElementAt(i).UserId);
                Assert.Equal(updatedExpenseDto.ExpenseSplits[i].Percentage, updatedExpense.ExpenseSplits.ElementAt(i).Percentage);
            }

            // Ensure the group members are unchanged
            Assert.Equal(4, group.GetMembers().Count); // Original group members remain the same
            Assert.Contains(group.GetMembers(), u => u.Id == 3); // User 3 still part of the group
            Assert.Contains(group.GetMembers(), u => u.Id == 4); // User 4 still part of the group
            int debt12 = group.getDebtBetweenUsers(1, 2);//0
            int debt13 = group.getDebtBetweenUsers(1, 3);//0
            int debt14 = group.getDebtBetweenUsers(1, 4);//0
            int debt21 = group.getDebtBetweenUsers(2, 1);//40
            int debt23 = group.getDebtBetweenUsers(2, 3);//0
            int debt24 = group.getDebtBetweenUsers(2, 4);//0
            int debt31 = group.getDebtBetweenUsers(3, 1);//0
            int debt41 = group.getDebtBetweenUsers(4, 1);//0
            Assert.Equal(0, debt12);
            Assert.Equal(0, debt13);
            Assert.Equal(0, debt14);
            Assert.Equal(60, debt21);
            Assert.Equal(0, debt23);
            Assert.Equal(0, debt24);
            Assert.Equal(0, debt31);
            Assert.Equal(0, debt41);
        }

        #endregion

        #region AddMemberToGroupAsync

        [Fact]
        public async Task TestAddMemberToGroup_WhenGroupHasExpenses_ShouldAddMemberWithoutAffectingExpenses()
        {
            // Arrange
            var groupId = 1;
            var admin = new User("admin", "admin@bgu.ac.il", "adminPassword!1") { Id = 1 };
            var user1 = new User("user1", "user1@bgu.ac.il", "user1Password!1") { Id = 2 };
            var user2 = new User("user2", "user2@bgu.ac.il", "user2Password!1") { Id = 3 };
            var newMember = new User("newMember", "newMember@bgu.ac.il", "newMemberPassword!1") { Id = 4 };

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
            new ExpenseSplitDto { UserId = 1, Percentage = 60.0 },
            new ExpenseSplitDto { UserId = 2, Percentage = 20.0 },
            new ExpenseSplitDto { UserId = 3, Percentage = 20.0 },
        }
            };

            var group = new Group("Test Group", admin, new List<User> { admin, user1, user2 })
            {
                Id = groupId,
                Expenses = new List<Expense> { }
            };

             _groupDbMock.Setup(x => x.GetGroupById(groupId)).Returns(group);
            _userFacadeMock.Setup(x => x.GetUserByIdAsync(admin.Id)).ReturnsAsync(admin);
            _userFacadeMock.Setup(x => x.GetUserByIdAsync(newMember.Id)).ReturnsAsync(newMember);
            _groupFacade = new GroupFacade(_groupDbMock.Object, _loggerMock.Object, _userFacadeMock.Object);

          
            // _groupDbMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);


            // Act
            await _groupFacade.AddExpenseAsync(expensedto);

            await _groupFacade.AddMemberToGroupAsync(newMember, group);

            // Assert
            Assert.Contains(group.GetMembers(), u => u.Id == newMember.Id); // New member is added
            Assert.Equal(4, group.GetMembers().Count); // Total members should now be 4

            // Ensure the expense remains unaffected
            Assert.Single(group.Expenses);
            var existingExpense = group.Expenses.First();
            // Assert.Equal(expense.Amount, existingExpense.Amount);
            // Assert.Equal(expense.Description, existingExpense.Description);
            // Assert.Equal(expense.PayerId, existingExpense.PayerId);
            // Assert.Equal(expense.ExpenseSplits.Count, existingExpense.ExpenseSplits.Count);
            Assert.Contains(existingExpense.ExpenseSplits, es => es.UserId == admin.Id && es.Percentage == 60.0);
            Assert.Contains(existingExpense.ExpenseSplits, es => es.UserId == user1.Id && es.Percentage == 20.0);
            Assert.Contains(existingExpense.ExpenseSplits, es => es.UserId == user2.Id && es.Percentage == 20.0);
        }

        #endregion

        #region RemoveMemberFromGroupAsync

        [Fact]
        public async Task TestRemoveMemberFromGroup_WhenGroupHasExpenses_ShouldAemoveMemberWithoutAffectingExpenses()
        {
            // Arrange
            var groupId = 1;
            var admin = new User("admin", "admin@bgu.ac.il", "adminPassword!1") { Id = 1 };
            var user1 = new User("user1", "user1@bgu.ac.il", "user1Password!1") { Id = 2 };
            var user2 = new User("user2", "user2@bgu.ac.il", "user2Password!1") { Id = 3 };
            var newMember = new User("newMember", "newMember@bgu.ac.il", "newMemberPassword!1") { Id = 4 };

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
            new ExpenseSplitDto { UserId = 1, Percentage = 60.0 },
            new ExpenseSplitDto { UserId = 2, Percentage = 20.0 },
            new ExpenseSplitDto { UserId = 3, Percentage = 20.0 },
            new ExpenseSplitDto { UserId = 4, Percentage = 0.0 }
        }
            };

            var group = new Group("Test Group", admin, new List<User> { admin, user1, user2, newMember })
            {
                Id = groupId,
                Expenses = new List<Expense> { }
            };

             _groupDbMock.Setup(x => x.GetGroupById(groupId)).Returns(group);
            _userFacadeMock.Setup(x => x.GetUserByIdAsync(admin.Id)).ReturnsAsync(admin);
            _userFacadeMock.Setup(x => x.GetUserByIdAsync(newMember.Id)).ReturnsAsync(newMember);
            _groupFacade = new GroupFacade(_groupDbMock.Object, _loggerMock.Object, _userFacadeMock.Object);

          
            // _groupDbMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);


            // Act
            await _groupFacade.AddExpenseAsync(expensedto);
            await _groupFacade.RemoveMemberAsync(newMember, groupId);

            // Assert
            var existingExpense = group.Expenses.First();
            Assert.DoesNotContain(group.GetMembers(), u => u.Id == newMember.Id); // New member is removed
            Assert.Equal(3, group.GetMembers().Count); // Total members should now be 3
            Assert.Contains(existingExpense.ExpenseSplits, es => es.UserId == admin.Id && es.Percentage == 60.0);
            Assert.Contains(existingExpense.ExpenseSplits, es => es.UserId == user1.Id && es.Percentage == 20.0);
            Assert.Contains(existingExpense.ExpenseSplits, es => es.UserId == user2.Id && es.Percentage == 20.0);

        }

        #endregion

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
                    Percentage = splitDto.Percentage
                }).ToList()
            };
        }
        #endregion
    }
}