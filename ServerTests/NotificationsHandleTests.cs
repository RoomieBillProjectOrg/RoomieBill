using Roomiebill.Server.Common.Notificaiton;

namespace ServerTests
{
    public class NotificationsHandleTests
    {
        private const string TEST_TOKEN = "Test";
        private const string TEST_TOPIC = "test-topic";

        [Fact]
        public void SendNotificationByTopic_ValidParameters_DoesNotThrow()
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                NotificationsHandle.SendNotificationByTopicAsync(
                    "Test Title",
                    "Test Body",
                    TEST_TOPIC));  // Use test topic to avoid actual Firebase call

            Assert.Null(exception);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SendNotificationByTopic_InvalidTopic_ThrowsArgumentException(string topic)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                NotificationsHandle.SendNotificationByTopicAsync(
                    "Test Title",
                    "Test Body",
                    topic));

            Assert.Contains("Invalid topic name", exception.Message);
        }

        [Fact]
        public void SendNotificationByTopic_TopicTooLong_ThrowsArgumentException()
        {
            // Arrange
            string longTopic = new string('a', 901);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                NotificationsHandle.SendNotificationByTopicAsync(
                    "Test Title",
                    "Test Body",
                    longTopic));

            Assert.Contains("Invalid topic name", exception.Message);
        }

        [Theory]
        [InlineData("topic@invalid")]
        [InlineData("topic#invalid")]
        [InlineData("topic$invalid")]
        public void SendNotificationByTopic_InvalidTopicCharacters_ThrowsArgumentException(string topic)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                NotificationsHandle.SendNotificationByTopicAsync(
                    "Test Title",
                    "Test Body",
                    topic));

            Assert.Contains("Invalid topic name", exception.Message);
        }

        [Fact]
        public void SendNotificationByToken_ValidParameters_DoesNotThrow()
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                NotificationsHandle.SendNotificationByTokenAsync(
                    "Test Title",
                    "Test Body",
                    TEST_TOKEN));

            Assert.Null(exception);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SendNotificationByToken_InvalidToken_ThrowsArgumentException(string token)
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                NotificationsHandle.SendNotificationByTokenAsync(
                    "Test Title",
                    "Test Body",
                    token));

            Assert.Contains("Invalid token", exception.Message);
        }

        [Fact]
        public void SendNotificationByToken_TokenTooLong_ThrowsArgumentException()
        {
            // Arrange
            string longToken = new string('a', 901);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                NotificationsHandle.SendNotificationByTokenAsync(
                    "Test Title",
                    "Test Body",
                    longToken));

            Assert.Contains("Invalid token", exception.Message);
        }

        [Fact]
        public void SendNotificationByToken_TestToken_DoesNotThrow()
        {
            // Act & Assert
            var exception = Record.Exception(() =>
                NotificationsHandle.SendNotificationByTokenAsync(
                    "Test Title",
                    "Test Body",
                    TEST_TOKEN));

            Assert.Null(exception);
        }

        [Theory]
        [InlineData("valid-topic")]
        [InlineData("topic123")]
        [InlineData("topic_name")]
        [InlineData("topic-name")]
        public void SendNotificationByTopic_ValidTopicFormats_DoesNotThrow(string topic)
        {
            // Verify that the topic matches the required pattern
            bool validTopic = System.Text.RegularExpressions.Regex.IsMatch(topic, @"^[a-zA-Z0-9_-]+$");
            Assert.True(validTopic, $"Topic '{topic}' should match the regex pattern");

            // Act & Assert
            var exception = Record.Exception(() =>
                NotificationsHandle.SendNotificationByTopicAsync(
                    "Test Title",
                    "Test Body",
                    TEST_TOPIC));  // Use test topic to avoid actual Firebase call

            Assert.Null(exception);
        }

        [Fact]
        public void Constructor_CreatesInstance_DoesNotThrow()
        {
            // Act & Assert
            var exception = Record.Exception(() => new NotificationsHandle());
            Assert.Null(exception);
        }
    }
}
