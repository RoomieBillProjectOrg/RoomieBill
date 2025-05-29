using Moq;
using Moq.Protected;
using Microsoft.Extensions.Configuration;
using Roomiebill.Server.Common;
using System.Net;
using System.Text.Json;

namespace ServerTests.UnitTests
{
    public class GeminiServiceTests
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly GeminiService _service;

        public GeminiServiceTests()
        {
            _configMock = new Mock<IConfiguration>();
            _handlerMock = new Mock<HttpMessageHandler>();

            // Create HttpClient with mocked handler
            var client = new HttpClient(_handlerMock.Object);

            // Use reflection to set the private HttpClient field
            var field = typeof(GeminiService).GetField("_httpClient",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            _service = new GeminiService(_configMock.Object);
            field.SetValue(_service, client);
        }

        [Fact]
        public async Task GetFeedbackFromGeminiAsync_ValidPrompt_ReturnsResponse()
        {
            // Arrange
            var expectedResponse = "Generated response from Gemini";
            var responseJson = JsonSerializer.Serialize(new
            {
                candidates = new[]
                {
                    new
                    {
                        content = new
                        {
                            parts = new[]
                            {
                                new { text = expectedResponse }
                            }
                        }
                    }
                }
            });

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson)
                });

            // Act
            var result = await _service.GetFeedbackFromGeminiAsync("Test prompt");

            // Assert
            Assert.Equal(expectedResponse, result);
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains("generativelanguage.googleapis.com")),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetFeedbackFromGeminiAsync_ApiError_ThrowsException()
        {
            // Arrange
            var errorResponse = "API Error Message";
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent(errorResponse)
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _service.GetFeedbackFromGeminiAsync("Test prompt"));
            Assert.Equal($"Gemini API failed: {errorResponse}", exception.Message);
        }

        [Fact]
        public async Task GetFeedbackFromGeminiAsync_NullPrompt_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.GetFeedbackFromGeminiAsync(null));
            Assert.Equal("Prompt cannot be empty. (Parameter 'prompt')", exception.Message);
        }

        [Fact]
        public async Task GetFeedbackFromGeminiAsync_EmptyPrompt_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.GetFeedbackFromGeminiAsync(""));
            Assert.Equal("Prompt cannot be empty. (Parameter 'prompt')", exception.Message);
        }

        [Fact]
        public async Task GetFeedbackFromGeminiAsync_WhitespacePrompt_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.GetFeedbackFromGeminiAsync("   "));
            Assert.Equal("Prompt cannot be empty. (Parameter 'prompt')", exception.Message);
        }

        [Fact]
        public async Task GetFeedbackFromGeminiAsync_NullResponse_ReturnsEmptyString()
        {
            // Arrange
            var responseJson = JsonSerializer.Serialize(new
            {
                candidates = new[]
                {
                    new
                    {
                        content = new
                        {
                            parts = new[]
                            {
                                new { text = (string)null }
                            }
                        }
                    }
                }
            });

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson)
                });

            // Act
            var result = await _service.GetFeedbackFromGeminiAsync("Test prompt");

            // Assert
            Assert.Equal(string.Empty, result);
        }
    }
}
