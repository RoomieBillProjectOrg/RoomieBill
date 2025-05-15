using Microsoft.AspNetCore.Http;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.Services.Interfaces
{
    public interface IFileUploadService
    {
        Task<string> SaveFileAsync(IFormFile file);
        Task<byte[]> GetFileAsync(string fileNameWithGuid);
        Task<BillData> ExtractDataWithProcessor(string fileName, string contentType);
    }
}
