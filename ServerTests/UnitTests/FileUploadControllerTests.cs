using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Roomiebill.Server.Controllers;
using Roomiebill.Server.Services;
using Roomiebill.Server.Services.Interfaces;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using Xunit;

namespace ServerTests.UnitTests
{
    /// <summary>
    /// Tests for the file upload and download functionality.
    /// </summary>
    public class FileUploadControllerTests
    {
        private readonly Mock<IFileUploadService> _mockFileService;
        private readonly UploadController _controller;

        public FileUploadControllerTests()
        {
            _mockFileService = new Mock<IFileUploadService>();
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

            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultDict = JsonSerializer.Deserialize<Dictionary<string, string>>(
                JsonSerializer.Serialize(okResult.Value));
            Assert.Equal(expectedFileName, resultDict["FileName"]);
        }

        /// <summary>
        /// - Tests null file upload handling.
        /// - Verifies appropriate error response.
        /// </summary>
        [Fact]
        public async Task TestThatWhenNullFileThenReturnsBadRequest()
        {
            IActionResult result = await _controller.UploadReceipt(null);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Upload failed: File is null", badRequest.Value);
        }

        [Fact]
        public async Task TestThatWhenEmptyFileThenReturnsBadRequest()
        {
            var emptyFile = new FormFile(Stream.Null, 0, 0, null, "test.jpg");

            IActionResult result = await _controller.UploadReceipt(emptyFile);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Upload failed: File is empty", badRequest.Value);
        }

        [Fact]
        public async Task TestThatWhenUploadFailsThenReturnsBadRequest()
        {
            IFormFile file = new FormFile(Stream.Null, 0, 0, "test", "test.jpg");
            _mockFileService.Setup(s => s.SaveFileAsync(It.IsAny<IFormFile>()))
                          .ThrowsAsync(new Exception("Upload failed"));

            IActionResult result = await _controller.UploadReceipt(file);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Upload failed: File is empty", badRequest.Value);
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

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("File not found.", notFound.Value);
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

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal($"Error downloading file: {errorMessage}", badRequest.Value);
        }
    }
}
