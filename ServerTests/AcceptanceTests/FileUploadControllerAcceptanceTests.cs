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
        private const int MaxFileSize = 10 * 1024 * 1024; // 10MB
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
            var expectedGuid = $"unique-guid-{fileName}";

            _mockFileService
                .Setup(s => s.SaveFileAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync(expectedGuid);

            // Act
            var result = await _controller.UploadReceipt(file);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<FileUploadResponse>(okResult.Value);
            Assert.Equal(expectedGuid, response.FileName);
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

        [Theory]
        [InlineData("receipt.exe")]
        [InlineData("receipt.bat")]
        [InlineData("receipt.sh")]
        public async Task UploadReceipt_WithUnsupportedFileType_ShouldReturnBadRequest(string fileName)
        {
            // Arrange
            var file = CreateTestFormFile(fileName, "fake content");

            _mockFileService
                .Setup(s => s.SaveFileAsync(It.IsAny<IFormFile>()))
                .ThrowsAsync(new ArgumentException("Unsupported file type"));

            // Act
            var result = await _controller.UploadReceipt(file);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Unsupported file type", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task UploadReceipt_WhenServiceThrows_ShouldReturnBadRequest()
        {
            // Arrange
            var file = CreateTestFormFile("receipt.jpg", "content");
            _mockFileService
                .Setup(s => s.SaveFileAsync(It.IsAny<IFormFile>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.UploadReceipt(file);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Unexpected error", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task UploadReceipt_WithLargeFile_ShouldReturnBadRequest()
        {
            // Arrange
            var file = new Mock<IFormFile>();
            file.Setup(f => f.Length).Returns(MaxFileSize + 1);
            file.Setup(f => f.FileName).Returns("large.jpg");
            file.Setup(f => f.ContentType).Returns("image/jpeg");
            file.Setup(f => f.Name).Returns("file");
            file.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[MaxFileSize + 1]));

            // Act
            var result = await _controller.UploadReceipt(file.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("too large", badRequestResult.Value.ToString().ToLower());
        }

        [Theory]
        [InlineData("receipt.pdf")]
        [InlineData("receipt.png")]
        public async Task UploadReceipt_WithSupportedFileTypes_ShouldSucceed(string fileName)
        {
            // Arrange
            var file = CreateTestFormFile(fileName, "pdf or png content");
            var expectedGuid = $"guid-{fileName}";
            _mockFileService
                .Setup(s => s.SaveFileAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync(expectedGuid);

            // Act
            var result = await _controller.UploadReceipt(file);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<FileUploadResponse>(okResult.Value);
            Assert.Equal(expectedGuid, response.FileName);
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
        public async Task DownloadReceipt_WhenServiceThrows_ShouldReturnBadRequest()
        {
            // Arrange
            var fileName = "receipt.jpg";
            _mockFileService
                .Setup(s => s.GetFileAsync(fileName))
                .ThrowsAsync(new Exception("Disk error"));

            // Act
            var result = await _controller.DownloadReceipt(fileName);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Disk error", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task DownloadReceipt_WithEmptyFileName_ShouldReturnNotFound()
        {
            // Arrange
            string fileName = "";

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
            var currentDate = DateTime.Now;
            var billData = new BillData 
            { 
                TotalPrice = 100.50m,
                StartDate = currentDate,
                Description = "Test Store Receipt",
                EndDate = currentDate.AddDays(30)
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
            Assert.Equal(billData.StartDate, extractedData.StartDate);
            Assert.Equal(billData.EndDate, extractedData.EndDate);
        }

        [Fact]
        public async Task ExtractData_WithUnreadableFile_ShouldReturnBadRequest()
        {
            // Arrange
            var fileName = "corrupted.jpg";
            
            _mockFileService
                .Setup(s => s.ExtractDataWithProcessor(fileName, "image/jpeg"))
                .ThrowsAsync(new Exception("Unable to read file content"));

            // Act
            var result = await _controller.ExtractData(fileName);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("unable to read", badRequestResult.Value.ToString().ToLower());
        }

        [Fact]
        public async Task ExtractData_WithNullFileName_ShouldReturnBadRequest()
        {
            // Arrange
            string fileName = null;

            // Act
            var result = await _controller.ExtractData(fileName);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("invalid", badRequestResult.Value.ToString().ToLower());
        }

        [Fact]
        public async Task ExtractData_WhenServiceThrows_ShouldReturnBadRequest()
        {
            // Arrange
            var fileName = "receipt.jpg";
            _mockFileService
                .Setup(s => s.ExtractDataWithProcessor(fileName, "image/jpeg"))
                .ThrowsAsync(new Exception("Processing error"));

            // Act
            var result = await _controller.ExtractData(fileName);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("processing error", badRequestResult.Value.ToString().ToLower());
        }

        [Fact]
        public async Task ExtractData_WithUnsupportedFileType_ShouldReturnBadRequest()
        {
            // Arrange
            var fileName = "receipt.txt";
            _mockFileService
                .Setup(s => s.ExtractDataWithProcessor(fileName, "application/octet-stream"))
                .ThrowsAsync(new ArgumentException("Unsupported file type"));

            // Act
            var result = await _controller.ExtractData(fileName);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("unsupported file type", badRequestResult.Value.ToString().ToLower());
        }

        private IFormFile CreateTestFormFile(string fileName, string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            
            var file = new Mock<IFormFile>();
            var ms = new MemoryStream(bytes);
            
            file.Setup(f => f.Length).Returns(bytes.Length);
            file.Setup(f => f.FileName).Returns(fileName);
            file.Setup(f => f.OpenReadStream()).Returns(ms);
            file.Setup(f => f.ContentType).Returns(GetContentType(fileName));
            file.Setup(f => f.Name).Returns("file");
            file.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, token) => {
                    ms.CopyTo(stream);
                })
                .Returns(Task.CompletedTask);

            return file.Object;
        }

        private string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }
    }
}
