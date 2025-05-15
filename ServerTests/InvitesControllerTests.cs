using Microsoft.AspNetCore.Mvc;
using Moq;
using Roomiebill.Server.Controllers;
using Roomiebill.Server.DataAccessLayer.Dtos;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services;
using Xunit;

namespace ServerTests
{
    public class InvitesControllerTests
    {
        private readonly Mock<InviteService> _mockInviteService;
        private readonly InvitesController _controller;

        public InvitesControllerTests()
        {
            _mockInviteService = new Mock<InviteService>();
            _controller = new InvitesController(_mockInviteService.Object);
        }

        [Fact]
        public async Task TestThatWhenAnsweringInviteThenReturnsSuccess()
        {
            AnswerInviteByUserDto inviteAnswer = new AnswerInviteByUserDto
            {
                InviteId = 1,
                InvitedUsername = "testuser",
                IsAccepted = true
            };

            _mockInviteService.Setup(s => s.AnswerInviteByUser(inviteAnswer))
                            .Returns(Task.CompletedTask);

            IActionResult result = await _controller.AnswerInvite(inviteAnswer);
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("successfully", (okResult.Value as dynamic).Message.ToString());
        }

        [Fact]
        public async Task TestThatWhenAnsweringInviteFailsThenReturnsBadRequest()
        {
            AnswerInviteByUserDto inviteAnswer = new AnswerInviteByUserDto();
            string errorMessage = "Failed to process invite response";
            
            _mockInviteService.Setup(s => s.AnswerInviteByUser(inviteAnswer))
                            .ThrowsAsync(new Exception(errorMessage));

            IActionResult result = await _controller.AnswerInvite(inviteAnswer);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, (badRequest.Value as dynamic).Message);
        }

        [Fact]
        public async Task TestThatWhenInvitingUserByEmailThenReturnsSuccess()
        {
            InviteToGroupByEmailDto inviteDetails = new InviteToGroupByEmailDto
            {
                GroupId = 1,
                Email = "test@example.com",
                InviterUsername = "inviter"
            };

            _mockInviteService.Setup(s => s.InviteToGroupByEmail(inviteDetails))
                            .Returns(Task.CompletedTask);

            IActionResult result = await _controller.InviteToGroupByEmail(inviteDetails);
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("successfully", (okResult.Value as dynamic).Message.ToString());
        }

        [Fact]
        public async Task TestThatWhenInvitingByEmailFailsThenReturnsBadRequest()
        {
            InviteToGroupByEmailDto inviteDetails = new InviteToGroupByEmailDto();
            string errorMessage = "Failed to send invite";
            
            _mockInviteService.Setup(s => s.InviteToGroupByEmail(inviteDetails))
                            .ThrowsAsync(new Exception(errorMessage));

            IActionResult result = await _controller.InviteToGroupByEmail(inviteDetails);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, (badRequest.Value as dynamic).Message);
        }

        [Fact]
        public async Task TestThatWhenInvitingInvalidEmailThenReturnsBadRequest()
        {
            InviteToGroupByEmailDto inviteDetails = new InviteToGroupByEmailDto
            {
                GroupId = 1,
                Email = "",
                InviterUsername = "inviter"
            };

            _mockInviteService.Setup(s => s.InviteToGroupByEmail(inviteDetails))
                            .ThrowsAsync(new ArgumentException("Invalid email address"));

            IActionResult result = await _controller.InviteToGroupByEmail(inviteDetails);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid email", (badRequest.Value as dynamic).Message);
        }

        [Fact]
        public async Task TestThatWhenInvitingToNonexistentGroupThenReturnsBadRequest()
        {
            InviteToGroupByEmailDto inviteDetails = new InviteToGroupByEmailDto
            {
                GroupId = 999,
                Email = "test@example.com",
                InviterUsername = "inviter"
            };

            _mockInviteService.Setup(s => s.InviteToGroupByEmail(inviteDetails))
                            .ThrowsAsync(new Exception("Group not found"));

            IActionResult result = await _controller.InviteToGroupByEmail(inviteDetails);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Group not found", (badRequest.Value as dynamic).Message);
        }

        [Fact]
        public async Task TestThatWhenInviterNotInGroupThenReturnsBadRequest()
        {
            InviteToGroupByEmailDto inviteDetails = new InviteToGroupByEmailDto
            {
                GroupId = 1,
                Email = "test@example.com",
                InviterUsername = "nonmember"
            };

            _mockInviteService.Setup(s => s.InviteToGroupByEmail(inviteDetails))
                            .ThrowsAsync(new Exception("Inviter is not a member of the group"));

            IActionResult result = await _controller.InviteToGroupByEmail(inviteDetails);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("not a member", (badRequest.Value as dynamic).Message);
        }
    }
}
