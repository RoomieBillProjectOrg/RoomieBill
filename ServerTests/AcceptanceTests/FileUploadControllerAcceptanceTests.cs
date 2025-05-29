using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Roomiebill.Server.Controllers;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services.Interfaces;
using System.Text;
using Xunit;

namespace ServerTests.AcceptanceTests
{
    public class FileUploadControllerAcceptanceTests
    {
        private readonly Mock<IFileUploadService> _mockFileService;
        private readonly UploadController _controller;

        public FileUploadControllerAcceptanceTests()
        {
            _mockFileService = new Mock<IFileUploadService>();
            _controller = new UploadController(_mockFileService.Object);
        }

        [Fact]
        public async Task UploadReceipt_WithValidFile_ShouldSucceed()
        {
            // Arrange
            var fileName = "receipt.jpg";
            var content = "Fake file content";
            var file = CreateTestFormFile(fileName, content);

            _mockFileService
                .Setup(s => s.SaveFileAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync($"unique-guid-{fileName}");

            // Act
            var result = await _controller.UploadReceipt(file);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<FileUploadResponse>(okResult.Value);
            Assert.Contains(fileName, response.FileName);
        }

        [Fact]
        public async Task UploadReceipt_WithNullFile_ShouldReturnBadRequest()
        {
            // Arrange
            IFormFile file = null;

            // Act
            var result = await _controller.UploadReceipt(file);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Upload failed: File is null", badRequestResult.Value);
        }

        [Fact]
        public async Task UploadReceipt_WithEmptyFile_ShouldReturnBadRequest()
        {
            // Arrange
            var file = CreateTestFormFile("empty.jpg", "");

            // Act
            var result = await _controller.UploadReceipt(file);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Upload failed: File is empty", badRequestResult.Value);
        }

        [Fact]
        public async Task DownloadReceipt_WithValidFile_ShouldReturnFile()
        {
            // Arrange
            var fileName = "receipt.jpg";
            var fileContent = new byte[] { 1, 2, 3, 4, 5 };
            
            _mockFileService
                .Setup(s => s.GetFileAsync(fileName))
                .ReturnsAsync(fileContent);

            // Act
            var result = await _controller.DownloadReceipt(fileName);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("image/jpeg", fileResult.ContentType);
            Assert.Equal(fileContent, fileResult.FileContents);
            Assert.Equal(fileName, fileResult.FileDownloadName);
        }

        [Fact]
        public async Task DownloadReceipt_WithNonexistentFile_ShouldReturnNotFound()
        {
            // Arrange
            var fileName = "nonexistent.jpg";
            
            _mockFileService
                .Setup(s => s.GetFileAsync(fileName))
                .ReturnsAsync((byte[])null);

            // Act
            var result = await _controller.DownloadReceipt(fileName);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("File not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task ExtractData_WithValidFile_ShouldReturnBillData()
        {
            // Arrange
            var fileName = "receipt.jpg";
            var billData = new BillData 
            { 
                TotalPrice = 100.50m,
                StartDate = DateTime.Now,
                Description = "Test Store Receipt",
                EndDate = DateTime.Now.AddDays(30)
            };

            _mockFileService
                .Setup(s => s.ExtractDataWithProcessor(fileName, "image/jpeg"))
                .ReturnsAsync(billData);

            // Act
            var result = await _controller.ExtractData(fileName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var extractedData = Assert.IsType<BillData>(okResult.Value);
            Assert.Equal(billData.TotalPrice, extractedData.TotalPrice);
            Assert.Equal(billData.Description, extractedData.Description);
        }

        [Theory]
        [InlineData("receipt.jpg", "image/jpeg")]
        [InlineData("receipt.jpeg", "image/jpeg")]
        [InlineData("receipt.png", "image/png")]
        [InlineData("receipt.pdf", "application/pdf")]
        [InlineData("receipt.txt", "application/octet-stream")]
        public async Task DownloadReceipt_WithDifferentFileTypes_ShouldReturnCorrectContentType(string fileName, string expectedContentType)
        {
            // Arrange
            var fileContent = new byte[] { 1, 2, 3, 4, 5 };
            
            _mockFileService
                .Setup(s => s.GetFileAsync(fileName))
                .ReturnsAsync(fileContent);

            // Act
            var result = await _controller.DownloadReceipt(fileName);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal(expectedContentType, fileResult.ContentType);
        }

        private IFormFile CreateTestFormFile(string fileName, string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            var file = new Mock<IFormFile>();
            
            file.Setup(f => f.Length).Returns(bytes.Length);
            file.Setup(f => f.FileName).Returns(fileName);
            file.Setup(f => f.OpenReadStream()).Returns(stream);
            file.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            return file.Object;
        }
    }
}
