using Microsoft.AspNetCore.Mvc;
using Moq;
using Roomiebill.Server.Controllers;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services;
using Xunit;

namespace ServerTests
{
    public class UsersControllerTests
    {
        private readonly Mock<UserService> _mockUserService;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mockUserService = new Mock<UserService>();
            _controller = new UsersController(_mockUserService.Object);
        }

        [Fact]
        public async Task TestThatWhenRegisteringValidUserThenReturnsSuccess()
        {
            RegisterUserDto userDto = new RegisterUserDto 
            { 
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!" 
            };

            User registeredUser = new User { Username = userDto.Username };
            _mockUserService.Setup(s => s.RegisterUserAsync(userDto))
                          .ReturnsAsync(registeredUser);

            IActionResult result = await _controller.RegisterUser(userDto);
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("User registered successfully", (okResult.Value as dynamic).Message);
        }

        [Fact]
        public async Task TestThatWhenRegistrationFailsThenReturnsBadRequest()
        {
            RegisterUserDto userDto = new RegisterUserDto();
            string errorMessage = "Registration failed";
            _mockUserService.Setup(s => s.RegisterUserAsync(userDto))
                          .ThrowsAsync(new Exception(errorMessage));

            IActionResult result = await _controller.RegisterUser(userDto);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, (badRequest.Value as dynamic).Message);
        }

        [Fact]
        public async Task TestThatWhenVerifyingValidUserDetailsThenReturnsVerificationCode()
        {
            RegisterUserDto userDto = new RegisterUserDto 
            { 
                Email = "test@example.com" 
            };
            VerifiyCodeModel expectedCode = new VerifiyCodeModel { VerifyCode = "123456" };

            _mockUserService.Setup(s => s.VerifyRegisterUserDetailsAsync(userDto))
                          .Returns(Task.CompletedTask);

            IActionResult result = await _controller.VerifyUserRegisterDetails(userDto);
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCode = Assert.IsType<VerifiyCodeModel>(okResult.Value);
            Assert.NotNull(returnedCode.VerifyCode);
        }

        [Fact]
        public async Task TestThatWhenVerificationFailsThenReturnsBadRequest()
        {
            RegisterUserDto userDto = new RegisterUserDto();
            string errorMessage = "Verification failed";
            _mockUserService.Setup(s => s.VerifyRegisterUserDetailsAsync(userDto))
                          .ThrowsAsync(new Exception(errorMessage));

            IActionResult result = await _controller.VerifyUserRegisterDetails(userDto);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, (badRequest.Value as dynamic).Message);
        }

        [Fact]
        public async Task TestThatWhenUpdatingPasswordWithValidDataThenReturnsUpdatedUser()
        {
            UpdatePasswordDto updateDto = new UpdatePasswordDto 
            { 
                Username = "testuser",
                OldPassword = "OldPass123!",
                NewPassword = "NewPass123!" 
            };

            User updatedUser = new User { Username = "testuser" };
            _mockUserService.Setup(s => s.UpdatePasswordAsync(updateDto))
                          .ReturnsAsync(updatedUser);

            IActionResult result = await _controller.UpdatePassword(updateDto);
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(updatedUser, okResult.Value);
        }

        [Fact]
        public async Task TestThatWhenPasswordUpdateFailsThenReturnsBadRequest()
        {
            UpdatePasswordDto updateDto = new UpdatePasswordDto();
            string errorMessage = "Password update failed";
            _mockUserService.Setup(s => s.UpdatePasswordAsync(updateDto))
                          .ThrowsAsync(new Exception(errorMessage));

            IActionResult result = await _controller.UpdatePassword(updateDto);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, (badRequest.Value as dynamic).Message);
        }

        [Fact]
        public async Task TestThatWhenLoginSucceedsThenReturnsUser()
        {
            LoginDto loginDto = new LoginDto 
            { 
                Username = "testuser",
                Password = "Password123!" 
            };

            User user = new User { Username = "testuser" };
            _mockUserService.Setup(s => s.LoginAsync(loginDto))
                          .ReturnsAsync(user);

            IActionResult result = await _controller.Login(loginDto);
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(user, okResult.Value);
        }

        [Fact]
        public async Task TestThatWhenLoginFailsThenReturnsBadRequest()
        {
            LoginDto loginDto = new LoginDto();
            string errorMessage = "Login failed";
            _mockUserService.Setup(s => s.LoginAsync(loginDto))
                          .ThrowsAsync(new Exception(errorMessage));

            IActionResult result = await _controller.Login(loginDto);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, (badRequest.Value as dynamic).Message);
        }

        [Fact]
        public async Task TestThatWhenLogoutSucceedsThenReturnsSuccess()
        {
            string username = "testuser";
            _mockUserService.Setup(s => s.LogoutAsync(username))
                          .Returns(Task.CompletedTask);

            IActionResult result = await _controller.Logout(username);
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("User logged out successfully", (okResult.Value as dynamic).Message);
        }

        [Fact]
        public async Task TestThatWhenLogoutFailsThenReturnsBadRequest()
        {
            string username = "testuser";
            string errorMessage = "Logout failed";
            _mockUserService.Setup(s => s.LogoutAsync(username))
                          .ThrowsAsync(new Exception(errorMessage));

            IActionResult result = await _controller.Logout(username);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, (badRequest.Value as dynamic).Message);
        }

        [Fact]
        public async Task TestThatWhenGettingUserInvitesThenReturnsInvitesList()
        {
            string username = "testuser";
            List<Invite> invites = new List<Invite> 
            { 
                new Invite { Id = 1 },
                new Invite { Id = 2 } 
            };

            _mockUserService.Setup(s => s.GetUserInvitesAsync(username))
                          .ReturnsAsync(invites);

            IActionResult result = await _controller.GetUserGroups(username);
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(invites, okResult.Value);
        }

        [Fact]
        public async Task TestThatWhenGettingInvitesFailsThenReturnsBadRequest()
        {
            string username = "testuser";
            string errorMessage = "Failed to get invites";
            _mockUserService.Setup(s => s.GetUserInvitesAsync(username))
                          .ThrowsAsync(new Exception(errorMessage));

            IActionResult result = await _controller.GetUserGroups(username);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, (badRequest.Value as dynamic).Message);
        }
    }
}
