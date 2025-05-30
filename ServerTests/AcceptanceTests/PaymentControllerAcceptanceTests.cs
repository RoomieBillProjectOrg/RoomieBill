using Microsoft.AspNetCore.Mvc;
using Moq;
using Roomiebill.Server.Controllers;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services.Interfaces;
using Xunit;

namespace ServerTests.AcceptanceTests
{
    public class PaymentControllerAcceptanceTests
    {
        private readonly User testPayer;
        private readonly User testPayee;

        public PaymentControllerAcceptanceTests()
        {
            testPayer = new User
            {
                Id = 1,
                Username = "payer123",
                Email = "payer@test.com",
                BitLink = "bit.ly/payer123"
            };

            testPayee = new User
            {
                Id = 2,
                Username = "payee123",
                Email = "payee@test.com",
                BitLink = "bit.ly/payee123"
            };
        }

        [Fact]
        public async Task ProcessPayment_WithValidData_ShouldSucceed()
        {
            // Arrange
            var request = new PaymentRequest
            {
                Amount = 100.00m,
                Currency = "USD",
                PayerInfo = testPayer,
                PayeeInfo = testPayee,
                PaymentMethod = "BitPay",
                GroupId = 1
            };

            var controller = CreateController(
                paymentSuccess: true,
                debtSettlementSuccess: true);

            // Act
            var result = await controller.ProcessPayment(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaymentResponse>(okResult.Value);
            Assert.Equal("Payment processed successfully.", response.Message);
        }

        [Fact]
        public async Task ProcessPayment_WithPaymentFailure_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new PaymentRequest
            {
                Amount = 100.00m,
                Currency = "USD",
                PayerInfo = testPayer,
                PayeeInfo = testPayee,
                PaymentMethod = "BitPay",
                GroupId = 1
            };

            var controller = CreateController(
                paymentSuccess: false,
                debtSettlementSuccess: true);

            // Act
            var result = await controller.ProcessPayment(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<PaymentResponse>(badRequestResult.Value);
            Assert.Equal("Payment failed.", response.Message);
        }

        [Fact]
        public async Task ProcessPayment_WithDebtSettlementFailure_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new PaymentRequest
            {
                Amount = 100.00m,
                Currency = "USD",
                PayerInfo = testPayer,
                PayeeInfo = testPayee,
                PaymentMethod = "BitPay",
                GroupId = 1
            };

            var controller = CreateController(
                paymentSuccess: true,
                debtSettlementSuccess: false);

            // Act
            var result = await controller.ProcessPayment(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<PaymentResponse>(badRequestResult.Value);
            Assert.Equal("Payment failed: Unable to settle debt.", response.Message);
        }

        [Fact]
        public async Task ProcessPayment_WithInvalidAmount_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new PaymentRequest
            {
                Amount = -50.00m,
                Currency = "USD",
                PayerInfo = testPayer,
                PayeeInfo = testPayee,
                PaymentMethod = "BitPay",
                GroupId = 1
            };

            var controller = CreateController(
                paymentSuccess: false,
                debtSettlementSuccess: true);

            // Act
            var result = await controller.ProcessPayment(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<PaymentResponse>(badRequestResult.Value);
            Assert.Equal("Payment failed.", response.Message);
        }

        [Fact]
        public async Task ProcessPayment_WithNullPayerInfo_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new PaymentRequest
            {
                Amount = 100.00m,
                Currency = "USD",
                PayerInfo = null,
                PayeeInfo = testPayee,
                PaymentMethod = "BitPay",
                GroupId = 1
            };

            var controller = CreateController(
                paymentSuccess: false,
                debtSettlementSuccess: true);

            // Act
            var result = await controller.ProcessPayment(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<PaymentResponse>(badRequestResult.Value);
            Assert.Equal("Payment failed.", response.Message);
        }

        [Fact]
        public async Task ProcessPayment_WithNullRequest_ShouldReturnBadRequest()
        {
            // Arrange
            PaymentRequest request = null;
            var controller = CreateController(paymentSuccess: false, debtSettlementSuccess: true);

            // Act
            var result = await controller.ProcessPayment(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<PaymentResponse>(badRequestResult.Value);
            Assert.Contains("failed", response.Message.ToLower());
        }

        [Fact]
        public async Task ProcessPayment_WhenPaymentServiceThrows_ShouldReturnBadRequest()
        {
            // Arrange
            var paymentService = new Mock<IPaymentService>();
            var groupService = new Mock<IGroupService>();
            paymentService.Setup(s => s.ProcessPaymentAsync(It.IsAny<PaymentRequest>()))
                .ThrowsAsync(new Exception("Payment service error"));
            var controller = new PaymentController(paymentService.Object, groupService.Object);

            var request = new PaymentRequest
            {
                Amount = 100.00m,
                Currency = "USD",
                PayerInfo = testPayer,
                PayeeInfo = testPayee,
                PaymentMethod = "BitPay",
                GroupId = 1
            };

            // Act
            var result = await controller.ProcessPayment(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<PaymentResponse>(badRequestResult.Value);
            Assert.Equal("Payment failed.", response.Message);
        }

        [Fact]
        public async Task ProcessPayment_WhenSettleDebtThrows_ShouldReturnBadRequest()
        {
            // Arrange
            var paymentService = new Mock<IPaymentService>();
            var groupService = new Mock<IGroupService>();
            paymentService.Setup(s => s.ProcessPaymentAsync(It.IsAny<PaymentRequest>()))
                .ReturnsAsync(true);
            groupService.Setup(s => s.SettleDebtAsync(
                    It.IsAny<decimal>(),
                    It.IsAny<User>(),
                    It.IsAny<User>(),
                    It.IsAny<int>()))
                .ThrowsAsync(new Exception("Settle debt error"));
            var controller = new PaymentController(paymentService.Object, groupService.Object);

            var request = new PaymentRequest
            {
                Amount = 100.00m,
                Currency = "USD",
                PayerInfo = testPayer,
                PayeeInfo = testPayee,
                PaymentMethod = "BitPay",
                GroupId = 1
            };

            // Act
            var result = await controller.ProcessPayment(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<PaymentResponse>(badRequestResult.Value);
            Assert.Equal("Payment failed: Unable to settle debt.", response.Message);
        }

        [Fact]
        public async Task ProcessPayment_WithNullPayeeInfo_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new PaymentRequest
            {
                Amount = 100.00m,
                Currency = "USD",
                PayerInfo = testPayer,
                PayeeInfo = null,
                PaymentMethod = "BitPay",
                GroupId = 1
            };

            var controller = CreateController(
                paymentSuccess: false,
                debtSettlementSuccess: true);

            // Act
            var result = await controller.ProcessPayment(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<PaymentResponse>(badRequestResult.Value);
            Assert.Equal("Payment failed.", response.Message);
        }

        [Fact]
        public async Task ProcessPayment_WithNullCurrency_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new PaymentRequest
            {
                Amount = 100.00m,
                Currency = null,
                PayerInfo = testPayer,
                PayeeInfo = testPayee,
                PaymentMethod = "BitPay",
                GroupId = 1
            };

            var controller = CreateController(
                paymentSuccess: false,
                debtSettlementSuccess: true);

            // Act
            var result = await controller.ProcessPayment(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<PaymentResponse>(badRequestResult.Value);
            Assert.Equal("Payment failed.", response.Message);
        }

        [Fact]
        public async Task ProcessPayment_WithNullPaymentMethod_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new PaymentRequest
            {
                Amount = 100.00m,
                Currency = "USD",
                PayerInfo = testPayer,
                PayeeInfo = testPayee,
                PaymentMethod = null,
                GroupId = 1
            };

            var controller = CreateController(
                paymentSuccess: false,
                debtSettlementSuccess: true);

            // Act
            var result = await controller.ProcessPayment(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<PaymentResponse>(badRequestResult.Value);
            Assert.Equal("Payment failed.", response.Message);
        }

        private PaymentController CreateController(bool paymentSuccess, bool debtSettlementSuccess)
        {
            var paymentService = new Mock<IPaymentService>();
            var groupService = new Mock<IGroupService>();

            // Configure payment service
            paymentService.Setup(s => s.ProcessPaymentAsync(It.IsAny<PaymentRequest>()))
                .ReturnsAsync(paymentSuccess);

            // Configure group service
            if (debtSettlementSuccess)
            {
                groupService.Setup(s => s.SettleDebtAsync(
                    It.IsAny<decimal>(),
                    It.IsAny<User>(),
                    It.IsAny<User>(),
                    It.IsAny<int>()))
                    .Returns(Task.CompletedTask);
            }
            else
            {
                groupService.Setup(s => s.SettleDebtAsync(
                    It.IsAny<decimal>(),
                    It.IsAny<User>(),
                    It.IsAny<User>(),
                    It.IsAny<int>()))
                    .ThrowsAsync(new Exception("Debt settlement failed"));
            }

            return new PaymentController(paymentService.Object, groupService.Object);
        }
    }
}
