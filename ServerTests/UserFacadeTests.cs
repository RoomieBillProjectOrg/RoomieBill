using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Roomiebill.Server.Common;
using Roomiebill.Server.DataAccessLayer;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Facades;
using Roomiebill.Server.Models;
using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit;

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
        Assert.ThrowsAsync<ArgumentNullException>(() => _userFacade.RegisterUserAsync(registerDto));
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
        Assert.ThrowsAsync<ArgumentNullException>(() => _userFacade.RegisterUserAsync(registerDto));
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
        Assert.ThrowsAsync<Exception>(() => _userFacade.RegisterUserAsync(registerDto));
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
        Assert.ThrowsAsync<Exception>(() => _userFacade.RegisterUserAsync(registerDto));
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
}