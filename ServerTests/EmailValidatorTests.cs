using Roomiebill.Server.Common.Validators;

namespace ServerTests
{
    public class EmailValidatorTests
    {
        [Fact]
        public void ValidateEmail_ValidBGUEmail_ReturnsTrue()
        {
            // Arrange
            var validator = new EmailValidator();
            var email = "user@bgu.ac.il";

            // Act
            var result = validator.ValidateEmail(email);

            // Assert
            Assert.True(result);
            Assert.Equal("", validator.Error);
        }

        [Fact]
        public void ValidateEmail_ValidPostBGUEmail_ReturnsTrue()
        {
            // Arrange
            var validator = new EmailValidator();
            var email = "user@post.bgu.ac.il";

            // Act
            var result = validator.ValidateEmail(email);

            // Assert
            Assert.True(result);
            Assert.Equal("", validator.Error);
        }

        [Fact]
        public void ValidateEmail_InvalidEmail_ReturnsFalse()
        {
            // Arrange
            var validator = new EmailValidator();
            var email = "user@example.com";

            // Act
            var result = validator.ValidateEmail(email);

            // Assert
            Assert.False(result);
            Assert.Equal("Email should be a BGU University email", validator.Error);
        }
    }
}