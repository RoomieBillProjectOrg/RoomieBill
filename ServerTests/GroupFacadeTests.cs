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
        private readonly Mock<IGroupDb> _groupDbMock;
        private readonly Mock<ILogger<GroupFacade>> _loggerMock;
        private readonly Mock<IUserFacade> _userFacadeMock;
        private GroupFacade _groupFacade;

        public GroupFacadeTests()
        {
            _groupDbMock = new Mock<IGroupDb>();
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

        #endregion
    }
}
