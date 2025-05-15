using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Roomiebill.Server.Controllers;
using Roomiebill.Server.Services;
using System.IO;
using System.Text;
using Xunit;

namespace ServerTests
{
    /// <summary>
    /// Tests for the file upload and download functionality.
    /// </summary>
    public class FileUploadControllerTests
    {
        private readonly Mock<FileUploadService> _mockFileService;
        private readonly UploadController _controller;

        public FileUploadControllerTests()
        {
            _mockFileService = new Mock<FileUploadService>();
            _controller = new UploadController(_mockFileService.Object);
        }

        /// <summary>
        /// - Tests successful receipt file upload.
        /// - Verifies the file is saved and a unique filename is returned.
        /// </summary>
        [Fact]
        public async Task TestThatWhenFileUploadedThenReturnsUniqueFileName()
        {
            // Create a mock file using a memory stream
            string fileName = "test.jpg";
            string content = "test file content";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            IFormFile file = new FormFile(stream, 0, stream.Length, "test", fileName);

            string expectedFileName = "unique_test.jpg";
            _mockFileService.Setup(s => s.SaveFileAsync(It.IsAny<IFormFile>()))
                          .ReturnsAsync(expectedFileName);

            IActionResult result = await _controller.UploadReceipt(file);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            dynamic resultValue = okResult.Value;
            Assert.Equal(expectedFileName, resultValue.FileName.ToString());
        }

        /// <summary>
        /// - Tests file upload failure handling.
        /// - Verifies appropriate error response when service throws exception.
        /// </summary>
        [Fact]
        public async Task TestThatWhenUploadFailsThenReturnsBadRequest()
        {
            IFormFile file = new FormFile(Stream.Null, 0, 0, "test", "test.jpg");
            _mockFileService.Setup(s => s.SaveFileAsync(It.IsAny<IFormFile>()))
                          .ThrowsAsync(new Exception("Upload failed"));

            IActionResult result = await _controller.UploadReceipt(file);

            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            dynamic errorValue = badRequest.Value;
            Assert.Contains("Upload failed", errorValue.Message.ToString());
        }

        /// <summary>
        /// - Tests successful file download.
        /// - Verifies correct content type and file content.
        /// </summary>
        [Fact]
        public async Task TestThatWhenFileDownloadedThenReturnsFileContent()
        {
            string fileName = "test.jpg";
            byte[] fileContent = Encoding.UTF8.GetBytes("test content");
            _mockFileService.Setup(s => s.GetFileAsync(fileName))
                          .ReturnsAsync(fileContent);

            IActionResult result = await _controller.DownloadReceipt(fileName);

            FileContentResult fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("image/jpeg", fileResult.ContentType);
            Assert.Equal(fileContent, fileResult.FileContents);
        }

        /// <summary>
        /// - Tests file download when file not found.
        /// - Verifies NotFound response.
        /// </summary>
        [Fact]
        public async Task TestThatWhenFileNotFoundThenReturnsNotFound()
        {
            string fileName = "nonexistent.jpg";
            _mockFileService.Setup(s => s.GetFileAsync(fileName))
                          .ReturnsAsync((byte[])null);

            IActionResult result = await _controller.DownloadReceipt(fileName);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        /// <summary>
        /// - Tests download with various file extensions.
        /// - Verifies correct content type for different file types.
        /// </summary>
        [Theory]
        [InlineData("test.jpg", "image/jpeg")]
        [InlineData("test.jpeg", "image/jpeg")]
        [InlineData("test.png", "image/png")]
        [InlineData("test.pdf", "application/pdf")]
        [InlineData("test.txt", "application/octet-stream")]
        public async Task TestThatWhenDifferentFileTypesThenReturnsCorrectContentType(
            string fileName, string expectedContentType)
        {
            byte[] fileContent = new byte[] { 1, 2, 3 };
            _mockFileService.Setup(s => s.GetFileAsync(fileName))
                          .ReturnsAsync(fileContent);

            IActionResult result = await _controller.DownloadReceipt(fileName);

            FileContentResult fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal(expectedContentType, fileResult.ContentType);
        }

        /// <summary>
        /// - Tests download error handling.
        /// - Verifies BadRequest response with error message.
        /// </summary>
        [Fact]
        public async Task TestThatWhenDownloadFailsThenReturnsBadRequest()
        {
            string fileName = "test.jpg";
            string errorMessage = "File access error";
            _mockFileService.Setup(s => s.GetFileAsync(fileName))
                          .ThrowsAsync(new Exception(errorMessage));

            IActionResult result = await _controller.DownloadReceipt(fileName);

            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(result);
            dynamic errorValue = badRequest.Value;
            Assert.Contains(errorMessage, errorValue.Message.ToString());
        }
    }
}
