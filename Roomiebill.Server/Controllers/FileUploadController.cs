using Google.Cloud.DocumentAI.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Roomiebill.Server.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Roomiebill.Server.Controllers{

    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly FileUploadService _fileStorageService;

        public UploadController(FileUploadService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadReceipt(IFormFile file)
        {
            try
            {
                string fileNameWithGuid = await _fileStorageService.SaveFileAsync(file);
                return Ok(new { FileName = fileNameWithGuid });
            }
            catch (Exception ex)
            {
                return BadRequest($"Upload failed: {ex.Message}");
            }
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadReceipt(string fileName)
        {
            try
            {
                byte[] receiptBytes = await _fileStorageService.GetFileAsync(fileName);
                if (receiptBytes == null || receiptBytes.Length == 0)
                {
                    return NotFound("File not found.");
                }

                // Get the content type based on the file extension (optional)
                string contentType = "application/octet-stream"; // Default
                string extension = Path.GetExtension(fileName).ToLower();
                if (extension == ".jpg" || extension == ".jpeg") contentType = "image/jpeg";
                if (extension == ".png") contentType = "image/png";
                if (extension == ".pdf") contentType = "application/pdf";

                return File(receiptBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error downloading file: {ex.Message}");
            }
        }

        [HttpGet("extract/{fileName}")]
        public async Task<IActionResult> ExtractData(string fileName)
        {
            try
            {
                // Get the content type based on the file extension (optional)
                string contentType = "application/octet-stream"; // Default
                string extension = Path.GetExtension(fileName).ToLower();
                if (extension == ".jpg" || extension == ".jpeg") contentType = "image/jpeg";
                if (extension == ".png") contentType = "image/png";
                if (extension == ".pdf") contentType = "application/pdf";

                Document document = await _fileStorageService.ExtractDataWithProcessor(fileName, contentType);
                return Ok(document.Text);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error downloading file: {ex.Message}");
            }
        }
    }
}

