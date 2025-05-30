using Microsoft.AspNetCore.Mvc;
using Moq;
using Roomiebill.Server.Controllers;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services.Interfaces;
using Xunit;

namespace ServerTests.AcceptanceTests
{
    public class InvitesControllerAcceptanceTests
    {
        [Fact]
        public async Task AnswerInvite_WithValidAcceptData_ShouldSucceed()
        {
            // Arrange
            var inviteAnswer = new AnswerInviteByUserDto
            {
                InviteId = 1,
                InvitedUsername = "testuser",
                IsAccepted = true
            };

            // Act
            var controller = CreateController();
            var result = await controller.AnswerInvite(inviteAnswer);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(okResult.Value);
            Assert.Equal("Invite accepted successfully", response.Message);
        }

        [Fact]
        public async Task AnswerInvite_WithNullInput_ShouldReturnBadRequest()
        {
            // Arrange
            AnswerInviteByUserDto inviteAnswer = null;

            // Act
            var controller = CreateController();
            var result = await controller.AnswerInvite(inviteAnswer);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(badRequestResult.Value);
            Assert.Equal("Invalid request: Input cannot be null", response.Message);
        }

        [Fact]
        public async Task AnswerInvite_WithValidRejectData_ShouldSucceed()
        {
            // Arrange
            var inviteAnswer = new AnswerInviteByUserDto
            {
                InviteId = 2,
                InvitedUsername = "testuser2",
                IsAccepted = false
            };

            var controller = CreateController();
            var result = await controller.AnswerInvite(inviteAnswer);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(okResult.Value);
            // The controller may have a different message for rejection; adjust as needed
            Assert.Contains("success", response.Message.ToLower());
        }

        [Fact]
        public async Task AnswerInvite_WhenServiceThrows_ShouldReturnBadRequest()
        {
            // Arrange
            var inviteService = new Mock<IInviteService>();
            inviteService.Setup(s => s.AnswerInviteByUser(It.IsAny<AnswerInviteByUserDto>()))
                .ThrowsAsync(new Exception("Unexpected error"));
            var controller = new InvitesController(inviteService.Object);

            var inviteAnswer = new AnswerInviteByUserDto
            {
                InviteId = 3,
                InvitedUsername = "failuser",
                IsAccepted = true
            };

            // Act
            var result = await controller.AnswerInvite(inviteAnswer);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(badRequestResult.Value);
            Assert.Contains("unexpected error", response.Message.ToLower());
        }

        [Fact]
        public async Task InviteToGroupByEmail_WithValidData_ShouldSucceed()
        {
            // Arrange
            var inviteDetails = new InviteToGroupByEmailDto
            {
                GroupId = 1,
                InviterUsername = "admin",
                Email = "newuser@example.com"
            };

            // Act
            var controller = CreateController();
            var result = await controller.InviteToGroupByEmail(inviteDetails);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(okResult.Value);
            Assert.Equal("Invite sent successfully", response.Message);
        }

        [Fact]
        public async Task InviteToGroupByEmail_WithNullInput_ShouldReturnBadRequest()
        {
            // Arrange
            InviteToGroupByEmailDto inviteDetails = null;

            // Act
            var controller = CreateController();
            var result = await controller.InviteToGroupByEmail(inviteDetails);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(badRequestResult.Value);
            Assert.Equal("Invalid request: Input cannot be null", response.Message);
        }

        [Fact]
        public async Task InviteToGroupByEmail_WithInvalidEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var inviteDetails = new InviteToGroupByEmailDto
            {
                GroupId = 1,
                InviterUsername = "admin",
                Email = "invalid-email"
            };

            // Act
            var controller = CreateController();
            var result = await controller.InviteToGroupByEmail(inviteDetails);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(badRequestResult.Value);
            Assert.Contains("invalid", response.Message.ToLower());
        }

        [Fact]
        public async Task InviteToGroupByEmail_WithNonexistentGroup_ShouldReturnBadRequest()
        {
            // Arrange
            var inviteDetails = new InviteToGroupByEmailDto
            {
                GroupId = 999,
                InviterUsername = "admin",
                Email = "newuser@example.com"
            };

            // Act
            var controller = CreateController();
            var result = await controller.InviteToGroupByEmail(inviteDetails);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(badRequestResult.Value);
            Assert.Contains("group", response.Message.ToLower());
        }

        [Fact]
        public async Task InviteToGroupByEmail_WhenServiceThrowsGenericException_ShouldReturnBadRequest()
        {
            // Arrange
            var inviteService = new Mock<IInviteService>();
            inviteService.Setup(s => s.InviteToGroupByEmail(It.IsAny<InviteToGroupByEmailDto>()))
                .ThrowsAsync(new Exception("Unexpected invite error"));
            var controller = new InvitesController(inviteService.Object);

            var inviteDetails = new InviteToGroupByEmailDto
            {
                GroupId = 2,
                InviterUsername = "admin",
                Email = "user@domain.com"
            };

            // Act
            var result = await controller.InviteToGroupByEmail(inviteDetails);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(badRequestResult.Value);
            Assert.Contains("unexpected invite error", response.Message.ToLower());
        }

        [Fact]
        public async Task InviteToGroupByEmail_WithEmptyEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var inviteDetails = new InviteToGroupByEmailDto
            {
                GroupId = 1,
                InviterUsername = "admin",
                Email = ""
            };

            var controller = CreateController();
            var result = await controller.InviteToGroupByEmail(inviteDetails);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(badRequestResult.Value);
            Assert.Contains("invalid", response.Message.ToLower());
        }

        [Fact]
        public async Task InviteToGroupByEmail_WithNullUsername_ShouldReturnBadRequest()
        {
            // Arrange
            var inviteDetails = new InviteToGroupByEmailDto
            {
                GroupId = 1,
                InviterUsername = null,
                Email = "user@domain.com"
            };

            var controller = CreateController();
            var result = await controller.InviteToGroupByEmail(inviteDetails);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<MessageResponse>(badRequestResult.Value);
            Assert.Contains("invalid", response.Message.ToLower());
        }

        private InvitesController CreateController()
        {
            var inviteService = new Mock<IInviteService>();
            
            // Configure mock service
            inviteService.Setup(s => s.AnswerInviteByUser(It.Is<AnswerInviteByUserDto>(a => a != null && a.IsAccepted)))
                .Returns(Task.CompletedTask);

            inviteService.Setup(s => s.InviteToGroupByEmail(It.Is<InviteToGroupByEmailDto>(i => 
                i != null && i.Email.Contains("@") && i.GroupId != 999)))
                .Returns(Task.CompletedTask);

            inviteService.Setup(s => s.InviteToGroupByEmail(It.Is<InviteToGroupByEmailDto>(i => 
                i != null && !i.Email.Contains("@"))))
                .ThrowsAsync(new ArgumentException("Invalid email format"));

            inviteService.Setup(s => s.InviteToGroupByEmail(It.Is<InviteToGroupByEmailDto>(i => 
                i != null && i.GroupId == 999)))
                .ThrowsAsync(new ArgumentException("Group not found"));

            return new InvitesController(inviteService.Object);
        }
    }
}
