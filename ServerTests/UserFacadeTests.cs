using Microsoft.AspNetCore.Identity;
using Moq;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Facades;
using Roomiebill.Server.Models;
using Microsoft.Extensions.Logging;

public class UserFacadeTests
{
    private readonly Mock<IUsersDb> _usersDbMock;
    private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
    private readonly Mock<ILogger<UserFacade>> _loggerMock;
    private readonly UserFacade _userFacade;

    public UserFacadeTests()
    {
        // Mock DbSet
        _usersDbMock = new Mock<IUsersDb>();
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
        _usersDbMock.Setup(db => db.GetUserByUsername(registerDto.Username)).Returns(new User());

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
            NewPassword = "newPassword!1"
        };
        var user = new User
        {
            Username = updateDto.Username,
            PasswordHash = "oldPassword!1"
        };
        _usersDbMock.Setup(db => db.GetUserByUsername(updateDto.Username)).Returns(user);
        _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(user, user.PasswordHash, updateDto.CurrentPassword))
            .Returns(PasswordVerificationResult.Success);
        _passwordHasherMock.Setup(ph => ph.HashPassword(user, updateDto.NewPassword))
            .Returns("newHashedPassword!1");

        // Act
        await _userFacade.UpdatePasswordAsync(updateDto);

        // Assert
        _usersDbMock.Verify(db => db.UpdateUser(It.Is<User>(u => u.PasswordHash == "newHashedPassword!1")), Times.Once);
    }

    [Fact]
    public async Task TestUpdatePasswordAsync_WhenNewPasswordInvalid_ThenThrowsException()
    {
        // Arrange
        var updateDto = new UpdatePasswordDto
        {
            Username = "testuser",
            CurrentPassword = "oldPassword!1",
            NewPassword = "short"
        };
        var user = new User
        {
            Username = updateDto.Username,
            PasswordHash = "oldPassword!1"
        };
        _usersDbMock.Setup(db => db.GetUserByUsername(updateDto.Username)).Returns(user);
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
            NewPassword = "newPassword!1"
        };
        var user = new User
        {
            Username = updateDto.Username,
            PasswordHash = "oldPassword!1"
        };
        _usersDbMock.Setup(db => db.GetUserByUsername(updateDto.Username)).Returns(user);
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
            NewPassword = "newPassword!1"
        };
        _usersDbMock.Setup(db => db.GetUserByUsername(updateDto.Username)).Returns((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _userFacade.UpdatePasswordAsync(updateDto));
        Assert.Equal("User with this username does not exist", exception.Message);
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
        _usersDbMock.Setup(db => db.GetUserByUsername(loginDto.Username)).Returns(user);
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
        _usersDbMock.Setup(db => db.GetUserByUsername(loginDto.Username)).Returns((User)null);

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
        _usersDbMock.Setup(db => db.GetUserByUsername(loginDto.Username)).Returns(user);
        _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password))
            .Returns(PasswordVerificationResult.Failed);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _userFacade.LoginAsync(loginDto));

    }

    #endregion
}