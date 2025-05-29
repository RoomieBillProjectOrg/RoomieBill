using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.Extensions.Configuration;
using Roomiebill.Server.Controllers;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services.Interfaces;
using Xunit;

namespace ServerTests.AcceptanceTests
{
    public class UsersControllerAcceptanceTests
    {
        [Fact]
        public async Task RegisterUser_WithValidData_ShouldSucceed()
        {
            // Arrange
            var validUser = new RegisterUserDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!",
                BitLink = "https://bit.wallet/test",
                FirebaseToken = "firebase-token-123"
            };

            // Act
            var controller = CreateController();
            var result = await controller.RegisterUser(validUser);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(okResult.Value);
            Assert.Equal("User registered successfully", response.Message);
        }

        [Fact]
        public async Task RegisterUser_WithInvalidEmail_ShouldFail()
        {
            // Arrange
            var invalidUser = new RegisterUserDto
            {
                Username = "testuser",
                Email = "invalid-email",
                Password = "Password123!",
                BitLink = "https://bit.wallet/test",
                FirebaseToken = "firebase-token-123"
            };

            // Act
            var controller = CreateController();
            var result = await controller.RegisterUser(invalidUser);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(badRequestResult.Value);
            Assert.Contains("invalid", response.Message.ToLower());
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnUserData()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "testuser",
                Password = "Password123!"
            };

            // Act
            var controller = CreateController();
            var result = await controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldFail()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "nonexistentuser",
                Password = "wrongpassword"
            };

            // Act
            var controller = CreateController();
            var result = await controller.Login(loginDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(badRequestResult.Value);
            Assert.Contains("invalid", response.Message.ToLower());
        }

        [Fact]
        public async Task UpdatePassword_WithValidData_ShouldSucceed()
        {
            // Arrange
            var updatePasswordDto = new UpdatePasswordDto
            {
                Username = "testuser",
                OldPassword = "Password123!",
                NewPassword = "NewPassword123!"
            };

            // Act
            var controller = CreateController();
            var result = await controller.UpdatePassword(updatePasswordDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task UpdatePassword_WithInvalidOldPassword_ShouldFail()
        {
            // Arrange
            var updatePasswordDto = new UpdatePasswordDto
            {
                Username = "testuser",
                OldPassword = "WrongPassword123!",
                NewPassword = "NewPassword123!"
            };

            // Act
            var controller = CreateController();
            var result = await controller.UpdatePassword(updatePasswordDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(badRequestResult.Value);
            Assert.Contains("invalid", response.Message.ToLower());
        }

        [Fact]
        public async Task Logout_WithValidUsername_ShouldSucceed()
        {
            // Arrange
            string username = "testuser";

            // Act
            var controller = CreateController();
            var result = await controller.Logout(username);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(okResult.Value);
            Assert.Equal("User logged out successfully", response.Message);
        }

        [Fact]
        public async Task GetUserInvites_WithValidUsername_ShouldReturnInvitesList()
        {
            // Arrange
            string username = "testuser";

            // Act
            var controller = CreateController();
            var result = await controller.GetUserInvites(username);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var invites = Assert.IsType<List<Invite>>(okResult.Value);
            Assert.NotNull(invites);
        }

        [Fact]
        public async Task VerifyUserRegisterDetails_WithValidData_ShouldReturnVerificationCode()
        {
            // Arrange
            var validUser = new RegisterUserDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!",
                BitLink = "https://bit.wallet/test",
                FirebaseToken = "firebase-token-123"
            };

            // Act
            var controller = CreateController();
            var result = await controller.VerifyUserRegisterDetails(validUser);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var verifyCode = Assert.IsType<VerifiyCodeModel>(okResult.Value);
            Assert.NotNull(verifyCode.VerifyCode);
        }

        private UsersController CreateController()
        {
            var userService = new Mock<IUserService>();
            var testUser = new User("testuser", "test@example.com", "passwordhash", "bitlink");

            // Setup successful responses
            userService.Setup(s => s.LoginAsync(It.Is<LoginDto>(x => 
                x.Username == "testuser" && x.Password == "Password123!")))
                .ReturnsAsync(testUser);

            userService.Setup(s => s.UpdatePasswordAsync(It.Is<UpdatePasswordDto>(x => 
                x.Username == "testuser" && x.OldPassword == "Password123!")))
                .ReturnsAsync(testUser);

            userService.Setup(s => s.GetUserInvitesAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<Invite>());

            // Setup failure responses
            userService.Setup(s => s.LoginAsync(It.Is<LoginDto>(x => 
                x.Username == "nonexistentuser")))
                .ThrowsAsync(new Exception("Invalid username or password"));

            userService.Setup(s => s.UpdatePasswordAsync(It.Is<UpdatePasswordDto>(x => 
                x.OldPassword == "WrongPassword123!")))
                .ThrowsAsync(new Exception("Invalid old password"));

            userService.Setup(s => s.VerifyRegisterUserDetailsAsync(It.Is<RegisterUserDto>(x => 
                x.Email == "invalid-email")))
                .ThrowsAsync(new Exception("Invalid email format"));

            return new UsersController(userService.Object);
        }
    }
}
