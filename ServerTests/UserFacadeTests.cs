using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Facades;
using Roomiebill.Server.Models;
using Roomiebill.Server.Common.Enums;

namespace ServerTests;

public class UserFacadeTests
{
    private readonly Mock<IApplicationDbContext> _usersDbMock;
    private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
    private readonly Mock<ILogger<UserFacade>> _loggerMock;
    private readonly UserFacade _userFacade;

    public UserFacadeTests()
    {
        _usersDbMock = new Mock<IApplicationDbContext>();
        _passwordHasherMock = new Mock<IPasswordHasher<User>>();
        _loggerMock = new Mock<ILogger<UserFacade>>();

        _userFacade = new UserFacade(_usersDbMock.Object, _passwordHasherMock.Object, _loggerMock.Object);
    }

    #region RegisterUserAsync

    [Fact]
    public async Task TestRegisterUserAsync_WhenSuccessfulRegistration_ThenReturnsNewUser()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            Username = "testuser",
            Email = "test@bgu.ac.il",
            Password = "ValidPassword123!"
        };
        _passwordHasherMock.Setup(ph => ph.HashPassword(It.IsAny<User>(), registerDto.Password))
            .Returns("hashedpassword");

        // Act
        var newUser = await _userFacade.RegisterUserAsync(registerDto);

        // Assert
        Assert.NotNull(newUser);
        Assert.Equal(registerDto.Username, newUser.Username);
        Assert.Equal(registerDto.Email, newUser.Email);
        Assert.Equal("hashedpassword", newUser.PasswordHash);

        _usersDbMock.Verify(db => db.AddUser(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task TestRegisterUserAsync_WhenUsernameIsNull_ThenThrowsArgumentNullException()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            Username = null,
            Email = "test@bgu.ac.il",
            Password = "ValidPassword123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _userFacade.RegisterUserAsync(registerDto));
    }

    [Fact]
    public async Task TestRegisterUserAsync_WhenEmailIsNull_ThenThrowsArgumentNullException()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            Username = "testuser",
            Email = null,
            Password = "ValidPassword123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _userFacade.RegisterUserAsync(registerDto));
    }

    [Fact]
    public async Task TestRegisterUserAsync_WhenPasswordIsNull_ThenThrowsArgumentNullException()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            Username = "testuser",
            Email = "test@bgu.ac.il",
            Password = null
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _userFacade.RegisterUserAsync(registerDto));
    }

    [Fact]
    public async Task TestRegisterUserAsync_WhenPasswordIsInvalid_ThenThrowsExceptionWithPasswordError()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            Username = "testuser",
            Email = "test@bgu.ac.il",
            Password = "short"
        };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _userFacade.RegisterUserAsync(registerDto));
    }

    [Fact]
    public async Task TestRegisterUserAsync_WhenEmailIsInvalid_ThenThrowsExceptionWithEmailError()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            Username = "testuser",
            Email = "invalid-email",
            Password = "ValidPassword123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _userFacade.RegisterUserAsync(registerDto));
    }

    [Fact]
    public async Task TestRegisterUserAsync_WhenUsernameAlreadyExists_ThenThrowsExceptionWithDuplicateUsernameError()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            Username = "existinguser",
            Email = "test@bgu.ac.il",
            Password = "ValidPassword123!"
        };

        // Mock that the username already exists
        _usersDbMock.Setup(db => db.GetUserByUsernameAsync(registerDto.Username)).ReturnsAsync(new User());

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _userFacade.RegisterUserAsync(registerDto));
    }

    [Fact]
    public async Task TestRegisterUserAsync_WhenEmailAlreadyExists_ThenThrowsExceptionWithDuplicateEmailError()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            Username = "testuser",
            Email = "existing@bgu.ac.il",
            Password = "ValidPassword123!"
        };

        // Mock that the email already exists
        _usersDbMock.Setup(db => db.GetUserByEmail(registerDto.Email)).Returns(new User());

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _userFacade.RegisterUserAsync(registerDto));
    }

    #endregion

    #region UpdatePasswordAsync

    [Fact]
    public async Task TestUpdatePasswordAsync_WhenSuccessfulUpdate_ThenUpdatesUserPassword()
    {
        // Arrange
        var updateDto = new UpdatePasswordDto
        {
            Username = "testuser",
            CurrentPassword = "oldPassword!1",
            NewPassword = "newPassword!1",
            VerifyNewPassword = "newPassword!1"
        };
        var user = new User
        {
            Username = updateDto.Username,
            PasswordHash = "oldPassword!1"
        };
        _usersDbMock.Setup(db => db.GetUserByUsernameAsync(updateDto.Username)).ReturnsAsync(user);
        _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(user, user.PasswordHash, updateDto.CurrentPassword))
            .Returns(PasswordVerificationResult.Success);
        _passwordHasherMock.Setup(ph => ph.HashPassword(user, updateDto.NewPassword))
            .Returns("newHashedPassword!1");

        // Act
        await _userFacade.UpdatePasswordAsync(updateDto);

        // Assert
        _usersDbMock.Verify(db => db.UpdateUserAsync(It.Is<User>(u => u.PasswordHash == "newHashedPassword!1")), Times.Once);
    }

    [Fact]
    public async Task TestUpdatePasswordAsync_WhenNewPasswordInvalid_ThenThrowsException()
    {
        // Arrange
        var updateDto = new UpdatePasswordDto
        {
            Username = "testuser",
            CurrentPassword = "oldPassword!1",
            NewPassword = "short",
            VerifyNewPassword = "short"
        };
        var user = new User
        {
            Username = updateDto.Username,
            PasswordHash = "oldPassword!1"
        };
        _usersDbMock.Setup(db => db.GetUserByUsernameAsync(updateDto.Username)).ReturnsAsync(user);
        _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(user, user.PasswordHash, updateDto.CurrentPassword))
            .Returns(PasswordVerificationResult.Success);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _userFacade.UpdatePasswordAsync(updateDto));
        Assert.Equal("Password must be at least 8 characters long", exception.Message);
    }

    [Fact]
    public async Task TestUpdatePasswordAsync_WhenCurrentPasswordIncorrect_ThenThrowsException()
    {
        // Arrange
        var updateDto = new UpdatePasswordDto
        {
            Username = "testuser",
            CurrentPassword = "wrongPassword!1",
            NewPassword = "newPassword!1",
            VerifyNewPassword = "newPassword!1"
        };
        var user = new User
        {
            Username = updateDto.Username,
            PasswordHash = "oldPassword!1"
        };
        _usersDbMock.Setup(db => db.GetUserByUsernameAsync(updateDto.Username)).ReturnsAsync(user);
        _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(user, user.PasswordHash, updateDto.CurrentPassword))
            .Returns(PasswordVerificationResult.Failed);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _userFacade.UpdatePasswordAsync(updateDto));
        Assert.Equal("Old password is incorrect", exception.Message);
    }

    [Fact]
    public async Task TestUpdatePasswordAsync_WhenUserNotFound_ThenThrowsException()
    {
        // Arrange
        var updateDto = new UpdatePasswordDto
        {
            Username = "nonexistentuser",
            CurrentPassword = "oldPassword!1",
            NewPassword = "newPassword!1",
            VerifyNewPassword = "newPassword!1"
        };
        _usersDbMock.Setup(db => db.GetUserByUsernameAsync(updateDto.Username)).ReturnsAsync((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _userFacade.UpdatePasswordAsync(updateDto));
        Assert.Equal("User with this username does not exist", exception.Message);
    }

    [Fact]
    public async Task TestUpdatePasswordAsync_WhenPasswordsDoNotMatch_ThenThrowsException()
    {
        // Arrange
        var updateDto = new UpdatePasswordDto
        {
            Username = "testuser",
            CurrentPassword = "oldPassword!1",
            NewPassword = "newPassword!1",
            VerifyNewPassword = "newPassword!2"
        };

        var user = new User
        {
            Username = updateDto.Username,
            PasswordHash = "oldPassword!1"
        };

        _usersDbMock.Setup(db => db.GetUserByUsernameAsync(updateDto.Username)).ReturnsAsync(user);
        _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(user, user.PasswordHash, updateDto.CurrentPassword))
            .Returns(PasswordVerificationResult.Success);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _userFacade.UpdatePasswordAsync(updateDto));
    }

    [Fact]
    public async Task TestUpdatePasswordAsync_WhenNewPasswordIsNull_ThenThrowsArgumentNullException()
    {
        // Arrange
        var updateDto = new UpdatePasswordDto
        {
            Username = "testuser",
            CurrentPassword = "oldPassword!1",
            NewPassword = null,
            VerifyNewPassword = "newPassword!1"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _userFacade.UpdatePasswordAsync(updateDto));
    }

    #endregion

    #region LoginAsync
    [Fact]
    public async Task TestLoginAsync_WhenSuccessfulLogin_ThenReturnsUser()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Username = "testuser",
            Password = "ValidPassword123!"
        };
        var user = new User
        {
            Username = loginDto.Username,
            PasswordHash = "hashedpassword"
        };
        _usersDbMock.Setup(db => db.GetUserByUsernameAsync(loginDto.Username)).ReturnsAsync(user);
        _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password))
            .Returns(PasswordVerificationResult.Success);

        // Act
        var loggedInUser = await _userFacade.LoginAsync(loginDto);

        // Assert
        Assert.NotNull(loggedInUser);
        Assert.Equal(loginDto.Username, loggedInUser.Username);
        Assert.Equal(user.PasswordHash, loggedInUser.PasswordHash);
    }

    [Fact]
    public async Task TestLoginAsync_WhenUserNotFound_ThenReturnsThrowException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Username = "nonexistentuser",
            Password = "ValidPassword123!"
        };
        _usersDbMock.Setup(db => db.GetUserByUsernameAsync(loginDto.Username)).ReturnsAsync((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _userFacade.LoginAsync(loginDto));
    }

    [Fact]
    public async Task TestLoginAsync_WhenPasswordIncorrect_ThenReturnsPasswordNotCorrectError()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Username = "testuser",
            Password = "InvalidPassword123!"
        };
        var user = new User
        {
            Username = loginDto.Username,
            PasswordHash = "hashedpassword"
        };
        _usersDbMock.Setup(db => db.GetUserByUsernameAsync(loginDto.Username)).ReturnsAsync(user);
        _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password))
            .Returns(PasswordVerificationResult.Failed);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _userFacade.LoginAsync(loginDto));

    }

    #endregion

    #region LogoutAsync
    [Fact]
    public async Task TestLogoutAsync_WhenSuccessfulLogout_ThenLogsOutUser()
    {
        // Arrange
        var username = "testuser";
        var user = new User
        {
            Username = username
        };

        _usersDbMock.Setup(db => db.GetUserByUsernameAsync(username)).ReturnsAsync(user);

        // Act
        await _userFacade.LogoutAsync(username);

        // Assert
        _usersDbMock.Verify(db => db.UpdateUserAsync(user), Times.Once);
    }

    [Fact]
    public async Task TestLogoutAsync_WhenUserNotFound_ThenThrowsException()
    {
        // Arrange
        var username = "nonexistentuser";

        _usersDbMock.Setup(db => db.GetUserByUsernameAsync(username)).ReturnsAsync((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _userFacade.LogoutAsync(username));
    }

    #endregion

    #region AnswerInviteByUser

    [Fact]
    public async Task TestAnswerInviteByUser_WhenTrueAnswer_ThenUpdatesInviteAndGroup()
    {
        // Arrange
            string inviterUsername = "inviter";
            string invitedUsername = "invited";
            
            User inviter = new User(inviterUsername, "Metar@bgu.ac.il",  "MetarPassword2@");
            User invited = new User(invitedUsername, "Metar2@bgu.ac.il",  "MetarPassword2@");

            Group group = new Group();

            Invite invite = new Invite(inviter, invited, group);

            group.AddInvite(invite);
            invited.AddInvite(invite);

            _usersDbMock.Setup(db => db.GetInviteByIdAsync(invite.Id)).ReturnsAsync(invite);

            // Act 
            await _userFacade.AnswerInviteByUser(invite.Id, true);

            // Assert
            Assert.True(invite.Status == Status.Accepted);
            Assert.True(group.Invites[0] == invite);
    }

    [Fact]
    public async Task TestAnswerInviteByUser_WhenFalseAnswer_ThenUpdatesInviteAndGroup()
    {
        // Arrange
            string inviterUsername = "inviter";
            string invitedUsername = "invited";
            
            User inviter = new User(inviterUsername, "Metar@bgu.ac.il",  "MetarPassword2@");
            User invited = new User(invitedUsername, "Metar2@bgu.ac.il",  "MetarPassword2@");

            Group group = new Group();

            Invite invite = new Invite(inviter, invited, group);

            group.AddInvite(invite);
            invited.AddInvite(invite);

            _usersDbMock.Setup(db => db.GetInviteByIdAsync(invite.Id)).ReturnsAsync(invite);

            // Act 
            await _userFacade.AnswerInviteByUser(invite.Id, false);

            // Assert
            Assert.True(invite.Status == Status.Rejected);
            Assert.True(group.Invites[0] == invite);
    }

    #endregion
}