using Microsoft.AspNetCore.Mvc;
using Moq;
using Roomiebill.Server.Controllers;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services;
using Roomiebill.Server.Services.Interfaces;
using Xunit;

namespace ServerTests
{
    public class PaymentControllerTests
    {
        private readonly Mock<IPaymentService> _mockPaymentService;
        private readonly Mock<GroupService> _mockGroupService;
        private readonly PaymentController _controller;

        public PaymentControllerTests()
        {
            _mockPaymentService = new Mock<IPaymentService>();
            _mockGroupService = new Mock<GroupService>();
            _controller = new PaymentController(_mockPaymentService.Object, _mockGroupService.Object);
        }

        [Fact]
        public async Task TestThatWhenProcessingValidPaymentThenReturnsSuccess()
        {
            User payer = new User { Username = "payer" };
            User payee = new User { Username = "payee" };
            PaymentRequest request = new PaymentRequest
            {
                Amount = 100.0M,
                PayerInfo = payer,
                PayeeInfo = payee,
                GroupId = 1
            };

            _mockPaymentService.Setup(s => s.ProcessPaymentAsync(request))
                             .ReturnsAsync(true);

            _mockGroupService.Setup(s => s.SettleDebtAsync(
                request.Amount, request.PayeeInfo, request.PayerInfo, request.GroupId))
                .Returns(Task.CompletedTask);

            IActionResult result = await _controller.ProcessPayment(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("successfully", (okResult.Value as dynamic).Message.ToString());
        }

        [Fact]
        public async Task TestThatWhenPaymentProcessingFailsThenReturnsBadRequest()
        {
            User payer = new User { Username = "payer" };
            User payee = new User { Username = "payee" };
            PaymentRequest request = new PaymentRequest
            {
                Amount = 100.0M,
                PayerInfo = payer,
                PayeeInfo = payee,
                GroupId = 1
            };

            _mockPaymentService.Setup(s => s.ProcessPaymentAsync(request))
                             .ReturnsAsync(false);

            IActionResult result = await _controller.ProcessPayment(request);

            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Payment failed.", (badRequest.Value as dynamic).Message);
        }

        [Fact]
        public async Task TestThatWhenPaymentSucceedsButDebtSettlementFailsThenReturnsSuccess()
        {
            User payer = new User { Username = "payer" };
            User payee = new User { Username = "payee" };
            PaymentRequest request = new PaymentRequest
            {
                Amount = 100.0M,
                PayerInfo = payer,
                PayeeInfo = payee,
                GroupId = 1
            };

            _mockPaymentService.Setup(s => s.ProcessPaymentAsync(request))
                             .ReturnsAsync(true);

            _mockGroupService.Setup(s => s.SettleDebtAsync(
                request.Amount, request.PayeeInfo, request.PayerInfo, request.GroupId))
                .ThrowsAsync(new Exception("Debt settlement failed"));

            IActionResult result = await _controller.ProcessPayment(request);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("successfully", (okResult.Value as dynamic).Message.ToString());
        }

        [Fact]
        public async Task TestThatWhenPaymentProcessingThrowsThenReturnsBadRequest()
        {
            PaymentRequest request = new PaymentRequest
            {
                Amount = 0,
                PayerInfo = new User(),
                PayeeInfo = new User()
            };
            string errorMessage = "Payment processing error";

            _mockPaymentService.Setup(s => s.ProcessPaymentAsync(request))
                             .ThrowsAsync(new Exception(errorMessage));

            IActionResult result = await _controller.ProcessPayment(request);

            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Payment failed.", (badRequest.Value as dynamic).Message);
        }
    }
}
