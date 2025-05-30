using Roomiebill.Server.Common.Validators;

namespace ServerTests.UnitTests

{
    public class PasswordValidatorTests
    {
        [Fact]
        public void ValidatePassword_ValidPassword_ReturnsTrue()
        {
            // Arrange
            var validator = new PasswordValidator();
            var password = "Valid1@Password";

            // Act
            var result = validator.ValidatePassword(password);

            // Assert
            Assert.True(result);
            Assert.Equal("", validator.Error);
        }

        [Fact]
        public void ValidatePassword_TooShort_ReturnsFalse()
        {
            // Arrange
            var validator = new PasswordValidator();
            var password = "Short1@";

            // Act
            var result = validator.ValidatePassword(password);

            // Assert
            Assert.False(result);
            Assert.Equal("Password must be at least 8 characters long", validator.Error);
        }

        [Fact]
        public void ValidatePassword_NoUppercase_ReturnsFalse()
        {
            // Arrange
            var validator = new PasswordValidator();
            var password = "lowercase1@";

            // Act
            var result = validator.ValidatePassword(password);

            // Assert
            Assert.False(result);
            Assert.Equal("Password does not contain an uppercase letter", validator.Error);
        }

        [Fact]
        public void ValidatePassword_NoLowercase_ReturnsFalse()
        {
            // Arrange
            var validator = new PasswordValidator();
            var password = "UPPERCASE1@";

            // Act
            var result = validator.ValidatePassword(password);

            // Assert
            Assert.False(result);
            Assert.Equal("Password does not contain a lowercase letter", validator.Error);
        }

        [Fact]
        public void ValidatePassword_NoNumber_ReturnsFalse()
        {
            // Arrange
            var validator = new PasswordValidator();
            var password = "NoNumber@";

            // Act
            var result = validator.ValidatePassword(password);

            // Assert
            Assert.False(result);
            Assert.Equal("Password does not contain a number", validator.Error);
        }

        [Fact]
        public void ValidatePassword_NoSpecialCharacter_ReturnsFalse()
        {
            // Arrange
            var validator = new PasswordValidator();
            var password = "NoSpecial1";

            // Act
            var result = validator.ValidatePassword(password);

            // Assert
            Assert.False(result);
            Assert.Equal("Password does not contain a special character", validator.Error);
        }
    }
}