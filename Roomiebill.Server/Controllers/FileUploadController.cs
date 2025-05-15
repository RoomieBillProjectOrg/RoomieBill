using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Roomiebill.Server.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Roomiebill.Server.Controllers
{
    /// <summary>
    /// Handles file upload and download operations for expense receipts.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly FileUploadService _fileStorageService;

        public UploadController(FileUploadService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        /// <summary>
        /// Uploads a receipt file and returns the generated unique filename.
        /// </summary>
        /// <param name="file">The receipt file to upload.</param>
        /// <returns>The generated unique filename for the uploaded file.</returns>
        /// <response code="200">Returns the generated filename.</response>
        /// <response code="400">If the file upload fails.</response>
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

        /// <summary>
        /// Downloads a receipt file by its filename.
        /// </summary>
        /// <param name="fileName">The unique filename of the receipt to download.</param>
        /// <returns>The receipt file content.</returns>
        /// <response code="200">Returns the file content.</response>
        /// <response code="404">If the file is not found.</response>
        /// <response code="400">If the file download fails.</response>
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

                // Get the content type based on the file extension
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
    }
}
