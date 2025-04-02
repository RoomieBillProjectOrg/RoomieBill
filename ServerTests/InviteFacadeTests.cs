using Microsoft.Extensions.Logging;
using Moq;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.Facades;
using Roomiebill.Server.Models;

namespace ServerTests
{
    public class InviteFacadeTests
    {
        private readonly Mock<IApplicationDbContext> _applicationDbs;
        private readonly Mock<ILogger<InviteFacade>> _loggerMock;
        private readonly Mock<IUserFacade> _userFacadeMock;
        private readonly Mock<IGroupFacade> _groupFacadeMock;
        private InviteFacade _inviteFacade;

        public InviteFacadeTests()
        {
            _applicationDbs = new Mock<IApplicationDbContext>();
            _loggerMock = new Mock<ILogger<InviteFacade>>();
            _userFacadeMock = new Mock<IUserFacade>();
            _groupFacadeMock = new Mock<IGroupFacade>();
            _inviteFacade = new InviteFacade(_applicationDbs.Object, _loggerMock.Object, _userFacadeMock.Object, _groupFacadeMock.Object);
        }

        #region InviteToGroupByUsername

        [Fact]
        public async Task TestInviteToGroupByUsername_WhenInviterDoesNotExist_ThenThrowsException()
        {
            // Arrange
            string inviterUsername = "inviter";
            int groupId = 1;

            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync(inviterUsername))!.ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _inviteFacade.InviteToGroupByEmail(inviterUsername, "email", groupId));
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
            await Assert.ThrowsAsync<Exception>(() => _inviteFacade.InviteToGroupByEmail(inviterUsername, inviter.Email, groupId));
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

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _inviteFacade.InviteToGroupByEmail(inviterUsername, invited.Email, groupId));
        }

        [Fact]
        public async Task TestInviteToGroupByUsername_WhenInviteForInvitedExistInGroup_ThenThrowsException()
        {
            // Arrange
            string inviterUsername = "inviter";
            string invitedUsername = "invited";
            
            User inviter = new User(inviterUsername, "Metar@bgu.ac.il",  "MetarPassword2@");
            User invited = new User(invitedUsername, "Metar2@bgu.ac.il",  "MetarPassword2@");
            Group group = new Group();
            group.AddMember(inviter);
            group.AddInvite(new Invite(inviter, invited, group));
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync(inviterUsername))!.ReturnsAsync(inviter);
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync(invitedUsername))!.ReturnsAsync(invited);
            _applicationDbs.Setup(x => x.GetGroupByIdAsync(group.Id))!.ReturnsAsync(group);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _inviteFacade.InviteToGroupByEmail(inviterUsername, invited.Email, group.Id));
        }

        [Fact]
        public async Task TestInviteToGroupByUsername_WhenInviterNotInGroup_ThenThrowsException()
        {
            // Arrange
            string inviterUsername = "inviter";
            string invitedUsername = "invited";
            
            User inviter = new User("inviter", "Metar@bgu.ac.il",  "MetarPassword2@");
            User invited = new User("invited", "Metar2@bgu.ac.il",  "MetarPassword2@");
            Group group = new Group();
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync(inviterUsername))!.ReturnsAsync(inviter);
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync(invitedUsername))!.ReturnsAsync(invited);
            _applicationDbs.Setup(x => x.GetGroupByIdAsync(group.Id))!.ReturnsAsync(group);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _inviteFacade.InviteToGroupByEmail(inviterUsername, invited.Email, group.Id));
        }

        [Fact]
        public async Task TestInviteToGroupByUsername_WhenInvitedAlreadyInGroup_ThenThrowsException()
        {
            // Arrange
            string inviterUsername = "inviter";
            string invitedUsername = "invited";
            
            User inviter = new User(inviterUsername, "Metar@bgu.ac.il",  "MetarPassword2@");
            User invited = new User(invitedUsername, "Metar2@bgu.ac.il",  "MetarPassword2@");
            Group group = new Group();
            group.AddMember(inviter);
            group.AddMember(invited);
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync(inviterUsername))!.ReturnsAsync(inviter);
            _userFacadeMock.Setup(x => x.GetUserByUsernameAsync(invitedUsername))!.ReturnsAsync(invited);
            _applicationDbs.Setup(x => x.GetGroupByIdAsync(group.Id))!.ReturnsAsync(group);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _inviteFacade.InviteToGroupByEmail(inviterUsername, invited.Email, group.Id));
        }

        //[Fact]
        //public async Task TestInviteToGroupByUsername_WhenAllExistAndInviterInGroupAndInvitedIsnt_ThenSendInvite()
        //{
        //    // Arrange
        //    string inviterUsername = "inviter";
        //    string invitedUsername = "invited";

        //    User inviter = new User(inviterUsername, "Metar@bgu.ac.il", "MetarPassword2@", firebaseToken: "token");
        //    User invited = new User(invitedUsername, "Metar2@bgu.ac.il", "MetarPassword2@", firebaseToken: "token");
        //    Group group = new Group();
        //    group.AddMember(inviter);
        //    _userFacadeMock.Setup(x => x.GetUserByUsernameAsync(inviterUsername))!.ReturnsAsync(inviter);
        //    _userFacadeMock.Setup(x => x.GetUserByUsernameAsync(invitedUsername))!.ReturnsAsync(invited);
        //    _applicationDbs.Setup(x => x.GetGroupByIdAsync(group.Id))!.ReturnsAsync(group);
        //    _groupFacadeMock.Setup(x => x.IsUserInGroup(inviter, group))!.Returns(true);

        //    // Act 
        //    await _inviteFacade.InviteToGroupByUsername(inviterUsername, invitedUsername, group.Id);

        //    // Assert
        //    Assert.NotEmpty(group.Invites);
        //}

        #endregion

        #region AnswerInviteByUser

        [Fact]
        public async Task TestAnswerInviteByUser_WhenTrueAnswer_ThenAddUserToGroup()
        {
            // Arrange
            string inviterUsername = "inviter";
            string invitedUsername = "invited";
            
            User inviter = new User(inviterUsername, "Metar@bgu.ac.il",  "MetarPassword2@");
            User invited = new User(invitedUsername, "Metar2@bgu.ac.il",  "MetarPassword2@");

            Group group = new Group();

            Invite invite = new Invite(inviter, invited, group);

            _applicationDbs.Setup(x => x.GetInviteByIdAsync(invite.Id))!.ReturnsAsync(invite);
            _groupFacadeMock.Setup(x => x.AddMemberToGroupAsync(invite.Invited, invite.Group))
                .Callback<User, Group>((user, grp) =>
                {
                    grp.AddMember(user); // Simulate adding the member to the group
                })
                .Returns(Task.CompletedTask);
        
            // Act
            await _inviteFacade.AnswerInviteByUser(invite.Id, true);

            // Assert
            Assert.Contains(invited, group.GetMembers());
        }

        [Fact]
        public async Task TestAnswerInviteByUser_WhenFalseAnswer_ThenDontAddUserToGroup()
        {
            // Arrange
            string inviterUsername = "inviter";
            string invitedUsername = "invited";
            
            User inviter = new User(inviterUsername, "Metar@bgu.ac.il",  "MetarPassword2@");
            User invited = new User(invitedUsername, "Metar2@bgu.ac.il",  "MetarPassword2@");

            Group group = new Group();

            Invite invite = new Invite(inviter, invited, group);

            _applicationDbs.Setup(x => x.GetInviteByIdAsync(invite.Id))!.ReturnsAsync(invite);
        
            // Act
            await _inviteFacade.AnswerInviteByUser(invite.Id, false);

            // Assert
            Assert.DoesNotContain(invited, group.GetMembers());
        }

        [Fact]
        public async Task TestAnswerInviteByUser_WhenAnswerAndInviteIsAccepted_ThenDontAddUserToGroup()
        {
            // Arrange
            string inviterUsername = "inviter";
            string invitedUsername = "invited";
            
            User inviter = new User(inviterUsername, "Metar@bgu.ac.il",  "MetarPassword2@");
            User invited = new User(invitedUsername, "Metar2@bgu.ac.il",  "MetarPassword2@");

            Group group = new Group();

            Invite invite = new Invite(inviter, invited, group);

            invite.AcceptInvite();

            _applicationDbs.Setup(x => x.GetInviteByIdAsync(invite.Id))!.ReturnsAsync(invite);
        
            // Act
            await Assert.ThrowsAsync<Exception>(() => _inviteFacade.AnswerInviteByUser(invite.Id, false));
            
            // Assert
            Assert.DoesNotContain(invited, group.GetMembers());
        }

        [Fact]
        public async Task TestAnswerInviteByUser_WhenAnswerAndInviteIsRejected_ThenDontAddUserToGroup()
        {
            // Arrange
            string inviterUsername = "inviter";
            string invitedUsername = "invited";
            
            User inviter = new User(inviterUsername, "Metar@bgu.ac.il",  "MetarPassword2@");
            User invited = new User(invitedUsername, "Metar2@bgu.ac.il",  "MetarPassword2@");

            Group group = new Group();

            Invite invite = new Invite(inviter, invited, group);

            invite.RejectInvite();

            _applicationDbs.Setup(x => x.GetInviteByIdAsync(invite.Id))!.ReturnsAsync(invite);
        
            // Act
            await Assert.ThrowsAsync<Exception>(() => _inviteFacade.AnswerInviteByUser(invite.Id, false));
            
            // Assert
            Assert.DoesNotContain(invited, group.GetMembers());
        }

        #endregion

    }
}
