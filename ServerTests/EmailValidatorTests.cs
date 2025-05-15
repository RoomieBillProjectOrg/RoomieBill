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

        [Fact]
        public void TestThatWhenEmailIsNullThenReturnsFalseWithError()
        {
            // Arrange
            var validator = new EmailValidator();
            string email = null;

            // Act
            var result = validator.ValidateEmail(email);

            // Assert
            Assert.False(result);
            Assert.Equal("Email should be a BGU University email", validator.Error);
        }

        [Fact]
        public void TestThatWhenEmailIsEmptyThenReturnsFalseWithError()
        {
            // Arrange
            var validator = new EmailValidator();
            var email = "";

            // Act
            var result = validator.ValidateEmail(email);

            // Assert
            Assert.False(result);
            Assert.Equal("Email should be a BGU University email", validator.Error);
        }

        [Fact]
        public void TestThatWhenEmailHasMixedCaseDomainThenReturnsTrue()
        {
            // Arrange
            var validator = new EmailValidator();
            var email = "user@BGU.AC.IL";

            // Act
            var result = validator.ValidateEmail(email);

            // Assert
            Assert.True(result);
            Assert.Equal("", validator.Error);
        }

        [Fact]
        public void TestThatWhenEmailContainsSpacesThenReturnsFalseWithError()
        {
            // Arrange
            var validator = new EmailValidator();
            var email = "user @ bgu.ac.il";

            // Act
            var result = validator.ValidateEmail(email);

            // Assert
            Assert.False(result);
            Assert.Equal("Email should be a BGU University email", validator.Error);
        }

        [Fact]
        public void TestThatWhenEmailHasInvalidSubdomainThenReturnsFalseWithError()
        {
            // Arrange
            var validator = new EmailValidator();
            var email = "user@invalid.bgu.ac.il";

            // Act
            var result = validator.ValidateEmail(email);

            // Assert
            Assert.False(result);
            Assert.Equal("Email should be a BGU University email", validator.Error);
        }

        [Fact]
        public void TestThatWhenEmailHasNoAtSymbolThenReturnsFalseWithError()
        {
            // Arrange
            var validator = new EmailValidator();
            var email = "user.bgu.ac.il";

            // Act
            var result = validator.ValidateEmail(email);

            // Assert
            Assert.False(result);
            Assert.Equal("Email should be a BGU University email", validator.Error);
        }

        [Fact]
        public void TestThatWhenEmailHasSpecialCharactersInDomainThenReturnsFalseWithError()
        {
            // Arrange
            var validator = new EmailValidator();
            var email = "user@bgu!.ac.il";

            // Act
            var result = validator.ValidateEmail(email);

            // Assert
            Assert.False(result);
            Assert.Equal("Email should be a BGU University email", validator.Error);
        }

        [Fact]
        public void TestThatWhenEmailHasHyphenInLocalPartThenReturnsTrue()
        {
            // Arrange
            var validator = new EmailValidator();
            var email = "user-name@bgu.ac.il";

            // Act
            var result = validator.ValidateEmail(email);

            // Assert
            Assert.True(result);
            Assert.Equal("", validator.Error);
        }

        [Fact]
        public void TestThatWhenEmailHasNumbersInLocalPartThenReturnsTrue()
        {
            // Arrange
            var validator = new EmailValidator();
            var email = "user123@bgu.ac.il";

            // Act
            var result = validator.ValidateEmail(email);

            // Assert
            Assert.True(result);
            Assert.Equal("", validator.Error);
        }
    }
}
