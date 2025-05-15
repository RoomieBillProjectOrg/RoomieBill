using Xunit;
using System;
using System.Threading.Tasks;
using Roomiebill.Server.Common.Validators;
using MimeKit;
using Moq;

namespace ServerTests
{
    public class RegisterVerifyTests : IDisposable
    {
        private readonly Mock<IEmailService> _mockEmailService;

        public RegisterVerifyTests()
        {
            _mockEmailService = new Mock<IEmailService>();
            RegisterVerify.EmailService = _mockEmailService.Object;
        }

        public void Dispose()
        {
            RegisterVerify.EmailService = new SmtpEmailService();
        }

        [Fact]
        public void GenerateVerificationCode_ReturnsValidCode()
        {
            // Act
            string code = RegisterVerify.GenerateVerificationCode();

            // Assert
            Assert.NotNull(code);
            Assert.Equal(6, code.Length);
            Assert.True(int.TryParse(code, out int numericCode));
            Assert.True(numericCode >= 100000 && numericCode <= 999999);
        }

        [Theory]
        [InlineData("123456", true)]
        [InlineData("999999", true)]
        [InlineData("100000", true)]
        [InlineData("12345", false)]  // Too short
        [InlineData("1234567", false)]  // Too long
        [InlineData("abcdef", false)]  // Non-numeric
        [InlineData("12345a", false)]  // Contains letters
        [InlineData("", false)]  // Empty
        [InlineData(null, false)]  // Null
        [InlineData("099999", false)]  // Starts with 0
        [InlineData(" 12345", false)]  // Contains whitespace
        public void ValidateVerificationCode_ReturnsExpectedResult(string code, bool expectedResult)
        {
            // Act
            bool result = RegisterVerify.ValidateVerificationCode(code);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task SendVerificationEmail_ValidEmail_ReturnsVerificationCode()
        {
            // Arrange
            string validEmail = "test@bgu.ac.il";
            _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<MimeMessage>()))
                           .Returns(Task.CompletedTask);

            // Act
            var result = await RegisterVerify.SendVerificationEmail(validEmail);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.VerifyCode);
            Assert.True(RegisterVerify.ValidateVerificationCode(result.VerifyCode));
            _mockEmailService.Verify(x => x.SendEmailAsync(It.Is<MimeMessage>(m => 
                m.To.ToString() == validEmail)), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SendVerificationEmail_InvalidEmail_ThrowsArgumentNullException(string invalidEmail)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                RegisterVerify.SendVerificationEmail(invalidEmail));
            Assert.Equal("Recipient email address cannot be empty. (Parameter 'recipientEmail')", exception.Message);
        }

        [Fact]
        public void GenerateVerificationCode_MultipleCalls_ReturnsUniqueValues()
        {
            // Arrange
            const int numberOfCodes = 1000;
            var codes = new HashSet<string>();

            // Act
            for (int i = 0; i < numberOfCodes; i++)
            {
                string code = RegisterVerify.GenerateVerificationCode();
                codes.Add(code);
            }

            // Assert
            // If all codes were unique, the HashSet size should equal the number of generated codes
            Assert.Equal(numberOfCodes, codes.Count);
            
            // Verify all codes are valid
            foreach (string code in codes)
            {
                Assert.True(RegisterVerify.ValidateVerificationCode(code));
            }
        }

        [Fact]
        public void ValidateVerificationCode_ValidCodeRange_ReturnsTrue()
        {
            // Test the full range of valid codes
            Assert.True(RegisterVerify.ValidateVerificationCode("100000")); // Minimum valid code
            Assert.True(RegisterVerify.ValidateVerificationCode("500000")); // Middle of range
            Assert.True(RegisterVerify.ValidateVerificationCode("999999")); // Maximum valid code
        }

        [Theory]
        [InlineData("099999")] // Leading zero
        [InlineData("1000000")] // Too large
        [InlineData("099999")] // Too small with leading zero
        public void ValidateVerificationCode_InvalidRanges_ReturnsFalse(string code)
        {
            // Act & Assert
            Assert.False(RegisterVerify.ValidateVerificationCode(code));
        }

        [Theory]
        [InlineData("123 456")] // Space in middle
        [InlineData(" 123456")] // Leading space
        [InlineData("123456 ")] // Trailing space
        [InlineData("\t123456")] // Tab
        [InlineData("123456\n")] // Newline
        public void ValidateVerificationCode_ContainsWhitespace_ReturnsFalse(string code)
        {
            // Act & Assert
            Assert.False(RegisterVerify.ValidateVerificationCode(code));
        }

        [Fact]
        public async Task SendVerificationEmail_SmtpError_ThrowsException()
        {
            // Arrange
            string validEmail = "test@bgu.ac.il";
            _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<MimeMessage>()))
                           .ThrowsAsync(new Exception("SMTP error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                RegisterVerify.SendVerificationEmail(validEmail));
            Assert.Contains("Failed to send verification email", exception.Message);
            Assert.Contains("SMTP error", exception.Message);
        }

        [Fact]
        public async Task SendVerificationEmail_VerifiesEmailContent()
        {
            // Arrange
            string validEmail = "test@bgu.ac.il";
            MimeMessage capturedMessage = null;
            _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<MimeMessage>()))
                           .Callback<MimeMessage>(msg => capturedMessage = msg)
                           .Returns(Task.CompletedTask);

            // Act
            var result = await RegisterVerify.SendVerificationEmail(validEmail);

            // Assert
            Assert.NotNull(capturedMessage);
            Assert.Equal("Verify Your Email", capturedMessage.Subject);
            Assert.Contains(result.VerifyCode, ((TextPart)capturedMessage.Body).Text);
            Assert.Equal("RoomieBill register verify", capturedMessage.From.ToString());
            Assert.Equal(validEmail, capturedMessage.To.ToString());
        }
    }
}
